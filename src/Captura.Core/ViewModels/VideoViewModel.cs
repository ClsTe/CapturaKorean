using System;
using System.Collections.Generic;
using Captura.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;

namespace Captura.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class VideoViewModel : ViewModelBase
    {
        readonly IRegionProvider _regionProvider;
        readonly FullScreenSourceProvider _fullScreenProvider;

        // To prevent deselection or cancelling selection
        readonly SynchronizationContext _syncContext = SynchronizationContext.Current;

        public NoVideoSourceProvider NoVideoSourceProvider { get; }

        public ObservableCollection<VideoSourceModel> VideoSources { get; } = new ObservableCollection<VideoSourceModel>();

        public ObservableCollection<IVideoWriterProvider> VideoWriterProviders { get; } = new ObservableCollection<IVideoWriterProvider>();

        const string NoVideoDescription = @"영상이 녹화되지 않습니다.
소리 전용 녹음에 사용할 수 있습니다.
소리 소스가 활성화되어 있는지 확인하십시오.";

        const string FullScreenDescription = "전체화면을 녹화합니다.";

        const string ScreenDescription = "특정 모니터 화면을 녹화합니다.";

        const string WindowDescription = @"특정 윈도우를 녹화합니다.
영상의 크기는 창의 처음 크기와 같습니다.";

        const string RegionDescription = "영역 선택기를 통해 특정 영역을 녹화합니다";

        const string DeskDuplDescription = @"전체화면인 DirectX 게임 등의 화면 녹화를 위한 빠른 API입니다.
모든 게임을 녹화 할 수있는 것은 아닙니다.
Windows 8 이상이 필요합니다.
작동하지 않으면 통합 그래픽 카드에서 Captura를 실행해봅니다.";

        public VideoViewModel(IRegionProvider RegionProvider,
            IEnumerable<IImageWriterItem> ImageWriters,
            Settings Settings,
            LanguageManager LanguageManager,
            FullScreenSourceProvider FullScreenProvider,
            IIconSet Icons,
            // ReSharper disable SuggestBaseTypeForParameter
            ScreenSourceProvider ScreenSourceProvider,
            WindowSourceProvider WindowSourceProvider,
            RegionSourceProvider RegionSourceProvider,
            NoVideoSourceProvider NoVideoSourceProvider,
            DeskDuplSourceProvider DeskDuplSourceProvider,
            FFmpegWriterProvider FFmpegWriterProvider,
            SharpAviWriterProvider SharpAviWriterProvider,
            GifWriterProvider GifWriterProvider,
            StreamingWriterProvider StreamingWriterProvider,
            DiscardWriterProvider DiscardWriterProvider
            // ReSharper restore SuggestBaseTypeForParameter
            ) : base(Settings, LanguageManager)
        {
            this.NoVideoSourceProvider = NoVideoSourceProvider;

            AvailableVideoWriters = new ReadOnlyObservableCollection<IVideoWriterItem>(_videoWriters);

            AvailableImageWriters = new ReadOnlyObservableCollection<IImageWriterItem>(_imageWriters);

            _regionProvider = RegionProvider;
            _fullScreenProvider = FullScreenProvider;

            VideoSources.Add(new VideoSourceModel(NoVideoSourceProvider, nameof(Loc.OnlyAudio), NoVideoDescription, Icons.Video));
            VideoSources.Add(new VideoSourceModel(FullScreenProvider, nameof(Loc.FullScreen), FullScreenDescription, Icons.MultipleMonitor));
            VideoSources.Add(new VideoSourceModel(ScreenSourceProvider, nameof(Loc.Screen), ScreenDescription, Icons.Screen));
            VideoSources.Add(new VideoSourceModel(WindowSourceProvider, nameof(Loc.Window), WindowDescription, Icons.Window));
            VideoSources.Add(new VideoSourceModel(RegionSourceProvider, nameof(Loc.Region), RegionDescription, Icons.Region));

            if (Windows8OrAbove)
            {
                VideoSources.Add(new VideoSourceModel(DeskDuplSourceProvider, "Desktop Duplication", DeskDuplDescription, Icons.Game));
            }

            VideoWriterProviders.Add(FFmpegWriterProvider);
            VideoWriterProviders.Add(GifWriterProvider);
            VideoWriterProviders.Add(SharpAviWriterProvider);
            // VideoWriterProviders.Add(StreamingWriterProvider);
            // VideoWriterProviders.Add(DiscardWriterProvider);

            foreach (var imageWriter in ImageWriters)
            {
                _imageWriters.Add(imageWriter);
            }

            SetDefaultSource();

            if (!AvailableImageWriters.Any(M => M.Active))
                AvailableImageWriters[0].Active = true;

            SelectedVideoWriterKind = FFmpegWriterProvider;
        }

        public bool Windows8OrAbove
        {
            get
            {
                // All versions above Windows 8 give the same version number
                var version = new Version(6, 2, 9200, 0);

                return Environment.OSVersion.Platform == PlatformID.Win32NT &&
                       Environment.OSVersion.Version >= version;
            }
        }

        void SetDeskDuplSource(DeskDuplSourceProvider DeskDuplSourceProvider)
        {
            // Select first screen if there is only one
            if (ScreenItem.Count == 1 && DeskDuplSourceProvider.SelectFirst())
            {
                _videoSourceKind = DeskDuplSourceProvider;
            }
            else
            {
                if (DeskDuplSourceProvider.PickScreen())
                {
                    _videoSourceKind = DeskDuplSourceProvider;
                }
            }
        }

        void SetScreenSource(ScreenSourceProvider ScreenSourceProvider)
        {
            // Select first screen if there is only one
            if (ScreenItem.Count == 1)
            {
                ScreenSourceProvider.Set(0);
                _videoSourceKind = ScreenSourceProvider;
            }
            else
            {
                if (ScreenSourceProvider.PickScreen())
                {
                    _videoSourceKind = ScreenSourceProvider;
                }
            }
        }

        public void SetDefaultSource()
        {
            SelectedVideoSourceKind = _fullScreenProvider;
        }

        public void Init()
        {                                               
            RefreshCodecs();

            RefreshVideoSources();
            
            _regionProvider.SelectorHidden += () =>
            {
                if (SelectedVideoSourceKind is RegionSourceProvider)
                    SetDefaultSource();
            };
        }

        void RefreshVideoSources()
        {
            // RegionSelector should only be shown on Region Capture.
            _regionProvider.SelectorVisible = SelectedVideoSourceKind is RegionSourceProvider;
        }

        public void RefreshCodecs()
        {
            // Available Codecs
            _videoWriters.Clear();

            foreach (var writerItem in SelectedVideoWriterKind)
            {
                _videoWriters.Add(writerItem);
            }

            if (_videoWriters.Count > 0)
                SelectedVideoWriter = _videoWriters[0];
        }

        readonly ObservableCollection<IVideoWriterItem> _videoWriters = new ObservableCollection<IVideoWriterItem>();

        public ReadOnlyObservableCollection<IVideoWriterItem> AvailableVideoWriters { get; }
        
        IVideoWriterProvider _writerKind;

        public IVideoWriterProvider SelectedVideoWriterKind
        {
            get => _writerKind;
            set
            {
                if (_writerKind == value)
                    return;

                if (value != null)
                    _writerKind = value;

                if (_syncContext != null)
                    _syncContext.Post(S => RaisePropertyChanged(nameof(SelectedVideoWriterKind)), null);
                else OnPropertyChanged();

                RefreshCodecs();
            }
        }

        IVideoSourceProvider _videoSourceKind;

        public IVideoSourceProvider SelectedVideoSourceKind
        {
            get => _videoSourceKind;
            set
            {
                if (_videoSourceKind == value)
                    return;

                switch (value)
                {
                    case ScreenSourceProvider screenSourceProvider:
                        SetScreenSource(screenSourceProvider);
                        break;

                    case DeskDuplSourceProvider deskDuplSourceProvider:
                        SetDeskDuplSource(deskDuplSourceProvider);
                        break;

                    case WindowSourceProvider windowSourceProvider:
                        if (windowSourceProvider.PickWindow())
                        {
                            _videoSourceKind = windowSourceProvider;
                        }
                        break;

                    default:
                        if (value != null)
                            _videoSourceKind = value;
                        break;
                }

                RefreshVideoSources();

                if (_syncContext != null)
                {
                    _syncContext.Post(S => RaisePropertyChanged(nameof(SelectedVideoSourceKind)), null);
                }
                else OnPropertyChanged();
            }
        }

        public void RestoreSourceKind(IVideoSourceProvider SourceProvider)
        {
            _videoSourceKind = SourceProvider;
        }

        IVideoWriterItem _writer;

        public IVideoWriterItem SelectedVideoWriter
        {
            get => _writer;
            set
            {
                _writer = value ?? (AvailableVideoWriters.Count == 0 ? null : AvailableVideoWriters[0]);

                OnPropertyChanged();
            }
        }

        readonly ObservableCollection<IImageWriterItem> _imageWriters = new ObservableCollection<IImageWriterItem>();

        public ReadOnlyObservableCollection<IImageWriterItem> AvailableImageWriters { get; }
    }
}
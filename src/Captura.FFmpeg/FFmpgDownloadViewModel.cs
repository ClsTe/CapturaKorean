using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Captura.Models;

namespace Captura.ViewModels
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class FFmpegDownloadViewModel : NotifyPropertyChanged
    {
        public DelegateCommand StartCommand { get; }

        public DelegateCommand SelectFolderCommand { get; }

        public ICommand OpenFolderCommand { get; }

        readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        bool _isDownloading;

        public bool IsDownloading
        {
            get => _isDownloading;
            set
            {
                _isDownloading = value;
                
                OnPropertyChanged();
            }
        }

        readonly IDialogService _dialogService;
        readonly ProxySettings _proxySettings;
        readonly FFmpegSettings _ffmpegSettings;
        readonly LanguageManager _loc;

        public FFmpegDownloadViewModel(IDialogService DialogService, ProxySettings ProxySettings, LanguageManager Loc, FFmpegSettings FFmpegSettings)
        {
            _dialogService = DialogService;
            _proxySettings = ProxySettings;
            _loc = Loc;
            _ffmpegSettings = FFmpegSettings;

            StartCommand = new DelegateCommand(OnStartExecute);

            SetDefaultTargetFolderToLocalAppData();

            SelectFolderCommand = new DelegateCommand(OnSelectFolderExecute);

            OpenFolderCommand = new DelegateCommand(() =>
            {
                if (Directory.Exists(_targetFolder))
                {
                    Process.Start(_targetFolder);
                }
            });
        }

        void SetDefaultTargetFolderToLocalAppData()
        {
            if (!string.IsNullOrWhiteSpace(_ffmpegSettings.FolderPath))
            {
                _targetFolder = _ffmpegSettings.FolderPath;
            }
            else
            {
                var localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

                _targetFolder = Path.Combine(localAppDataPath, "Captura");
            }

            if (!Directory.Exists(_targetFolder))
            {
                Directory.CreateDirectory(_targetFolder);
            }
        }

        async void OnStartExecute()
        {
            IsDownloading = true;

            try
            {
                var result = await Start();

                AfterDownload?.Invoke(result);
            }
            finally
            {
                IsDownloading = false;
            }
        }

        void OnSelectFolderExecute()
        {
            var folder = _dialogService.PickFolder(TargetFolder, _loc.SelectFFmpegFolder);

            if (!string.IsNullOrWhiteSpace(folder))
                TargetFolder = folder;
        }

        const string CancelDownload = "다운로드 취소";
        const string StartDownload = "다운로드 시작";
        const string Finish = "다운로드 완료";

        public Action CloseWindowAction;

        public event Action<int> ProgressChanged;

        public event Action<bool> AfterDownload;
        
        public async Task<bool> Start()
        {
            switch (ActionDescription)
            {
                case CancelDownload:
                    _cancellationTokenSource.Cancel();

                    CloseWindowAction.Invoke();
                
                    return false;

                case Finish:
                    CloseWindowAction?.Invoke();

                    return true;
            }

            ActionDescription = CancelDownload;

            Status = "다운로드중";

            try
            {
                await DownloadFFmpeg.DownloadArchive(P =>
                {
                    Progress = P;

                    Status = $"다운로드중 ({P}%)";

                    ProgressChanged?.Invoke(P);
                }, _proxySettings.GetWebProxy(), _cancellationTokenSource.Token);
            }
            catch (WebException webException) when(webException.Status == WebExceptionStatus.RequestCanceled)
            {
                Status = "취소됨";
                return false;
            }
            catch (Exception e)
            {
                Status = $"실패 - {e.Message}";
                return false;
            }

            _cancellationTokenSource.Dispose();

            // No cancelling after download
            StartCommand.RaiseCanExecuteChanged(false);
            
            Status = "압축을 푸는 중";

            try
            {
                await DownloadFFmpeg.ExtractTo(TargetFolder);
            }
            catch (UnauthorizedAccessException)
            {
                Status = "해당 폴더에 압출을 풀 수 없습니다.";
                return false;
            }
            catch
            {
                Status = "압축 풀기 실패";
                return false;
            }
            
            // Update FFmpeg folder setting
            _ffmpegSettings.FolderPath = TargetFolder;
            
            Status = "완료";
            ActionDescription = Finish;

            StartCommand.RaiseCanExecuteChanged(true);

            return true;
        }

        string _actionDescription = StartDownload;

        public string ActionDescription
        {
            get => _actionDescription;
            private set
            {
                _actionDescription = value;

                OnPropertyChanged();
            }
        }

        string _targetFolder;

        public string TargetFolder
        {
            get => _targetFolder;
            set
            {
                _targetFolder = value;

                OnPropertyChanged();
            }
        }

        int _progress;

        public int Progress
        {
            get => _progress;
            private set
            {
                _progress = value;

                OnPropertyChanged();
            }
        }

        string _status = "준비";

        public string Status
        {
            get => _status;
            private set
            {
                _status = value;

                OnPropertyChanged();
            }
        }
    }
}

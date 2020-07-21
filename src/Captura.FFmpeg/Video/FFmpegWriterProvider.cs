using System.Collections;
using System.Collections.Generic;

namespace Captura.Models
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class FFmpegWriterProvider : IVideoWriterProvider
    {
        public string Name => "FFmpeg";

        readonly FFmpegSettings _settings;

        public FFmpegWriterProvider(FFmpegSettings Settings)
        {
            _settings = Settings;
        }

        public IEnumerator<IVideoWriterItem> GetEnumerator()
        {
            foreach (var codec in FFmpegItem.Items)
            {
                yield return codec;
            }

            foreach (var codec in FFmpegPostProcessingItem.Items)
            {
                yield return codec;
            }

            foreach (var item in _settings.CustomCodecs)
            {
                yield return new FFmpegItem(item);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public override string ToString() => Name;

        public string Description => @"인코딩에는 FFmpeg를 사용합니다.
ffmpeg.exe를 다운로드하거나 경로를 지정해야합니다.";
    }
}
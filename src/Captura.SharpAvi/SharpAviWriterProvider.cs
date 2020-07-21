using SharpAvi.Codecs;
using System.Collections;
using System.Collections.Generic;

namespace Captura.Models
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class SharpAviWriterProvider : IVideoWriterProvider
    {
        public string Name => "SharpAvi";

        public IEnumerator<IVideoWriterItem> GetEnumerator()
        {
            yield return new SharpAviItem(AviCodec.MotionJpeg, "WPF의 JPG 인코더를 사용하는 모션 JPEG 인코더");
            yield return new SharpAviItem(AviCodec.Uncompressed, "비압축 Avi");
            yield return new SharpAviItem(AviCodec.Lagarith, "Lagarith 코덱을 수동으로 설치하고 Null 프레임을 비활성화 한 상태에서 RGB 모드를 사용하도록 설정해야 합니다.");

            foreach (var codec in Mpeg4VideoEncoderVcm.GetAvailableCodecs())
                yield return new SharpAviItem(new AviCodec(codec.Codec, codec.Name), "");
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public override string ToString() => Name;

        public string Description => "SharpAvi를 사용하여 AVI 비디오를 인코딩합니다.";
    }
}
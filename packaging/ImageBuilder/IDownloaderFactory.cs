namespace ImageBuilder;

using Microsoft.Extensions.Logging;
using ImageDownloader;

// IArtifactDownloader 인터페이스를 사용해 다운로더를 생성하는 팩토리 클래스.
// 이 클래스를 테스트하기 위해 인터페이스를 정의합니다.
public interface IDownloaderFactory
{
    IArtifactDownloader CreateDownloader(ILoggerFactory loggerFactory, IArtifactSource source);
}
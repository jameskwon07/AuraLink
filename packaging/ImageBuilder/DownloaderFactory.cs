namespace ImageBuilder;

using ImageDownloader;
using Microsoft.Extensions.Logging;

// IArtifactDownloader 인터페이스를 사용해 다운로더를 생성하는 팩토리 클래스.
// 이 클래스는 프로그램의 아티팩트 소스 타입에 따라 적절한 다운로더를 반환합니다.
public static class DownloaderFactory
{
    public static IArtifactDownloader CreateDownloader(ILoggerFactory loggerFactory, IArtifactSource source)
    {
        switch (source.Type.ToLower())
        {
            case "jenkins":
                return new JenkinsDownloader(loggerFactory.CreateLogger<JenkinsDownloader>(), (JenkinsArtifactSource)source);
            case "github":
                return new GitHubDownloader(loggerFactory.CreateLogger<GitHubDownloader>(), (GitHubArtifactSource)source);
            default:
                throw new NotSupportedException($"지원되지 않는 아티팩트 소스 타입: {source.Type}");
        }
    }
}
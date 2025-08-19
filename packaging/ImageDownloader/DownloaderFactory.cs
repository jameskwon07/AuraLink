namespace ImageDownloader;

using Microsoft.Extensions.Logging;

public class DownloaderFactory : IDownloaderFactory
{
    public IArtifactDownloader CreateDownloader(ILoggerFactory loggerFactory, IArtifactSource source)
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
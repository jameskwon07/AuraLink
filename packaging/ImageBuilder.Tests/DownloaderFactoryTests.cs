namespace ImageBuilder.Tests;

using TestUtils;
using Moq;
using Microsoft.Extensions.Logging;
using ImageDownloader;

// DownloaderFactory 클래스에 대한 유닛 테스트.
public class DownloaderFactoryTests
{
    [Fact]
    public void CreateDownloader_WithJenkinsSource_ReturnsJenkinsDownloader()
    {
        // 준비
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        var source = new JenkinsArtifactSource();

        // 실행
        var downloader = DownloaderFactory.CreateDownloader(mockLoggerFactory.Object, source);

        // 검증
        Assert.IsType<JenkinsDownloader>(downloader);
    }

    [Fact]
    public void CreateDownloader_WithGitHubSource_ReturnsGitHubDownloader()
    {
        // 준비
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        var source = new GitHubArtifactSource();

        // 실행
        var downloader = DownloaderFactory.CreateDownloader(mockLoggerFactory.Object, source);

        // 검증
        Assert.IsType<GitHubDownloader>(downloader);
    }

    [Fact]
    public void CreateDownloader_WithUnsupportedSource_ThrowsException()
    {
        // 준비
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        var source = new Mock<IArtifactSource>();
        source.SetupGet(s => s.Type).Returns("unsupported");
        
        // 실행 & 검증
        Assert.Throws<NotSupportedException>(() => DownloaderFactory.CreateDownloader(mockLoggerFactory.Object, source.Object));
    }
}
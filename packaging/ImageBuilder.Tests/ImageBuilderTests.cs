using ImageDownloader;
using Moq;
using Microsoft.Extensions.Logging;

namespace ImageBuilder.Tests;

// ImageBuilder 클래스에 대한 유닛 테스트.
public class ImageBuilderTests
{
    [Fact]
    public async Task BuildAsync_WithProgramsAndDependencies_CallsDownloadForEach()
    {
        // 준비
        var mockLoggerFactory = new Mock<ILoggerFactory>();
        var mockImageBuilderLogger = new Mock<ILogger<ImageBuilder>>();
        mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockImageBuilderLogger.Object);

        // Mock IArtifactDownloader instances for main program and dependencies
        var mockMainDownloader = new Mock<IArtifactDownloader>();
        mockMainDownloader.Setup(d => d.DownloadArtifactAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
        var mockDependencyADownloader = new Mock<IArtifactDownloader>();
        mockDependencyADownloader.Setup(d => d.DownloadArtifactAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
        var mockDependencyBDownloader = new Mock<IArtifactDownloader>();
        mockDependencyBDownloader.Setup(d => d.DownloadArtifactAsync(It.IsAny<string>())).Returns(Task.CompletedTask);

        // Mock the DownloaderFactory to return the mock downloaders
        var mockDownloaderFactory = new Mock<IDownloaderFactory>();
        mockDownloaderFactory.Setup(f => f.CreateDownloader(
            It.IsAny<ILoggerFactory>(),
            It.Is<IArtifactSource>(s => s.ProgramName == "AuralinkAgent"))
        ).Returns(mockMainDownloader.Object);
        mockDownloaderFactory.Setup(f => f.CreateDownloader(
            It.IsAny<ILoggerFactory>(),
            It.Is<IArtifactSource>(s => s.ProgramName == "ThirdPartyTool-A"))
        ).Returns(mockDependencyADownloader.Object);
        mockDownloaderFactory.Setup(f => f.CreateDownloader(
            It.IsAny<ILoggerFactory>(),
            It.Is<IArtifactSource>(s => s.ProgramName == "ThirdPartyTool-B"))
        ).Returns(mockDependencyBDownloader.Object);

        var source = new JenkinsArtifactSource
        {
            ProgramName = "AuralinkAgent",
            Address = "http://testjenkins.com",
            JobName = "test-job",
            JobIdentifier = "invalid-build",
            ArtifactPath = "build/artifact.zip"
        };
        
        var programs = new List<Program>
        {
            new Program
            {
                Name = "AuralinkAgent",
                ArtifactSource = source,
                ThirdParty = new List<Program>
                {
                    new Program { Name = "ThirdPartyTool-A", ArtifactSource = new GitHubArtifactSource { ProgramName = "ThirdPartyTool-A" } },
                    new Program { Name = "ThirdPartyTool-B", ArtifactSource = new JenkinsArtifactSource { ProgramName = "ThirdPartyTool-B" } }
                }
            }
        };

        var imageBuilder = new ImageBuilder(mockLoggerFactory.Object, mockDownloaderFactory.Object);
        var downloadPath = "test_downloads";

        // 실행
        await imageBuilder.BuildAsync(programs, downloadPath);

        // 검증
        mockMainDownloader.Verify(d => d.DownloadArtifactAsync(It.IsAny<string>()), Times.Once);
        mockDependencyADownloader.Verify(d => d.DownloadArtifactAsync(It.IsAny<string>()), Times.Once);
        mockDependencyBDownloader.Verify(d => d.DownloadArtifactAsync(It.IsAny<string>()), Times.Once);
    }
}
using ImageDownloader;
using Moq;
using Microsoft.Extensions.Logging;
using System.IO.Compression;

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
        // mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<Type>())).Returns(mockImageBuilderLogger.Object);

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
            It.Is<IArtifactSource>(s => s.ProgramName == "TestApp"))
        ).Returns(mockMainDownloader.Object);
        mockDownloaderFactory.Setup(f => f.CreateDownloader(
            It.IsAny<ILoggerFactory>(),
            It.Is<IArtifactSource>(s => s.ProgramName == "DepA"))
        ).Returns(mockDependencyADownloader.Object);
        mockDownloaderFactory.Setup(f => f.CreateDownloader(
            It.IsAny<ILoggerFactory>(),
            It.Is<IArtifactSource>(s => s.ProgramName == "DepB"))
        ).Returns(mockDependencyBDownloader.Object);

        // Mock the ChecksumGenerator
        var mockChecksumGenerator = new Mock<IChecksumGenerator>();
        mockChecksumGenerator.Setup(g => g.GenerateChecksumsFileAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);
        
        var programs = new List<Program>
        {
            new Program
            {
                Name = "TestApp",
                ArtifactSource = new JenkinsArtifactSource { ProgramName = "TestApp" },
                ThirdParty = new List<Program>
                {
                    new Program { Name = "DepA", ArtifactSource = new GitHubArtifactSource { ProgramName = "DepA" } },
                    new Program { Name = "DepB", ArtifactSource = new JenkinsArtifactSource { ProgramName = "DepB" } }
                }
            }
        };
        
        var imageBuilder = new ImageBuilder(mockLoggerFactory.Object, mockDownloaderFactory.Object, mockChecksumGenerator.Object);
        var downloadPath = "test_downloads";
        
        // Ensure clean test environment
        if (Directory.Exists(downloadPath))
        {
            Directory.Delete(downloadPath, true);
        }

        // Execution
        await imageBuilder.BuildAsync(programs, downloadPath);

        // 메인 프로그램 다운로드가 호출되었는지 확인
        mockMainDownloader.Verify(d => d.DownloadArtifactAsync(It.IsAny<string>()), Times.Once);
        
        // 모든 의존성 다운로드가 호출되었는지 확인
        mockDependencyADownloader.Verify(d => d.DownloadArtifactAsync(It.IsAny<string>()), Times.Once);
        mockDependencyBDownloader.Verify(d => d.DownloadArtifactAsync(It.IsAny<string>()), Times.Once);

        // ChecksumGenerator가 호출되었는지 확인
        mockChecksumGenerator.Verify(g => g.GenerateChecksumsFileAsync(
            It.IsAny<string>(),
            It.Is<string>(p => p.Contains("checksums.txt"))
        ), Times.Once);

        // 다운로드 경로가 생성되었고, 마지막에 삭제되었는지 확인 (이 부분은 테스트가 어려워 시각적으로 확인하거나 별도의 테스트로 분리)
        // 이 테스트는 오케스트레이션 로직에 집중하므로, 파일 시스템 직접 검증은 생략합니다.

        // 정리
        if (Directory.Exists(downloadPath))
        {
            Directory.Delete(downloadPath, true);
        }
    }
}
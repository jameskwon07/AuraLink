using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Moq;
using Xunit;
using ImageDownloader;
using TestUtils;


// GitHubDownloader 클래스에 대한 유닛 테스트.
public class GitHubDownloaderTests
{
    [Fact]
    public async Task DownloadArtifactAsync_SuccessfulResponse_ReturnsTrueAndSavesFile()
    {
        // 준비
        var mockLogger = new Mock<ILogger<GitHubDownloader>>();
        var source = new GitHubArtifactSource
        {
            ProgramName = "TestProgram",
            Owner = "test-owner",
            Repo = "test-repo",
            Tag = "v1.0.0",
            ArtifactName = "test-artifact.zip"
        };
        var mockContent = new StringContent("Test file content for GitHub");
        var mockResponse = new HttpResponseMessage(HttpStatusCode.OK) { Content = mockContent };

        var mockHandler = new MockHttpMessageHandler((request, cancellationToken) =>
            Task.FromResult(mockResponse));
        var mockHttpClient = new HttpClient(mockHandler);

        var downloader = new GitHubDownloader(mockHttpClient, mockLogger.Object, source);
        var localFilePath = "test_github_artifact.zip";

        // 실행
        await downloader.DownloadArtifactAsync(localFilePath);

        // 검증
        Assert.True(File.Exists(localFilePath));
        Assert.Equal("Test file content for GitHub", File.ReadAllText(localFilePath));

        // 정리
        File.Delete(localFilePath);
    }

    [Fact]
    public async Task DownloadArtifactAsync_Exception_ReturnsFalse()
    {
        // 준비
        var mockLogger = new Mock<ILogger<GitHubDownloader>>();
        var source = new GitHubArtifactSource
        {
            ProgramName = "TestProgram",
            Owner = "test-owner",
            Repo = "test-repo",
            Tag = "v1.0.0",
            ArtifactName = "test-artifact.zip"
        };
        var mockHandler = new MockHttpMessageHandler((request, cancellationToken) =>
            Task.FromException<HttpResponseMessage>(new Exception("Simulated generic error")));
        var mockHttpClient = new HttpClient(mockHandler);
        var downloader = new GitHubDownloader(mockHttpClient, mockLogger.Object, source);
        var localFilePath = "test_github_artifact.zip";

        // 검증
        await Assert.ThrowsAsync<Exception>(async () => await downloader.DownloadArtifactAsync(localFilePath));
        Assert.False(File.Exists(localFilePath));
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("다운로드 실패")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}

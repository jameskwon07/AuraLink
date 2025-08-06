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
// 이 코드는 Auralink.Downloader 프로젝트를 참조합니다.
using ImageDownloader;

// JenkinsDownloader 클래스에 대한 유닛 테스트.
public class JenkinsDownloaderTests
{
    [Fact]
    public async Task DownloadArtifactAsync_SuccessfulResponse_ReturnsTrueAndSavesFile()
    {
        // 준비: 가짜 HTTP 응답과 로거를 설정합니다.
        var mockLogger = new Mock<ILogger<JenkinsDownloader>>();
        var source = new JenkinsArtifactSource
        {
            ProgramName = "TestProgram",
            Address = "http://testjenkins.com",
            JobName = "test-job",
            JobIdentifier = "lastSuccessfulBuild",
            ArtifactPath = "build/artifact.zip"
        };
        var mockContent = new StringContent("Test file content");
        var mockResponse = new HttpResponseMessage(HttpStatusCode.OK) { Content = mockContent };
        
        var mockHandler = new MockHttpMessageHandler((request, cancellationToken) =>
            Task.FromResult(mockResponse));
        var mockHttpClient = new HttpClient(mockHandler);

        var downloader = new JenkinsDownloader(mockHttpClient, mockLogger.Object, source);
        var localFilePath = "test_jenkins_artifact.zip";

        // 실행
        var result = await downloader.DownloadArtifactAsync(localFilePath);

        // 검증
        Assert.True(result);
        Assert.True(File.Exists(localFilePath));
        Assert.Equal("Test file content", File.ReadAllText(localFilePath));

        // 정리
        File.Delete(localFilePath);
    }

    [Fact]
    public async Task DownloadArtifactAsync_HttpRequestException_ReturnsFalse()
    {
        // 준비
        var mockLogger = new Mock<ILogger<JenkinsDownloader>>();
        var source = new JenkinsArtifactSource
        {
            ProgramName = "TestProgram",
            Address = "http://testjenkins.com",
            JobName = "test-job",
            JobIdentifier = "invalid-build",
            ArtifactPath = "build/artifact.zip"
        };
        var mockHandler = new MockHttpMessageHandler((request, cancellationToken) =>
            Task.FromException<HttpResponseMessage>(new HttpRequestException("Simulated network error")));
        var mockHttpClient = new HttpClient(mockHandler);
        var downloader = new JenkinsDownloader(mockHttpClient, mockLogger.Object, source);
        var localFilePath = "test_jenkins_artifact.zip";

        // 실행
        var result = await downloader.DownloadArtifactAsync(localFilePath);

        // 검증
        Assert.False(result);
        Assert.False(File.Exists(localFilePath));
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("HTTP 요청 오류")),
                It.IsAny<HttpRequestException>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
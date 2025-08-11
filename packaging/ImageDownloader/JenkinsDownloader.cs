using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace ImageDownloader
{
    
    // 젠킨스에서 아티팩트를 다운로드하는 구체적인 클래스.
    // IArtifactDownloader 인터페이스를 구현합니다.
    public class JenkinsDownloader : IArtifactDownloader
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<JenkinsDownloader> _logger;
        private readonly JenkinsArtifactSource _source;

        public JenkinsDownloader(ILogger<JenkinsDownloader> logger, JenkinsArtifactSource source)
        {
            _httpClient = new HttpClient();
            _logger = logger;
            _source = source;
        }

        // HttpClient를 모킹하기 위해 테스트용 생성자를 추가합니다.
        public JenkinsDownloader(HttpClient httpClient, ILogger<JenkinsDownloader> logger, JenkinsArtifactSource source)
        {
            _httpClient = httpClient;
            _logger = logger;
            _source = source;
        }

        public async Task DownloadArtifactAsync(string localSavePath)
        {
            _logger.LogInformation($"[{_source.ProgramName}] 젠킨스 아티팩트 다운로드 시작. 빌드 식별자: {_source.JobIdentifier}");

            try
            {
                var artifactUrl = BuildArtifactUrl(_source);
                _logger.LogDebug($"다운로드 URL: {artifactUrl}");

                HttpResponseMessage response = await _httpClient.GetAsync(artifactUrl, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                await using (var fileStream = new FileStream(localSavePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await response.Content.CopyToAsync(fileStream);
                }

                _logger.LogInformation($"[{_source.ProgramName}] 젠킨스 아티팩트 다운로드 완료: {localSavePath}");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"[{_source.ProgramName}] 아티팩트 다운로드 실패: HTTP 요청 오류.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{_source.ProgramName}] 예상치 못한 오류로 아티팩트 다운로드 실패.");
                throw;
            }
        }

        private string BuildArtifactUrl(JenkinsArtifactSource source)
        {
            var baseUri = new Uri(source.Address);
            var uriBuilder = new UriBuilder(baseUri);
            uriBuilder.Path = $"{baseUri.AbsolutePath.TrimEnd('/')}/job/{source.JobName}/{source.JobIdentifier}/artifact/{source.ArtifactPath.TrimStart('/')}";
            return uriBuilder.ToString();
        }
    }

    // 젠킨스 아티팩트 소스 정보를 나타내는 클래스.
    public class JenkinsArtifactSource : IArtifactSource
    {
        public string Type { get; } = "jenkins";
        public string ProgramName { get; set; }
        public string Address { get; set; }
        public string JobName { get; set; }
        public string JobIdentifier { get; set; } // 빌드 번호나 "lastSuccessfulBuild" 같은 별칭
        public string ArtifactPath { get; set; }
    }

}

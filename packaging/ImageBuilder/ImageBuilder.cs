namespace ImageBuilder;

using Microsoft.Extensions.Logging;
using ImageDownloader;

// ImageBuilder 클래스: 설정 파일을 읽어 다운로드를 오케스트레이션합니다.
public class ImageBuilder
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<ImageBuilder> _logger;

    private readonly IDownloaderFactory _downloaderFactory;

    public ImageBuilder(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
        _logger = _loggerFactory.CreateLogger<ImageBuilder>();
    }

    public ImageBuilder(ILoggerFactory loggerFactory, IDownloaderFactory downloaderFactory)
    {
        _loggerFactory = loggerFactory;
        _logger = _loggerFactory.CreateLogger<ImageBuilder>();
        _downloaderFactory = downloaderFactory;
    }    

    public async Task BuildAsync(List<Program> programs, string rootDownloadPath)
    {
        _logger.LogInformation("골든 이미지 빌드를 시작합니다.");

        // 다운로드 경로가 없으면 생성합니다.
        Directory.CreateDirectory(rootDownloadPath);

        try
        {
            foreach (var program in programs)
            {
                _logger.LogInformation($"프로그램 '{program.Name}' 및 의존성 다운로드 시작.");
                await DownloadProgramAndDependencies(program, rootDownloadPath);
                _logger.LogInformation($"프로그램 '{program.Name}' 다운로드 완료.");
            }

            _logger.LogInformation("골든 이미지 빌드 과정이 완료되었습니다.");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError($"다운로드에 실패하였습니다. {ex}");
        }

    }

    private async Task DownloadProgramAndDependencies(Program program, string rootDownloadPath)
    {
        // 1. 메인 프로그램 아티팩트 다운로드
        _logger.LogInformation($"메인 프로그램 '{program.Name}' 아티팩트 다운로드 중...");
        IArtifactDownloader mainDownloader = _downloaderFactory.CreateDownloader(_loggerFactory, program.ArtifactSource);
        string mainSavePath = Path.Combine(rootDownloadPath, $"{program.Name}_main.zip");
        await mainDownloader.DownloadArtifactAsync(mainSavePath);

        // 2. 의존성 프로그램 아티팩트 다운로드
        if (program.ThirdParty != null && program.ThirdParty.Any())
        {
            _logger.LogInformation($"프로그램 '{program.Name}'의 의존성 다운로드 시작.");
            foreach (var dependency in program.ThirdParty)
            {
                _logger.LogInformation($"의존성 '{dependency.Name}' 아티팩트 다운로드 중...");
                IArtifactDownloader dependencyDownloader = _downloaderFactory.CreateDownloader(_loggerFactory, dependency.ArtifactSource);
                string dependencySavePath = Path.Combine(rootDownloadPath, $"{program.Name}_{dependency.Name}_dependency.zip");
                await dependencyDownloader.DownloadArtifactAsync(dependencySavePath);
            }
        }
    }
}
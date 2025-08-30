namespace ImageBuilder;

using Microsoft.Extensions.Logging;
using ImageDownloader;
using System.Security.Cryptography;
using System.IO.Compression;

// ImageBuilder 클래스: 설정 파일을 읽어 다운로드를 오케스트레이션합니다.
public class ImageBuilder
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<ImageBuilder> _logger;

    private readonly IDownloaderFactory _downloaderFactory;
    private readonly IChecksumGenerator _checksumGenerator;

    // 생성자를 통해 의존성을 주입받도록 변경되었습니다.
    public ImageBuilder(ILoggerFactory loggerFactory, IDownloaderFactory downloaderFactory, IChecksumGenerator checksumGenerator)
    {
        _loggerFactory = loggerFactory;
        _logger = _loggerFactory.CreateLogger<ImageBuilder>();
        _downloaderFactory = downloaderFactory;
        _checksumGenerator = checksumGenerator;
    }

    public async Task BuildAsync(List<Program> programs, string rootDownloadPath)
    {
        _logger.LogInformation("골든 이미지 빌드를 시작합니다.");

        // 다운로드 경로가 없으면 생성합니다.
        Directory.CreateDirectory(rootDownloadPath);

        foreach (var program in programs)
        {
            _logger.LogInformation($"프로그램 '{program.Name}' 및 의존성 다운로드 시작.");
            
            // 임시 다운로드 경로 생성
            var tempDownloadPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDownloadPath);
            
            // 모든 아티팩트 다운로드
            await DownloadProgramAndDependencies(program, tempDownloadPath);
            
            // 해시값 계산 및 파일로 저장
            var checksumsPath = Path.Combine(tempDownloadPath, "checksums.txt");
            await _checksumGenerator.GenerateChecksumsFileAsync(tempDownloadPath, checksumsPath);

            // 다운로드된 모든 파일과 checksums.txt를 하나의 zip 파일로 압축
            var finalZipPath = Path.Combine(rootDownloadPath, $"{program.Name}_package.zip");
            _logger.LogInformation($"모든 아티팩트를 '{finalZipPath}'로 압축 중...");
            ZipFile.CreateFromDirectory(tempDownloadPath, finalZipPath);
            
            _logger.LogInformation($"프로그램 '{program.Name}' 패키지 생성 완료.");
            
            // 임시 폴더 삭제
            Directory.Delete(tempDownloadPath, true);
        }

    }

    private async Task DownloadProgramAndDependencies(Program program, string rootDownloadPath)
    {
        // 1. 메인 프로그램 아티팩트 다운로드
        _logger.LogInformation($"메인 프로그램 '{program.Name}' 아티팩트 다운로드 중...");
        IArtifactDownloader mainDownloader = _downloaderFactory.CreateDownloader(_loggerFactory, program.ArtifactSource);
        // string mainSavePath = Path.Combine(rootDownloadPath, $"{program.Name}");
        await mainDownloader.DownloadArtifactAsync(rootDownloadPath);

        // 2. 의존성 프로그램 아티팩트 다운로드
        if (program.ThirdParty != null && program.ThirdParty.Any())
        {
            _logger.LogInformation($"프로그램 '{program.Name}'의 의존성 다운로드 시작.");
            foreach (var dependency in program.ThirdParty)
            {
                _logger.LogInformation($"의존성 '{dependency.Name}' 아티팩트 다운로드 중...");
                IArtifactDownloader dependencyDownloader = _downloaderFactory.CreateDownloader(_loggerFactory, dependency.ArtifactSource);
                string dependencySavePath = Path.Combine(rootDownloadPath, $"{program.Name}_{dependency.Name}");
                await dependencyDownloader.DownloadArtifactAsync(dependencySavePath);
            }
        }
    }
    
    // 주어진 폴더의 모든 파일에 대한 SHA-256 해시를 계산하고 파일에 저장합니다.
    private async Task GenerateChecksumsFile(string directoryPath, string outputFilePath)
    {
        _logger.LogInformation("파일 해시값 계산 및 체크섬 파일 생성 중...");
        var fileList = Directory.EnumerateFiles(directoryPath, "*", SearchOption.AllDirectories)
                               .Where(f => !f.EndsWith("checksums.txt"));

        using (var writer = new StreamWriter(outputFilePath))
        {
            foreach (var filePath in fileList)
            {
                string relativePath = Path.GetRelativePath(directoryPath, filePath);
                using (var sha256 = SHA256.Create())
                {
                    using (var stream = File.OpenRead(filePath))
                    {
                        var hash = await sha256.ComputeHashAsync(stream);
                        string hashString = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                        await writer.WriteLineAsync($"{hashString}  {relativePath}");
                    }
                }
            }
        }
        _logger.LogInformation("체크섬 파일 생성 완료.");
    }
}
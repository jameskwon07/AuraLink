// IChecksumGenerator의 구현체. 파일 해시를 계산하여 파일로 저장합니다.
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;

public class ChecksumGenerator : IChecksumGenerator
{
    private readonly ILogger<ChecksumGenerator> _logger;

    public ChecksumGenerator(ILogger<ChecksumGenerator> logger)
    {
        _logger = logger;
    }

    public async Task GenerateChecksumsFileAsync(string directoryPath, string outputFilePath)
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
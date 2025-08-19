namespace ImageBuilder;

using System.Security.Cryptography;
using ImageDownloader;
using Microsoft.Extensions.Logging;
using Moq;

public class ChecksumGeneratorTests
{
    [Fact]
    public async Task GenerateChecksumsFileAsync_WithValidFiles_CreatesCorrectFile()
    {
        // 준비
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        
        var fileA = Path.Combine(tempDir, "fileA.txt");
        var fileB = Path.Combine(tempDir, "sub", "fileB.zip");
        Directory.CreateDirectory(Path.GetDirectoryName(fileB));
        
        File.WriteAllText(fileA, "Hello, world!");
        File.WriteAllBytes(fileB, new byte[] { 0x1, 0x2, 0x3, 0x4 });
        
        var mockLogger = new Mock<ILogger<ChecksumGenerator>>();
        
        var generator = new ChecksumGenerator(mockLogger.Object);
        var checksumsPath = Path.Combine(tempDir, "checksums.txt");
        
        // 실행
        await generator.GenerateChecksumsFileAsync(tempDir, checksumsPath);
        
        // 검증
        Assert.True(File.Exists(checksumsPath));
        var lines = await File.ReadAllLinesAsync(checksumsPath);
        Assert.Equal(2, lines.Length);
        
        // 예상 해시값 계산
        string hashA = GetSha256Hash("Hello, world!");
        string hashB = GetSha256Hash(new byte[] { 0x1, 0x2, 0x3, 0x4 });
        
        // 파일 경로에 따라 순서가 달라질 수 있으므로, 내용으로 검증
        Assert.Contains($"{hashA}  fileA.txt", lines);
        Assert.Contains($"{hashB}  sub{Path.DirectorySeparatorChar}fileB.zip", lines);
        
        // 정리
        Directory.Delete(tempDir, true);
    }
    
    // 테스트용 SHA-256 해시 계산 헬퍼 메서드
    private string GetSha256Hash(string content)
    {
        using (var sha256 = SHA256.Create())
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(content);
            var hash = sha256.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
    
    private string GetSha256Hash(byte[] content)
    {
        using (var sha256 = SHA256.Create())
        {
            var hash = sha256.ComputeHash(content);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
}
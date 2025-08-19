// 파일 무결성 체크섬 생성을 위한 인터페이스.
public interface IChecksumGenerator
{
    Task GenerateChecksumsFileAsync(string directoryPath, string outputFilePath);
}
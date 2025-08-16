namespace ImageBuilder;

using ImageDownloader;
using Microsoft.Extensions.Logging; // ILogger를 사용하기 위해 추가
using Microsoft.Extensions.Logging.Console;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions; // 콘솔 로깅을 위해 추가

// 애플리케이션의 진입점.
// 실제 프로젝트에서는 이 파일이 .NET Console Application의 Program.cs 파일이 됩니다.
public class ConsoleProgram
{
    public static async Task Main(string[] args)
    {
        string yamlFilePath = "ProgramList.yaml";
        if (!File.Exists(yamlFilePath))
        {
            Console.WriteLine($"Error: {yamlFilePath} 파일을 찾을 수 없습니다. ");
            return;
        }

        List<Program> programList = new List<Program>();

        try
        {
            string yamlContent = await File.ReadAllTextAsync(yamlFilePath);
            
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            
            // 1. YAML을 List<Dictionary<object, object>>로 역직렬화
            var yamlData = deserializer.Deserialize<List<object>>(yamlContent);

            // 2. 딕셔너리를 파싱하여 Program 객체 리스트로 변환
            var programs = ProgramParser.ParsePrograms(yamlData);

            Console.WriteLine("YAML 파싱 완료!");
            Console.WriteLine("---------------------------------");
            
            // 결과 확인
            foreach (var program in programs)
            {
                Console.WriteLine($"프로그램: {program.Name}");
                Console.WriteLine($"  아티팩트 소스 타입: {program.ArtifactSource.Type} ({program.ArtifactSource.GetType().Name})");
                if (program.ThirdParty != null)
                {
                    Console.WriteLine("  의존성 목록:");
                    foreach (var dependency in program.ThirdParty)
                    {
                        Console.WriteLine($"    - {dependency.Name}: {dependency.ArtifactSource.GetType().Name}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"YAML 파일을 읽는 중 오류가 발생했습니다: {ex.Message}");
            return;
        }

        // 로깅 설정을 위한 팩토리입니다. 실제로는 DI 컨테이너를 통해 주입됩니다.
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Debug);
        });

        // 의존성 주입을 위해 DownloaderFactory 인스턴스를 생성합니다.
        var imageBuilder = new ImageBuilder(loggerFactory);
        await imageBuilder.BuildAsync(programList, "./downloads");
    }
}
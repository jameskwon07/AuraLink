using ImageBuilder;
using ImageDownloader;
using Microsoft.Extensions.Logging; // ILogger를 사용하기 위해 추가
using Microsoft.Extensions.Logging.Console; // 콘솔 로깅을 위해 추가

// 애플리케이션의 진입점.
// 실제 프로젝트에서는 이 파일이 .NET Console Application의 Program.cs 파일이 됩니다.
public class ConsoleProgram
{
    public static async Task Main(string[] args)
    {
        // 실제 애플리케이션에서는 YAML 설정 파일을 읽어오는 로직이 필요합니다.
        // 여기서는 예시를 위해 더미 데이터를 사용합니다.
        var programList = new List<Program>
        {
            new Program
            {
                Name = "AuralinkAgent",
                ArtifactSource = new JenkinsArtifactSource
                {
                    ProgramName = "AuralinkAgent",
                    Address = "http://testjenkins.com",
                    JobName = "Auralink-Agent-Build",
                    JobIdentifier = "lastSuccessfulBuild",
                    ArtifactPath = "target/agent.zip"
                },
                ThirdParty = new List<Program>
                {
                    new Program
                    {
                        Name = "ThirdPartyTool-A",
                        ArtifactSource = new GitHubArtifactSource
                        {
                            ProgramName = "ThirdPartyTool-A",
                            Owner = "thirdparty",
                            Repo = "tool-a",
                            Tag = "v1.0.0",
                            ArtifactName = "tool-a-win.zip"
                        }
                    },
                    new Program
                    {
                        Name = "ThirdPartyTool-B",
                        ArtifactSource = new JenkinsArtifactSource
                        {
                            ProgramName = "ThirdPartyTool-B",
                            Address = "http://anotherjenkins.com",
                            JobName = "Tool-B-Build",
                            JobIdentifier = "50",
                            ArtifactPath = "bin/tool-b.zip"
                        }
                    }
                }
            }
        };

        // 로깅 설정을 위한 팩토리입니다. 실제로는 DI 컨테이너를 통해 주입됩니다.
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Debug);
        });

        var source = new JenkinsArtifactSource();

        var imageBuilder = new ImageBuilder.ImageBuilder(loggerFactory);
        await imageBuilder.BuildAsync(programList, "./downloads");
    }
}
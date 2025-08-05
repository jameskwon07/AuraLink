using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using GoldenImageConfig;
using Microsoft.Extensions.Logging; // ILogger를 사용하기 위해 추가
using Microsoft.Extensions.Logging.Console; // 콘솔 로깅을 위해 추가

// This test project requires the following NuGet packages:
// Microsoft.NET.Test.Sdk
// MSTest.TestFramework
// MSTest.TestAdapter
// YamlDotNet
// Microsoft.Extensions.Logging
// Microsoft.Extensions.Logging.Console

// To add them using dotnet CLI, run the following commands in your test project directory:
// dotnet add package Microsoft.NET.Test.Sdk
// dotnet add package MSTest.TestFramework
// dotnet add package MSTest.TestAdapter
// dotnet add package YamlDotNet
// dotnet add package Microsoft.Extensions.Logging
// dotnet add package Microsoft.Extensions.Logging.Console

[TestClass]
public class GoldenImageConfigReaderTests
{
    private string _yamlFilePath;
    private ILogger<GoldenImageConfigReaderTests> _logger; // ILogger 인스턴스 추가

    [TestInitialize]
    public void Setup()
    {
        // ILogger 설정
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole(options =>
            {
                options.LogToStandardErrorThreshold = LogLevel.Warning; // Warning 이상은 stderr로
                options.FormatterName = ConsoleFormatterNames.Simple; // 간단한 포맷
            })
            .SetMinimumLevel(LogLevel.Debug); // 최소 로그 레벨을 Debug로 설정하여 모든 로그 출력
        });
        _logger = loggerFactory.CreateLogger<GoldenImageConfigReaderTests>();

        _yamlFilePath = "ProgramList.yaml";
        CreateExampleYamlFile(_yamlFilePath, false);
        _logger.LogInformation("Setup: 테스트 환경이 초기화되었습니다."); // ILogger 사용
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (File.Exists(_yamlFilePath))
        {
            File.Delete(_yamlFilePath);
        }
        _logger.LogInformation("Cleanup: 테스트 환경이 정리되었습니다."); // ILogger 사용
    }

    [TestMethod]
    public void ReadProgramList_ShouldReturnCorrectProgramCount()
    {
        _logger.LogDebug("TestMethod: ReadProgramList_ShouldReturnCorrectProgramCount 실행 시작."); // ILogger 사용
        // Arrange
        var configReader = new GoldenImageConfigReader();

        // Act
        var programList = configReader.ReadProgramList(_yamlFilePath);

        // Assert
        Assert.IsNotNull(programList);
        Assert.AreEqual(2, programList.Programs.Count, "Program list should contain 2 items.");
        _logger.LogInformation($"TestMethod: {programList.Programs.Count}개의 프로그램이 확인되었습니다."); // ILogger 사용
    }

    [TestMethod]
    public void ReadProgramList_ShouldParseCorrectProgramData()
    {
        _logger.LogDebug("TestMethod: ReadProgramList_ShouldParseCorrectProgramData 실행 시작."); // ILogger 사용
        // Arrange
        var configReader = new GoldenImageConfigReader();

        // Act
        var programList = configReader.ReadProgramList(_yamlFilePath);
        var firstProgram = programList.Programs[0];
        var secondProgram = programList.Programs[1];

        // Assert
        Assert.AreEqual("Visual Studio Code", firstProgram.Name, "First program name is incorrect.");
        Assert.IsNotNull(firstProgram.Jenkins);
        Assert.AreEqual("http://jenkins.yourcompany.com", firstProgram.Jenkins.Address);
        Assert.AreEqual("vscode-build", firstProgram.Jenkins.JobName);
        Assert.AreEqual(123, firstProgram.Jenkins.JobNumber);

        Assert.AreEqual("Node.js Runtime", secondProgram.Name, "Second program name is incorrect.");
        Assert.IsNotNull(secondProgram.Jenkins);
        Assert.AreEqual("nodejs-runtime-build", secondProgram.Jenkins.JobName);
        Assert.AreEqual(456, secondProgram.Jenkins.JobNumber);
        _logger.LogInformation("TestMethod: 프로그램 데이터가 올바르게 파싱되었습니다."); // ILogger 사용
    }

    [TestMethod]
    public void ReadProgramList_ShouldHandleOptionalJobNumber()
    {
        _logger.LogDebug("TestMethod: ReadProgramList_ShouldHandleOptionalJobNumber 실행 시작."); // ILogger 사용
        // Arrange
        CreateExampleYamlFile(_yamlFilePath, true);
        var configReader = new GoldenImageConfigReader();

        // Act
        var programList = configReader.ReadProgramList(_yamlFilePath);
        var programWithJobNumber = programList.Programs[0];
        var programWithoutJobNumber = programList.Programs[1];

        // Assert
        Assert.IsNotNull(programWithJobNumber.Jenkins.JobNumber);
        Assert.AreEqual(789, programWithJobNumber.Jenkins.JobNumber);

        Assert.IsNull(programWithoutJobNumber.Jenkins.JobNumber, "JobNumber should be null when not specified in YAML.");

        // Example of how consuming code would handle the optional value
        string buildNumber = programWithoutJobNumber.Jenkins.JobNumber.HasValue
            ? programWithoutJobNumber.Jenkins.JobNumber.ToString()
            : "lastSuccessfulBuild";
        Assert.AreEqual("lastSuccessfulBuild", buildNumber);
        _logger.LogInformation("TestMethod: Optional JobNumber 처리가 올바르게 확인되었습니다."); // ILogger 사용
    }

    [TestMethod]
    [ExpectedException(typeof(FileNotFoundException))]
    public void ReadProgramList_ShouldThrowException_WhenFileDoesNotExist()
    {
        _logger.LogDebug("TestMethod: ReadProgramList_ShouldThrowException_WhenFileDoesNotExist 실행 시작."); // ILogger 사용
        // Arrange
        var nonExistentFile = "NonExistentFile.yaml";
        if (File.Exists(nonExistentFile))
        {
            File.Delete(nonExistentFile);
        }
        var configReader = new GoldenImageConfigReader();

        // Act
        configReader.ReadProgramList(nonExistentFile);

        // Assert is handled by the [ExpectedException] attribute
        _logger.LogInformation("TestMethod: 파일이 없을 때 예외가 성공적으로 발생했습니다."); // ILogger 사용
    }

    /// <summary>
    /// Creates a sample YAML file for testing purposes.
    /// </summary>
    /// <param name="filePath">The path to save the example file.</param>
    /// <param name="useOptionalJobNumber">If true, creates a YAML with one optional job number.</param>
    private void CreateExampleYamlFile(string filePath, bool useOptionalJobNumber)
    {
        var exampleYaml = useOptionalJobNumber
            ? @"
programs:
  - name: Visual Studio Code
    jenkins:
      address: http://jenkins.yourcompany.com
      jobName: vscode-build
      jobNumber: 789
  - name: Node.js Runtime
    jenkins:
      address: http://jenkins.yourcompany.com
      jobName: nodejs-runtime-build
"
            : @"
programs:
  - name: Visual Studio Code
    jenkins:
      address: http://jenkins.yourcompany.com
      jobName: vscode-build
      jobNumber: 123
  - name: Node.js Runtime
    jenkins:
      address: http://jenkins.yourcompany.com
      jobName: nodejs-runtime-build
      jobNumber: 456
";
        File.WriteAllText(filePath, exampleYaml);
    }
}

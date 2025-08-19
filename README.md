# Artifact Builder for Golden Images


이 프로젝트는 YAML 설정을 기반으로 다양한 소스(예: Jenkins, GitHub)에서 소프트웨어 아티팩트를 다운로드하고, 파일 무결성 체크섬을 생성하여 단일 "골든 이미지" 패키지로 압축하는 C# 유틸리티입니다. CI/CD 파이프라인에서 복잡한 애플리케이션 및 의존성 패키징을 자동화하는 데 이상적입니다.

## 주요 기능
* 다중 소스 지원: Jenkins 및 GitHub와 같은 여러 플랫폼에서 아티팩트를 다운로드합니다.
* 설정 기반: 간단한 YAML 파일을 통해 다운로드할 프로그램과 의존성을 정의합니다.
* 파일 무결성 검증: 다운로드된 모든 파일에 대한 SHA-256 체크섬을 자동으로 계산하고 checksums.txt 파일을 생성합니다.
* 자동 압축: 모든 아티팩트와 체크섬 파일을 최종 패키지 ZIP 파일로 압축합니다.
* 의존성 주입(DI): 테스트 용이성을 극대화하기 위해 유연한 의존성 주입 패턴으로 설계되었습니다.

## 시작하기
### 전제 조건
.NET 6.0 SDK 이상

YamlDotNet 및 Microsoft.Extensions.Logging.Abstractions NuGet 패키지.

### 설치
프로젝트 폴더에서 다음 명령어를 사용하여 필요한 패키지를 설치합니다.
```
dotnet add package YamlDotNet
dotnet add package Microsoft.Extensions.Logging.Abstractions
dotnet add package System.IO.Compression
```

### 사용법
1. YAML 설정 파일 생성
프로젝트 루트에 ProgramList.yaml 파일을 생성하고 다운로드할 아티팩트 목록을 정의합니다.
```
- Name: AuralinkAgent
  ArtifactSource:
    type: jenkins
    address: http://testjenkins.com
    jobName: Auralink-Agent-Build
    jobIdentifier: lastSuccessfulBuild
    artifactPath: target/agent.zip
  ThirdParty:
    - Name: ThirdPartyTool-A
      ArtifactSource:
        type: github
        owner: thirdparty
        repo: tool-a
        tag: v1.0.0
        artifactName: tool-a-win.zip
    - Name: ThirdPartyTool-B
      ArtifactSource:
        type: jenkins
        address: http://anotherjenkins.com
        jobName: Tool-B-Build
        jobIdentifier: '50'
        artifactPath: bin/tool-b.zip
```
2. 프로젝트 실행
프로젝트를 실행하려면 다음 명령어를 사용하세요.
```
dotnet run
```
이 명령어는 YAML 파일에 정의된 대로 모든 아티팩트를 다운로드하고, 체크섬 파일을 생성하며, 최종 ZIP 패키지를 ./downloads 폴더에 생성합니다.

### 프로젝트 구조
- Program.cs: 애플리케이션의 진입점 역할을 하며, 설정 파일 읽기 및 ImageBuilder 인스턴스 생성 로직을 포함합니다.
- ImageBuilder.cs: 다운로드 및 패키징 프로세스를 오케스트레이션하는 핵심 클래스입니다.
- IChecksumGenerator.cs, ChecksumGenerator.cs: 파일 체크섬 생성을 추상화하고 구현합니다.
- IArtifactDownloader.cs, JenkinsDownloader.cs, GitHubDownloader.cs: 각 소스별 아티팩트 다운로드 기능을 추상화하고 구현합니다.
- IDownloaderFactory.cs, DownloaderFactory.cs: 런타임에 올바른 다운로더 클래스를 생성하는 팩토리 패턴을 구현합니다.
- ArtifactSource.cs: YAML 설정의 아티팩트 소스 정보를 나타내는 데이터 모델 클래스입니다.

### 테스트
이 프로젝트는 Moq와 Xunit을 사용하여 각 클래스에 대한 유닛 테스트를 포함하고 있습니다. 테스트를 실행하려면 다음 명령어를 사용하세요.
```
dotnet test
```

## 기여
버그 리포트, 기능 제안 또는 코드 기여를 환영합니다. [이슈 페이지](https://github.com/jameskwon07/AuraLink/issues)에 문의하거나 풀 리퀘스트를 제출해 주세요.

## 라이선스
이 프로젝트는 [GNU Affero General Public License v3.0](https://www.gnu.org/licenses/agpl-3.0.html) 라이선스를 따릅니다.
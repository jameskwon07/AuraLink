# MVP - Golden Image 생성 

## 가치 
사용자가 Jenkins를 통해서 어떤 프로그램을 이미지화할 지 선택할 수 있다


## 요구사항
* 패키징 프로그램은 C# Self-Contained 모드로 동작한다.
    * Windows, Linux(RPi)에서 별도의 프로그램 없이 동작하길 바란다.
* Jenkins CI/CD에 존재하는 Job을 빌드하여 Artifact를 가져올 수 있다.
* Jenkins CI/CD에 존재하는 Job의 Artifact를 빌드하지 않고 가져올 수 있다.
* 2개 이상의 Job을 선택할 수 있다. 

## 해당 MVP에서 하지 않는 것
* 프로그램 실행에 필요한 설정을 주입하는 단계
* Jenkins CI/CD가 아닌 프로그램을 가져오는 단계

## 도구
* Jenkins CI/CD

## Input
* Artifact를 가져올 Jenkins Job의 정보
    * Jenkins Server URL
    * Job 이름
    * 가져올 Artifact 규칙

## Output
* 프로그램이 포함된 압축파일
* 인증정보가 포함된 json

## C4 Model 
### Level 1
``` mermaid
C4Context
    title 시스템 컨텍스트 다이어그램
    Person(user, "사용자", "프로그램을 통해서 패키징")
    System(system, "패키징 시스템", "프로그램을 가져와 패키징")
    System_Ext(jenkins, "Jenkins", "프로그램을 가지고 있는 CI/CD")
    
    Rel(user, system, "가져올 프로그램 정보, 패키징 요청")
    Rel(system, jenkins, "유저로 입력받은 정보를 이용하여 프로그램 가져오기")
    Rel(system, user, "인증정보와 함께 압축된 파일")
```

### Level 2
``` mermaid
C4Container
    title AuraLink 시스템 - 컨테이너 다이어그램
    
    Person(dev_user, "개발자", "CI/CD를 사용하여 환경을 관리")
    Person(general_user, "사용자", "직관적인 GUI로 환경을 관리")
    
    Container(gui_tool, "AuraLink GUI 툴", "WPF / Electron", "골든 이미지 생성 및 배포를 위한 데스크톱 애플리케이션")
    Container(cli_tool, "AuraLink CLI 툴", ".NET CLI", "명령줄을 통해 골든 이미지를 생성/관리하는 도구")
    
    ContainerDb(image_data_db, "메타데이터 DB", "NoSQL (DynamoDB / Firestore)", "이미지 서명, 해시, 버전 정보를 저장")
    ContainerDb(image_storage, "이미지 저장소", "클라우드 스토리지 (S3 / GCS)", "실제 골든 이미지(zip/폴더) 파일을 저장")
    
    Container(pc_agent, "PC 에이전트", "C# / Rust", "PC에 상주하며 골든 이미지를 다운로드 및 배포")
    
    Rel(dev_user, cli_tool, "이미지 생성 및 배포 명령", "CLI")
    Rel(general_user, gui_tool, "이미지 생성/배포 요청", "GUI")
    
    Rel(gui_tool, image_data_db, "메타데이터 업로드", "HTTPS")
    Rel(gui_tool, image_storage, "이미지 파일 업로드", "HTTPS")
    
    Rel(cli_tool, image_data_db, "메타데이터 업로드", "HTTPS")
    Rel(cli_tool, image_storage, "이미지 파일 업로드", "HTTPS")
    
    Rel(pc_agent, image_data_db, "메타데이터 조회", "HTTPS")
    Rel_Back(image_data_db, pc_agent, "서명 및 해시 정보 제공", "HTTPS")
    
    Rel(pc_agent, image_storage, "이미지 파일 다운로드", "HTTPS")
    Rel_Back(image_storage, pc_agent, "이미지 파일 제공", "HTTPS")

```
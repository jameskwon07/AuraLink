# AuraLink Software Design Description

- [AuraLink Software Design Description](#auralink-software-design-description)
  - [Stakeholders \& Concerns](#stakeholders--concerns)
  - [Context](#context)
    - [Viewpoint](#viewpoint)
    - [View](#view)
  - [Composition](#composition)
    - [Viewpoint](#viewpoint-1)
    - [View](#view-1)


## Stakeholders & Concerns
| 직군 | 관심사 |
| -- | -- |
| 시스템 엔지니어 | 버전 관리 |
| 개발자 | 버전 관리<br>배포 절차 |
| 배포 관리자 | 버전 관리<br>배포 절차<br>데이터 저장 및 관리<br>시스템 내부 구조 |
| 사용자 | 버전 관리<br>설치 절차 |


## Context
### Viewpoint
#### Purpose
AuraLink 시스템의 전체적인 시스템 구조를 파악하기 위한 Viewpoint
#### Element
Context Viewpoint의 구성 요소
##### Entities
* Actor  
    * 시스템 엔지니어
    * 개발자
    * 배포 관리자
    * 사용자
    * 아티팩트 저장소
    
* Service
    * 개발자는 소프트웨어를 개발하여 아티팩트 저장소에 업로드한다.
    * 시스템 엔지니어는 패키징된 소프트웨어의 버전을 확인한다.
    * 사용자는 패키지 버전을 선택하여 설치한다.

##### Relations
* 개발자는 패키징할 소프트웨어 설정을 입력한다.
* 시스템 엔지니어는 패키징되어있는 소프트웨어 버전을 확인한다.
* 사용자는 패키징되어있는 소프트웨어 버전 목록을 확인한다.
* 사용자는 패키징되어있는 소프트웨어를 다운로드 받는다.
#### Language
* C4 Diagram - Level 1
### View
```mermaid
C4Context

title Aura Link Context View

Enterprise_Boundary("Enterprise") {
  Person(sys, "시스템 엔지니어", "배포가 되는 소프트웨어 버전을 관리한다")
  Person(ops, "배포 관리자", "아티팩트를 이용하여 소프트웨어를 배포한다")
  Person(user, "사용자", "설치된 소프트웨어를 이용한다")
  Person(dev, "개발자", "소프트웨어를 개발하여 아티팩트 저장소에 업로드한다")

  System_Ext(artifacts, "아티팩트 저장소", "아티팩트가 저장되어있는 저장소")
}
System_Boundary("Aura Link") {
  System(aura_link, "Aura Link", "프로그램을 가져와 패키징")
}
    

Rel(sys, aura_link, "배포된 버전 확인")
Rel(dev, artifacts, "개발된 소프트웨어 아티팩트 업로드")
Rel(dev, aura_link, "패키지 사용된 소프트웨어 버전 확인")
Rel(ops, aura_link, "아티팩트를 이용하여 소프트웨어 패키징")
Rel(ops, aura_link, "패키지 다운로드하여 배포")
Rel(aura_link, artifacts, "아티팩트 다운로드")
Rel(user, aura_link, "패키지를 이용하여 설치")
```

## Composition
### Viewpoint
#### Purpose
AuraLink 시스템의 구성을 이해하기 위한 Viewpoint
#### Element
##### Entities

* Subsystem
  * 
##### Relations
#### Language
* C4 Diagram - Level 2
### View
```mermaid
C4Container

title Aura Link Composition View - Subsystems

System_Boundary(aura_link, "Aura Link") {
  Container_Boundary("MakerKit") {
    Container(web, "웹 애플리케이션", "사용자 인터페이스 제공")
    Container(api_service, "API 서비스", "백엔드 로직 처리")
    ContainerDb(version_database, "버전 데이터베이스", "이미지 버전을 관리하는 데이터베이스")
    ContainerDb(repository, "이미지 저장소", "패키징된 이미지를 관리")
  }

  Container_Boundary("ImageBuilder") {
    Container(downloader, "아티팩트 다운로더", "아티팩트 다운로드")
    Container(builder, "이미지 빌더", "다운로드한 아티팩트를 하나의 이미지로 패키징")
  }

  Container_Boundary("ImageInstaller") {
    Container(installer, "이미지 인스톨러", "이미지를 설치")
    Container(repository_downloader, "이미지 다운로드")
    Container(local_repository, "로컬 이미지 저장소", "이미지 저장소에서 일부 이미지만 관리")
  }

  
  Rel(builder, api_service, "이미지 업로드 요청")
  Rel(api_service, repository, "이미지 저장소에 업로드")
  Rel(web, api_service, "이미지 버전 리스트 요청")
  BiRel(api_service, version_database, "버전 CRUD")
  Rel(builder, downloader, "아티팩트 다운로드")

  Rel(repository_downloader, repository, "이미지 다운로드")
  Rel(repository_downloader, local_repository, "이미지를 로컬 저장소에 추가")
  Rel(installer, local_repository, "이미지 버전 조회 및 이미지 설치")

}
ContainerDb_Ext(artifacts, "아티팩트 저장소")

Rel(downloader, artifacts, "아티팩트 다운로드")


```
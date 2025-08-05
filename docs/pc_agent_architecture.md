# MVP - PC Agent가 실행되는 동글

## 가치
연결만 해도 자동으로 동작하는 동글  
PC Agent가 설치되어있지 않다면 LED로 알림

## 제약
PC Agent는 PC에 설치가 되어있어야한다.
위치는 C# .NET에서 기본으로 접근할 수 있는 UserProfile 관련 경로.

### 경로
* Environment.SpecialFolder.MyDocuments: (문서 폴더)  
    * Windows: C:\Users\\\<UserName>\Documents  
    * Linux: /home/\<UserName>/Documents

* Environment.SpecialFolder.UserProfile:
    * Windows: C:\Users\\\<UserName>
    * Linux: /home/\<UserName>  
    

## Sequence Diagram
``` mermaid
sequenceDiagram
autonumber
actor user as Client
box Green PC
    participant pc as PC
    participant installed_agent as 설치된 PC Agent
end
box Purple Dongle
    participant firmware as Firmware
    participant agent as PC Agent
    
    %% participant script as Packaging Script
end

user ->> pc: 동글 연결
firmware ->> pc: Device Descriptor 전송 (HID + CDC) 
pc ->> pc: 드라이버 로드 및 설정
installed_agent ->> firmware: OS 정보 전달

firmware ->> installed_agent: PC Agent 무슨 메세지를 전달하고 뭘 실행하지? 
```

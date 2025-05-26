# FastLink

## 🚀 소개

**FastLink**는 기존의 즐겨찾기/바로가기 시스템이 가진  
접근성, 통합성, 속도, 유연성의 한계를 쉽고 빠르게 해결하는 새로운 방식의 관리 도구입니다.

**웹 브라우저의 즐겨찾기, 파일 시스템의 바로가기와 즐겨찾기 기능, 불편하지 않으셨나요?**
브라우저마다 따로 관리해야 하는 즐겨찾기, 여러 폴더와 데스크톱에 흩어진 파일/폴더 바로가기, 동기화의 번거로움, 정리의 어려움, 원하는 순간에 바로 접근하기 힘든 경험...<br>
이런 문제로 인해 정말 자주 쓰는 자료와 웹페이지, 폴더, 명령어들이 오히려 더 복잡하게 느껴진 적이 많으실 겁니다.

**FastLink**는 이런 불편함을 한 번에 해결합니다.  
자주 사용하는 파일, 폴더, 웹사이트, 명령어를  
브라우저나 프로그램에 상관없이 한 곳에서 관리하고,  
언제 어디서든 단 한 번의 클릭이나 단축키로 즉시 실행할 수 있습니다.

- 더 이상 브라우저별로 즐겨찾기를 따로 관리할 필요가 없습니다.
- 복잡한 폴더 구조나 데스크톱 아이콘을 뒤질 필요도 없습니다.
- 원하는 방식(검색, 단축키, 더블클릭)으로 즉시 실행하세요.

---

## 🏆 주요 기능

- **추가(Add) 단축키**:  
  - (기본) Ctrl + Shift + F1
  - 현재 탐색기/브라우저의 경로를 자동 인식하여 빠르게 링크 추가
  - 웹 페이지 추가 시 'Microsoft Edge' 브라우저 사용 추천
  - 최신 'Google Chrome' 브라우저는 보안이 강화되어 일부 웹 페이지는 경로 인식이 제대로 안 되거나 페이지 전환 중에만 제대로 인식됨
    <p align="center">
      <img src="https://github.com/user-attachments/assets/03afcf4a-0f96-455e-9402-ab9504c4169d" alt="On Web Browser" width="500"/>
      <img src="https://github.com/user-attachments/assets/41223682-5980-454b-9c4b-6d94ba4d437d" alt="On File System" width="400"/>
    </p>

- **퀵뷰(QuickView) 단축키**:
  - (기본) Ctrl + Shift + Space
  - 전체 목록을 빠르게 팝업으로 띄워 바로 실행
  - 더블클릭 : 실행, 우클릭 : 경로 복사
    <p align="left">
      <img src="https://github.com/user-attachments/assets/5d128265-4600-4ab5-9c83-84192312c0a1" alt="On Web Browser" width="400"/>
    </p>

- **각 링크별 단축키**:
  - 원하는 단축키 지정, 언제든 실행 가능

- **시스템 트레이 아이콘**:
  - 항상 백그라운드에서 실행, 빠른 추가, 자동 시작 설정, 종료 등
    <p align="left">
      <img src="https://github.com/user-attachments/assets/addc1017-9f08-4d97-8083-3e04a5d4a1df" alt="On Web Browser" width="400"/>
    </p>

---

## ⚙️ 그 외 기능

- **드래그 & 드롭 / 정렬**: 목록 순서 자유롭게 변경
- **검색/필터**: 실시간 검색으로 원하는 링크 바로 찾기
- **윈도우 시작 시 자동 시작**: 옵션 체크 시 자동 실행 설정
- **설정/데이터 자동 저장**:
  - 링크 목록 및 사용자 설정(단축키, 자동시작 등) 자동 저장  
  - 사용자 AppData 폴더에 안전하게 저장

---

## 💻 설치 및 실행

### 1. 설치파일로 사용

- [Releases](https://github.com/pjy0121/FastLink/releases) 탭에서 최신 설치파일(.exe 또는 .msi) 다운로드 후 실행
- 요구사항 : .NET8 이상, Windows 10 이상

### 2. 직접 빌드

(1) git clone https://github.com/pjy0121/FastLink.git<br>
(2) cd FastLink<br>
(3) Visual Studio 2022 이상에서 FastLink.sln 열기<br>
(4) NuGet 패키지 복원 (MahApps.Metro, GongSolutions.Wpf.DragDrop, NHotkey.Wpf 등)<br>
(5) 빌드 후 bin 폴더 내 FastLink.exe 실행

---

## 📁 데이터/설정 저장 위치

- 링크(테이블 행) 목록:
  `%LOCALAPPDATA%\FastLink\fastlink_rows.json`
- 사용자 설정:
  `%LOCALAPPDATA%\FastLink\settings.json`

---

## 🛠️ 개발/기여 가이드

- .NET8 WPF, C#, MahApps.Metro 기반
- PR, 이슈 환영!
- 코드 컨벤션/폴더 구조는 [CONTRIBUTING.md](CONTRIBUTING.md) 참고

---

## 📜 라이선스

Apache 2.0 License<br>
비상업적 목적으로 자유롭게 사용/수정/배포/기여 가능합니다.

---

## 🙏 Special Thanks

- [MahApps.Metro](https://github.com/MahApps/MahApps.Metro)
- [GongSolutions.Wpf.DragDrop](https://github.com/punker76/gong-wpf-dragdrop)
- [NHotkey.Wpf](https://github.com/thomaslevesque/NHotkey)

---

**문의/제안/버그 제보는 [Issues](https://github.com/pjy0121/FastLink/issues) 탭을 이용해 주세요!**

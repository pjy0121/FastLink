# fastlink

**fastlink**는 초간편 링크 실행 및 단축키 관리 오픈소스 앱입니다.<br>
파일, 폴더, 웹사이트, 명령어 등을 한 번의 클릭 또는 단축키로 즉시 실행할 수 있습니다.

---

## 🚀 컨셉

- **모든 자주 쓰는 파일/폴더/웹사이트/명령어를 한 곳에서 관리**
- **Quick View와 핫키(Hotkey)로 어디서든 즉시 실행**
- **윈도우 시작 시 자동 실행, 트레이 아이콘, 직관적인 UI**

---

## 🏆 주요 기능

- **빠른 실행**: 파일, 폴더, 웹사이트, 명령어를 한 번의 클릭 또는 단축키로 실행
- **글로벌 단축키(Hotkey) 등록**:  
  - 각 항목별로 단축키 지정  
  - 전체 추가/빠른보기(QuickView) 핫키 설정
- **드래그&드롭 정렬**: 목록 순서 자유롭게 변경
- **검색/필터**: 실시간 검색으로 원하는 항목 바로 찾기
- **트레이 아이콘**: 항상 백그라운드에서 실행, 트레이 메뉴로 빠른 추가/종료
- **윈도우 시작 시 자동 시작**: 옵션 체크 한 번으로 자동 실행 설정
- **설정/데이터 자동 저장**:  
  - 항목 목록 및 사용자 설정(핫키, 자동시작 등) 자동 저장  
  - 사용자 AppData 폴더에 안전하게 저장
- **모던 UI**: MahApps.Metro 기반, 모든 테이블/버튼/컬럼 완전한 가운데 정렬, 반응형 디자인
- **오픈소스**: 누구나 자유롭게 개선/기여 가능

---

## 💻 설치 및 실행

### 1. 설치파일로 사용

- [Releases](https://github.com/pjy0121/fastlink/releases) 탭에서 최신 설치파일(.exe 또는 .msi) 다운로드 후 실행
- prerequisites : .NET8 이상, Windows 10 이상

### 2. 직접 빌드

(1) git clone https://github.com/pjy0121/fastlink.git
(2) cd fastlink

(3) Visual Studio 2022 이상에서 FastLink.sln 열기
(4) NuGet 패키지 복원 (MahApps.Metro, GongSolutions.Wpf.DragDrop, NHotkey.Wpf 등)
(5) 빌드 후 bin 폴더 내 fastlink.exe 실행

---

## ⚙️ 주요 단축키/기능

- **Add Hotkey**:  
  - (기본) Ctrl+Shift+F1
  - 현재 탐색기/브라우저의 경로를 자동 인식하여 빠르게 항목 추가
  - 웹 페이지 추가 시 'Microsoft Edge' 브라우저 사용 추천
  - 최신 'Google Chrome' 브라우저는 보안이 강화되어 일부 웹 페이지는 경로 인식이 제대로 안 되거나 페이지 전환 중에만 제대로 인식됨
- **QuickView Hotkey**:
  - (기본) Ctrl+Shift+Space
  - 전체 목록을 빠르게 팝업으로 띄워 바로 실행
  - 더블클릭 : 실행, 우클릭 : 경로 복사
- **각 항목별 Hotkey**:
  - 원하는 단축키 지정, 언제든 실행 가능
- **트레이 메뉴**:
  - 빠른 추가, 자동 시작 설정, 종료 등

---

## 📁 데이터/설정 저장 위치

- 항목 목록:
  `%LOCALAPPDATA%\FastLink\fastlink_rows.json`
- 사용자 설정:
  `%LOCALAPPDATA%\FastLink\settings.json`

---

## 🛠️ 개발/기여 가이드

- .NET 6/7/8 WPF, C#, MahApps.Metro 기반
- PR, 이슈 환영!  
- 코드 컨벤션/폴더 구조는 [CONTRIBUTING.md](CONTRIBUTING.md) 참고

---

## 📜 라이선스

Apache 2.0 License
상업용 이외 목적으로 자유롭게 사용/수정/배포/기여 가능합니다.

---

## 🙏 Special Thanks

- [MahApps.Metro](https://github.com/MahApps/MahApps.Metro)
- [GongSolutions.Wpf.DragDrop](https://github.com/punker76/gong-wpf-dragdrop)
- [NHotkey.Wpf](https://github.com/thomaslevesque/NHotkey)

---

**문의/제안/버그 제보는 [Issues](https://github.com/pjy0121/fastlink/issues) 탭을 이용해 주세요!**

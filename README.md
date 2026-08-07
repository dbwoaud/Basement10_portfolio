# Basement10_portfolio
# **🕹️ [프로젝트] 지하 10층**

https://github.com/user-attachments/assets/a18884e2-4b88-4d0a-9d88-52fda6e9c1ac

> **"8번 출구 게임과 영화에서 영감을 얻어, 스토리를 추가하고 다형성과 데이터 주도 설계로 재해석한 1인 개발 호러 퍼즐 게임입니다."**

---

## **📌 프로젝트 개요**

- **개발 인원**: 1인 개발 (기획, 프로그래밍, 리소스 관리)
- **개발 기간**: 2025.11 ~ 2026.07
- **기술 스택**: Unity(Built-in RP), C#, NavMesh, ScriptableObject, Unity Localization, Unity Test Framework, Unity Profiler
- **핵심 컨셉**: 8번 출구 게임에 스토리를 더해 재해석한 1인칭 호러 퍼즐 게임
- **주요 성과**
  1. SOLID 원칙 기반 시스템 구축과 오브젝트 풀링을 통한 런타임 할당 제거
  2. `GameManager`를 **순수 로직 + Unity 협력자**로 분리해 **EditMode 단위 테스트 28개** 확보
  3. `ProfilerRecorder` 기반 **정량 성능 계측** 및 렌더링 병목 진단
  4. 4개 언어 로컬라이제이션, 원자적 저장 기반 설정 시스템 등 **상용 배포 수준의 사이드 시스템** 구현

---

## **🛠️ 시스템 아키텍처**
컴퓨터공학과 전공자로서 **유지보수성, 확장성, 테스트 용이성**을 고려한 설계를 지향했습니다.


### **1. 싱글톤 & 매니저 패턴**
- `Singleton<T>` 베이스 클래스를 상속받아 `GameManager`, `SoundManager`, `FadeManager` 등 핵심 시스템의 단일성과 전역 접근성을 보장했습니다.
- 단순 널 체크를 넘어 **`isQuitting` 플래그와 `HasInstance` 프로퍼티**를 두어, 게임 종료와 씬 언로드 시점에 파괴된 인스턴스에 접근하는 것을 방지했습니다. 
https://github.com/dbwoaud/Basement10_portfolio/blob/5b20dc975c01c5f755e6fd6c7a5fb4c674a11b4d/Scripts/Core/Singleton.cs#L8-L25
- [🔗 **Singleton.cs 코드 보기**](https://github.com/dbwoaud/Basement10_portfolio/blob/main/Scripts/Core/Singleton.cs)
- 각 매니저는 단일 책임 원칙(SRP)에 따라 자신의 역할(사운드 재생, 페이드 연출, 설정 영속화)에만 집중하도록 분리했습니다.


### **2. 이벤트 기반 & 느슨한 결합**
- `Action`과 정적 이벤트를 활용해 시스템 간 직접 참조를 최소화했습니다.
- `EndingTrigger`는 엔딩 로직을 직접 실행하지 않고, `OnEndingTriggered` 이벤트만 발행하며, 이를 **`EndingDirector`가 구독**해 처리합니다. 마찬가지로 `ElevatorController`의 정답 선택은 `OnElevatorAnswerSelected` 이벤트로 `GameManager`에 전달됩니다.
https://github.com/dbwoaud/Basement10_portfolio/blob/5b20dc975c01c5f755e6fd6c7a5fb4c674a11b4d/Scripts/Manager/Ending/EndingTrigger.cs#L11-L23
https://github.com/dbwoaud/Basement10_portfolio/blob/5b20dc975c01c5f755e6fd6c7a5fb4c674a11b4d/Scripts/Game/EndingDirector.cs#L16-L26
- [🔗 **EndingTrigger.cs 코드 보기**](https://github.com/dbwoaud/Basement10_portfolio/blob/main/Scripts/Manager/Ending/EndingTrigger.cs)
- [🔗 **EndingDirector.cs 코드 보기**](https://github.com/dbwoaud/Basement10_portfolio/blob/main/Scripts/Game/EndingDirector.cs)


### **3. `GameManager`의 책임 분해**
게임의 핵심 루프를 담당하던 `GameManager`는 층 상태 관리, 정답 판정, 맵 생성, 엔딩 연출까지 떠안아 **거대해지기 쉬운 구조**였습니다. 이를 **역할별로 분리**해 단일 책임을 부여하고, 무엇보다 **Unity 런타임 없이도 검증 가능한 순수 로직**을 떼어냈습니다.

| 분리된 클래스 | 유형 | 책임 |
|---|---|---|
| `FloorProgress` | 순수 C# 클래스 | 현재 층과 방문 기록, 정답 회귀 상태 관리 |
| `FloorRule` | 정적 클래스 | 정답 판정과 다음 층 결정, 맵 종류 선택(순수 함수) |
| `MapSpawner` | MonoBehaviour | 맵 프리팹 생성 및 이상현상 적용, 층 표시 |
| `EndingDirector` | MonoBehaviour | 엔딩 트리거 구독 및 엔딩 시퀀스 재생 |

- `GameManager`는 이제 컴포넌트들을 **조립하고 제어하는 역할**만 남고, `[RequireComponent]`로 컴포넌트 누락을 컴파일 타임에 방지합니다.
https://github.com/dbwoaud/Basement10_portfolio/blob/5b20dc975c01c5f755e6fd6c7a5fb4c674a11b4d/Scripts/Manager/SingletonManager/GameManager.cs#L6-L37
- [🔗 **FloorProgress.cs 코드 보기**](https://github.com/dbwoaud/Basement10_portfolio/blob/main/Scripts/Game/FloorProgress.cs)
- [🔗 **FloorRule.cs 코드 보기**](https://github.com/dbwoaud/Basement10_portfolio/blob/main/Scripts/Game/FloorRule.cs)
- [🔗 **MapSpawner.cs 코드 보기**](https://github.com/dbwoaud/Basement10_portfolio/blob/main/Scripts/Game/MapSpawner.cs)
- [🔗 **GameManager.cs 코드 보기**](https://github.com/dbwoaud/Basement10_portfolio/blob/main/Scripts/Manager/SingletonManager/GameManager.cs)


### **4. 순수 로직 분리로 확보한 단위 테스트 (28개)**
`FloorProgress` / `FloorRule` / `GameSetting`을 Unity 런타임에서 떼어낸 결과,
Unity 에디터 실행 없이 게임 규칙 자체를 검증할 수 있게 되었습니다.

| 테스트 파일 | 케이스 | 검증 대상 |
|---|---|---|
| `FloorRuleTests` | 10 | 정답 판정, 다음 층 결정, 맵/이상현상 선택 규칙 |
| `FloorProgressTests` | 8 | 층 진행, 방문 기록 멱등성, 실패 회귀 |
| `GameSettingTests` | 10 | 값 범위 보정, 로케일 폴백, Clone 독립성 |

테스트 이름을 한국어 문장으로 작성해(`오답_제출_시_시작_층으로_돌아간다`)
테스트 목록 자체가 게임 규칙 명세로 읽히도록 했습니다.


### **5. 다형성 기반의 이상현상 시스템**

https://github.com/user-attachments/assets/a653e5a1-3dc7-46de-8197-0c291f981605

- **추상화:** `AbnormalData` 라는 추상 클래스(ScriptableObject)로 모든 이상현상의 공통 진입점 `ApplyAbnormal`을 정의했습니다.
- **구체화**: 생성(`Create`), 삭제(`Delete`), 교체(`Replace`), 크기 변형(`Scale`), 사운드 변조(`Sound`), NPC 변형(`NPCTransform`) 등 각기 다른 로직을 자식 클래스에 독립적으로 구현했습니다.
- 새로운 이상현상을 추가할 때 `SpawnAbnormalManager` 등 기존 시스템 코드를 수정할 필요 없이 클래스와 데이터 에셋만 추가하면 되는 **개방 폐쇄 원칙(OCP)** 을 실천했습니다.
- [🔗 **AbnormalData.cs 코드 보기**](https://github.com/dbwoaud/Basement10_portfolio/blob/main/Scripts/AbnormalData/AbnormalData.cs)


### **6. 컴포넌트 기반의 동적 기능 확장**
- `ScaleAbnormalData`의 Gradual(서서히 변형) 모드 실행 시, 대상 오브젝트에 `ObjectScaler` 컴포넌트를 런타임에 주입(`AddComponent`)하고 연출이 끝나면 스스로 `Destroy`하도록 했습니다.
- 모든 오브젝트에 무거운 스크립트를 미리 붙여두지 않고 **필요할 때만 기능을 활성화**해 메모리와 연산 효율을 높였습니다.
- [🔗 **ScaleAbnormalData.cs 코드 보기**](https://github.com/dbwoaud/Basement10_portfolio/blob/main/Scripts/AbnormalData/ScaleAbnormalData/ScaleAbnormalData.cs)
- [🔗 **ObjectScaler.cs 코드 보기**](https://github.com/dbwoaud/Basement10_portfolio/blob/main/Scripts/AbnormalData/ScaleAbnormalData/ObjectScaler.cs)


### **7. 설정 시스템 — Observer 패턴 & 견고한 영속화**
인게임 설정(언어 · 그래픽 프리셋 · 해상도 · 볼륨 · 감도 · 헤드밥)을 상용 배포 기준으로 구현했습니다.
- **Observer 패턴**: `SettingManager`가 설정 변경 시 `OnSettingsApplied` 이벤트를 발행하고, 각 적용기(`AudioVolumeApplier`, `DisplayApplier`, `GraphicPresetApplier`, `CameraLook`, `HeadBob`, `LanguageApplier`)는 `SettingApplierBase`를 상속해 자신에게 필요한 설정만 독립적으로 구독 및 적용합니다. 설정 항목이 늘어도 `SettingManager`의 코드는 그대로입니다.
https://github.com/dbwoaud/Basement10_portfolio/blob/5b20dc975c01c5f755e6fd6c7a5fb4c674a11b4d/Scripts/Settings/Applier/SettingApplierBase.cs#L4-L24
- **원자적 저장**: 저장 시 임시 파일에 먼저 쓴 뒤 `File.Replace`로 교체하고 `.bak` 백업을 남겨, 저장 도중 강제 종료되어도 설정 파일이 깨지지 않도록 했습니다. 읽기 실패 시에는 손상 파일을 격리(`.broken`)하고 기본값으로 복구합니다.
https://github.com/dbwoaud/Basement10_portfolio/blob/5b20dc975c01c5f755e6fd6c7a5fb4c674a11b4d/Scripts/Manager/SingletonManager/SettingManager.cs#L144-L160
- **버전 마이그레이션**: `GameSetting`에 `version` 필드를 두어, 구버전 저장 파일을 최신 스키마로 점진적으로 이관합니다.
- [🔗 **SettingManager.cs 코드 보기**](https://github.com/dbwoaud/Basement10_portfolio/blob/main/Scripts/Manager/SingletonManager/SettingManager.cs)
- [🔗 **SettingApplierBase.cs 코드 보기**](https://github.com/dbwoaud/Basement10_portfolio/blob/main/Scripts/Settings/Applier/SettingApplierBase.cs)
- [🔗 **SettingPanel.cs 코드 보기**](https://github.com/dbwoaud/Basement10_portfolio/blob/main/Scripts/UI/SettingPanel.cs)


### **8. 로컬라이제이션 (4개 언어)**
Unity Localization을 기반으로 **한국어 / 영어 / 일본어 / 중국어(간체)** 를 지원합니다.
- **정적 파사드 `Loc`**: `Loc.Story(key)` / `Loc.UI(key)` 형태로 어디서든 번역을 조회하고, 초기화가 끝날 때까지 안전하게 대기하는 `EnsureReady()` 코루틴을 제공합니다.
- **폴백 체인**: `현재 언어 → 영어 → 한국어` 순으로 조회해, 특정 언어에 번역이 누락되어도 빈 화면 대신 대체 문구를 노출합니다.
https://github.com/dbwoaud/Basement10_portfolio/blob/5b20dc975c01c5f755e6fd6c7a5fb4c674a11b4d/Scripts/Localizations/Loc.cs#L38-L58
- **UI 대응**: `TextSizeSynchronizer`가 언어별 글자 길이 차이에 맞춰 텍스트 크기를 동기화합니다.
- [🔗 **Loc.cs 코드 보기**](https://github.com/dbwoaud/Basement10_portfolio/blob/main/Scripts/Localizations/Loc.cs)
- [🔗 **GameLanguages.cs 코드 보기**](https://github.com/dbwoaud/Basement10_portfolio/blob/main/Scripts/Localizations/GameLanguages.cs)
- [🔗 **TextSizeSynchronizer.cs 코드 보기**](https://github.com/dbwoaud/Basement10_portfolio/blob/main/Scripts/UI/TextSizeSynchronizer.cs)

---


## **🚀 기술적 도전**

 
### **1. [탐색 범위 국소화] 자료구조(Stack) 기반 DFS 탐색 — `UIBinder`**
- **Problem**: 맵, UI 계층의 수많은 하위 오브젝트에서 특정 대상을 찾을 때, 씬 전역을 훑는 Find 계열 탐색은 대상과 무관한 오브젝트까지 순회하며, 재귀 구현은 계층이 깊어질수록 스택 오버플로 위험이 있습니다.
- **Solution**: `Stack<Transform>` 기반의 **반복적 DFS**를 직접 구현해 정적 유틸(`UIBinder`)로 추출했습니다. 탐색을 특정 루트 하위로 국소화하고, 재귀 대신 반복문을 사용해 스택 오버플로 위험을 제거했습니다. 이 유틸은 이상현상 대상 탐색(`AbnormalData.FindTarget`)과 UI 자동 바인딩 양쪽에서 재사용됩니다.
https://github.com/dbwoaud/Basement10_portfolio/blob/5b20dc975c01c5f755e6fd6c7a5fb4c674a11b4d/Scripts/Core/UIBinder.cs#L8-L28
- [🔗 **UIBinder.cs 코드 보기**](https://github.com/dbwoaud/Basement10_portfolio/blob/main/Scripts/Core/UIBinder.cs)


### **2. [데이터 주도] ScriptableObject 기반 설계** 
- `AbnormalData`를 `ScriptableObject`로 정의해 **로직과 데이터를 분리**했습니다.
- 새로운 이상현상 추가 시 코드 수정 없이 데이터 에셋 생성만으로 시스템에 즉시 반영되는 OCP를 실천했습니다.


### **3. [데이터 주도] 복합 데이터 구조를 활용한 연출 제어**
- **데이터화**: `struct`(`SpawnInfo`, `ReplaceInfo`, `ScaleInfo`)와 `List`로 교체, 생성, 변형 대상을 인스펙터에서 손쉽게 구성하도록 했습니다.
- **연출 유연성**: `enum`(`ScaleMode`, `SoundMode`)으로 즉시/서서히 변형, 음소거/중복 사운드 등 다양한 옵션을 하나의 시스템에서 제어합니다.


### **4. [UI 자동화] 자동 바인딩 & 선언적 시퀀스**
- `AutoBindUI()`에서 `UIBinder.Find<T>`로 하위 요소를 이름으로 탐색·할당하고, `UIBinder.BindButtons(root, handlers)`에 **`{버튼 이름 → 콜백}` 딕셔너리**를 넘겨 버튼 이벤트를 일괄 연결했습니다. 수동 인스펙터 할당의 번거로움과 휴먼 에러를 제거했습니다.
- `FadeManager`·엔딩 시퀀스는 코루틴과 `WaitUntil`을 조합해 페이드 → 사운드 → 씬 전환으로 이어지는 복잡한 연출을 선언적으로 관리합니다.
- [🔗 **FadeManager.cs 코드 보기**](https://github.com/dbwoaud/Basement10_portfolio/blob/main/Scripts/Manager/SingletonManager/FadeManager.cs)


### **5. [시스템 통합] 사운드 · 로직 연동** 
- 플레이어와 NPC의 발소리 시스템(`FootstepController`)을 이상현상 데이터와 연동해 **발소리 음소거** 및 **'발소리가 두 번 들리는 현상'**을 구현했습니다.
- `NavMeshAgent` 속도에 따라 NPC의 발소리 주기를 실시간 동기화했습니다.

---

## **📈 최적화 & 성능 계측**

 
### 오브젝트 풀링으로 GC 부담 최소화
`FloorNumberDisplay`에서 층 숫자 오브젝트를 매번 생성/파괴하는 대신, **슬롯 × 숫자(0–9) 조합을 미리 인스턴스화해두고 활성/비활성만 토글**하는 풀링을 적용했습니다. 런타임 중 `Instantiate`/`Destroy` 비용과 GC 스파이크를 제거했습니다.

https://github.com/user-attachments/assets/ede305d6-ae27-4c9f-b923-ae368601058a

https://github.com/user-attachments/assets/9a40a006-379f-490a-8833-449d049d41be

- [🔗 **FloorNumberDisplay.cs 코드 보기**](https://github.com/dbwoaud/Basement10_portfolio/blob/main/Scripts/UI/FloorNumberDisplay.cs)

  
### `ProfilerRecorder` 기반 정량 계측 
성능을 눈으로 직접 확인할 수 있도록, `ProfilerRecorder` API로 **프레임 타임, 배칭 방식별 드로우콜, 삼각형/버텍스, 그림자 캐스터, GC 할당량**을 0.5초 간격으로 수집해 **CSV로 기록**하는 `PerformanceLogger`를 만들었습니다.
- 이를 통해 GC 최적화가 유효함을 확인하고, **실제 병목이 렌더링**임을 정량적으로 진단했습니다.
- [🔗 **PerformanceLogger.cs 코드 보기**](https://github.com/dbwoaud/Basement10_portfolio/blob/main/Scripts/Profiling/PerformanceLogger.cs)

---

## **⚠️ 트러블슈팅: 이상현상 NPC의 외형 전환 구현**


### 1. 문제 상황
맵 내 NPC의 외형이 **이상현상 발생 시 실시간으로 변하는** 연출을 구현해야 했습니다. 평소에는 무표정한 연구원이지만, 이상현상이 감지되면 섬뜩한 표정으로 바뀌는 것이 핵심 연출이었습니다.
이를 구현하기 위해 세 가지 방식을 검토했습니다.

| 방식 | 검토 결과 |
|---|---|
| `Instantiate` / `Destroy`로 모델 교체 | 잦은 생성·삭제로 GC 부담 발생 → 배제 |
| 두 모델을 배치하고 `SetActive` 토글 | 실제 사용되는 모델은 하나인데 프리팹에 모델을 2개 배치 → 비효율적이라 판단 → 배제 |
| **런타임에 모델·애니메이션을 통째로 교체** | 채택 (→ 1차 시도) |

`SetActive` 방식이 가장 단순했지만, "실사용 모델은 하나"라는 점에서 리소스를 이중으로 배치하는 것이 부적절하다고 판단해, 하나의 NPC를 런타임에 교체하는 방향으로 접근했습니다.

---

### 2. 1차 시도: 런타임 모델·애니메이션 동적 교체 (접근의 한계 발견)
애니메이션이 적용된 모델은 `SkinnedMeshRenderer`, `Avatar`, `RuntimeAnimatorController`가 복잡하게 결합되어 있어, 단순히 오브젝트만 교체하면 애니메이션이 깨지거나 모델이 비정상 출력되는 문제가 있었습니다.

이를 해결하기 위해 애니메이션 시스템의 동작 원리를 분석해 **Cleanup → Setup → Sync**의 3단계 로직을 구축했습니다.

- **Cleanup**: `rootBoneName`을 추적해 하위 뼈대 구조를 먼저 제거하고, 기존 `SkinnedMeshRenderer`를 파괴해 메모리 충돌을 방지
- **Setup**: 새 모델을 인스턴스화한 뒤, 주체가 되는 `Animator`에 새 `Avatar`와 `RuntimeAnimatorController`를 수동 재할당. 자식 오브젝트의 불필요한 `Animator`는 제거해 연산 낭비 감소
- **Sync**: `Animator.Rebind()`와 `Update(0f)`로 변경된 아바타 정보를 강제 갱신하고, `CrossFadeInFixedTime`으로 기본 상태에 부드럽게 진입
> 📄 1차 방식 코드: [NPCTransformAbnormalData.cs (초기 버전)](https://github.com/dbwoaud/Basement10_portfolio/blob/e5ac1c0c0a6ba592b8e7c257b6af012b6548442e/Scripts/Abnormal/NPCTransformAbnormalData.cs)

**한계 발견**
겉보기에는 정상 작동했으나, 재테스트 과정에서 교체된 모델의 **손·팔이 뒤틀리는 현상**이 발생했습니다. 원인을 추적한 결과, 문제는 교체 로직이 아니라 **모델 자체에 있었습니다.**

기본 표정 모델과 이상현상 표정 모델을 AI로 **각각 따로 생성**했기 때문에, 두 모델의 스켈레톤이 미묘하게 달랐습니다. 이 상태에서 한 모델의 애니메이션을 다른 모델의 아바타로 리타게팅하면, 애니메이션이 뒤틀리는 문제가 발생했습니다.

즉, 아무리 교체 로직을 정교하게 다듬어도 **"모델이 둘"이라는 구조 자체가 문제의 근본 원인**이었습니다.

---

### 3. 2차 해결: 블렌드셰이프 기반 단일 모델 (성공)
**발상 전환**: 모델을 교체하는 대신, **하나의 모델**이 표정만 바꾸도록 하면 리타게팅 불일치가 원천적으로 사라진다고 판단했습니다.

https://github.com/user-attachments/assets/0ecd40c8-0b8b-4170-bb56-a193d5b9237c

- **모델 제작**: Blender에서 기본 표정 모델의 얼굴 버텍스를 직접 편집해 `Smile` 블렌드셰이프를 조각. 완전한 Mixamo 스켈레톤을 가진 모델을 베이스로 사용해 리깅 일관성 확보
- **런타임 제어**: `SkinnedMeshRenderer.SetBlendShapeWeight()`로 표정 가중치를 조절
- **시스템 통합**: 기존 이상현상 시스템(`AbnormalData` ScriptableObject)에 그대로 결합
> 📄 2차 방식 전체 코드: [NPCTransformAbnormalData.cs (최종 버전)](https://github.com/dbwoaud/Basement10_portfolio/blob/3b3122561798a61a5972a51668b22467bab13002/Scripts/AbnormalData/NPCTransformAbnormalData/NPCTransformAbnormalData.cs)

**결과**
- 모델이 하나로 통일되어 **애니메이션 리타게팅 문제 자체가 소멸**
- 표정 전환이 자연스러워지고, 코드도 3단계 교체 로직에서 단순한 가중치 조절로 축소
- 부수적으로, 표정별로 이원화되어 있던 모델·애니메이션 리소스가 절반으로 감소

---

### 4. 배운 점
- **증상이 아니라 근본 원인을 찾는 것**이 핵심이었습니다. "손이 꼬인다"는 증상에 매달려 교체 로직을 계속 다듬었다면 해결되지 않았을 문제였고, 원인이 모델 이원화에 있음을 파악한 뒤에야 방향이 잡혔습니다.
- **공들인 접근을 고수하지 않는 판단**이 결과적으로 더 단순하고 견고한 해결로 이어졌습니다. 1차 방식은 기술적으로 정교했지만, 문제의 뿌리를 제거하는 2차 방식이 코드·리소스·안정성 모든 면에서 우수했습니다.
- 이 과정에서 유니티 애니메이션 시스템의 **Avatar, 리타게팅 구조**와 **블렌드셰이프 파이프라인**에 대한 이해를 함께 얻었습니다.

---

## **📐 클래스 다이어그램**
전체 구조는 [**Docs/ClassDiagram.md**](Docs/ClassDiagram.md)에서 확인할 수 있습니다.

---

## **🔗 참조**
- **Notion**: [[지하 10층 Notion 링크]](https://pinnate-earthworm-118.notion.site/10-f31faf7d496e828dab0501cd8dd8dae3)
- **YouTube**: [[기술 데모 영상 링크]](https://www.youtube.com/watch?v=dkS35WRMzng)

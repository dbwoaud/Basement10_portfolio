# 📐 클래스 다이어그램 — 지하 10층 (Basement_10)

> 실제 구현 코드(`Scripts/`) 기준 구조도입니다.
> 가독성을 위해 **시스템별로 다이어그램을 분리**했으며, 각 다이어그램에는 핵심 멤버만 표기했습니다.

**설계의 세 가지 축**

1. **God-class 분리** — `GameManager`의 책임을 순수 로직(POCO/static)과 Unity 협력자(MonoBehaviour)로 분해 → 단위 테스트 가능
2. **추상화 기반 확장** — `AbnormalData` / `SettingApplierBase` / `BaseUIManager<T>` 상속으로 기존 코드 수정 없이 기능 추가(OCP)
3. **정적 이벤트 기반 느슨한 결합** — 시스템 간 직접 참조 최소화

### 📑 목차

| # | 다이어그램 | 내용 |
|---|---|---|
| 0 | [전체 구조 개요](#0-전체-구조-개요) | 시스템 간 의존 관계 맵 |
| 1 | [Core 공통 기반](#1-core-공통-기반) | `Singleton<T>` · `BaseUIManager<T>` · `UIBinder` |
| 2 | [게임 코어](#2-게임-코어--god-class-분리) | `GameManager` 책임 분해 & 단위 테스트 |
| 3 | [이상현상 시스템](#3-이상현상-시스템-다형성) | `AbnormalData` 계층 |
| 4 | [설정 시스템](#4-설정-시스템-observer) | `SettingManager` & 적용기 계층 |
| 5 | [로컬라이제이션](#5-로컬라이제이션) | `Loc` · 4개 언어 |
| 6 | [캐릭터 & 엘리베이터](#6-캐릭터--엘리베이터) | 이동 · 발소리 · 상호작용 |
| 7 | [UI 계층](#7-ui-계층) | UI 매니저 & 연출 컴포넌트 |
| 8 | [사운드 · 연출 · 씬 시퀀스](#8-사운드--연출--씬-시퀀스) | `SoundManager` · `FadeManager` · 엔딩 |
| 9 | [이벤트 흐름](#9-이벤트-흐름-정적-이벤트) | 정적 이벤트 발행/구독 관계 |

---

## 0. 전체 구조 개요

세부 클래스는 생략하고 **시스템 단위의 의존 방향**만 표현했습니다.

```mermaid
flowchart TD
    subgraph CORE["🧩 Core 공통 기반"]
        A1["Singleton&lt;T&gt;"]
        A2["BaseUIManager&lt;T&gt;"]
        A3["UIBinder · AnimatorParams"]
    end

    subgraph GAME["🎮 게임 코어"]
        B1["GameManager"]
        B2["FloorProgress · FloorRule<br/>(순수 로직)"]
        B3["MapSpawner · EndingDirector"]
    end

    subgraph ANOMALY["👻 이상현상"]
        C1["AbnormalData 계층"]
        C2["SpawnAbnormalManager"]
    end

    subgraph SETTING["⚙️ 설정"]
        D1["SettingManager · GameSetting"]
        D2["SettingApplierBase 파생 6종"]
    end

    subgraph LOC["🌐 로컬라이제이션"]
        E1["Loc · GameLanguages"]
    end

    subgraph CHAR["🚶 캐릭터 · 상호작용"]
        F1["Player · NPC · Footstep"]
        F2["Elevator 계열"]
    end

    subgraph UI["🖥️ UI"]
        G1["UI 매니저 계층"]
        G2["TypewriterText · FloorNumberDisplay"]
    end

    subgraph SCENE["🎬 사운드 · 연출"]
        H1["SoundManager · FadeManager"]
        H2["씬 시퀀스 매니저"]
    end

    TEST["🧪 EditMode 테스트 28개"]
    PROF["📈 PerformanceLogger"]

    A1 --> B1
    A1 --> D1
    A1 --> H1
    A2 --> G1
    A3 --> C1
    A3 --> G1

    B1 --> B2
    B1 --> B3
    B3 --> C2
    C2 --> C1

    D1 --> D2
    D2 --> E1
    D2 --> F1

    E1 --> G1
    G1 --> G2
    B1 -. 이벤트 .-> G1
    F2 -. 이벤트 .-> B1
    G1 --> H1
    H2 --> H1
    C1 --> F1

    TEST -. 검증 .-> B2
    TEST -. 검증 .-> D1

    style GAME fill:#e8f4ff,stroke:#4a90d9
    style TEST fill:#eaffea,stroke:#4caf50
    style CORE fill:#fff6e5,stroke:#e0a030
```

---

## 1. Core 공통 기반

프로젝트 전반에서 재사용되는 기반 계층입니다. 싱글톤·UI 바인딩·문자열 해시 캐싱의 중복 로직을 제거합니다.

```mermaid
classDiagram
    class Singleton_T {
        <<Abstract MonoBehaviour>>
        -static T instance
        -static bool isQuitting
        +static T Instance$
        +static bool HasInstance$
        #virtual Awake()
        #virtual OnDestroy()
        -OnApplicationQuit()
    }
    note for Singleton_T "isQuitting 플래그로 종료 시점\n파괴된 인스턴스 접근 방지"

    class BaseUIManager_T {
        <<Abstract MonoBehaviour>>
        -static T instance
        +static T Instance$
        +static bool HasInstance$
        #virtual Awake()
        #abstract AutoBindUI()
        #virtual InitializeUI()
        #static PlayButtonSound()
    }

    class UIBinder {
        <<Static>>
        +FindTransform(root, name) Transform
        +FindObject(root, name) GameObject
        +Find~T~(root, name) T
        +FindInRow~T~(root, rowName, childName) T
        +BindButtons(root, handlers)
    }
    note for UIBinder "Stack 기반 반복적 DFS\n재귀 없이 국소 탐색"

    class AnimatorParams {
        <<Static>>
        +static int Opening$
        +static int MainDoorOpen$
        +static int ElevatorDoorOpen$
    }

    BaseUIManager_T ..> UIBinder : 요소 자동 바인딩
```

---

## 2. 게임 코어 — God-class 분리

핵심 게임 루프를 담당하던 `GameManager`를 **역할별로 분해**했습니다.
`FloorProgress`·`FloorRule`은 `MonoBehaviour`가 아닌 **순수 C# 로직**이라 Unity 런타임 없이 테스트할 수 있습니다.

```mermaid
classDiagram
    class GameManager {
        <<Singleton · RequireComponent>>
        -int startFloor
        -int targetFloor
        -Transform mapSpawnPoint
        +bool showFloorNumber
        +int CurrentFloor
        +bool isEnded
        +GameObject player
        +static event Action~int~ OnFloorFirstVisited$
        +static event Action OnLoopReset$
        -OnSceneLoaded(scene, mode)
        +StartLoop()
        -ResetPlayerPositionRoutine() IEnumerator
        -RaiseFloorEvents()
        +CheckAnswer(choice)
    }
    note for GameManager "협력자를 조립·조율하는\n얇은 코디네이터"

    class FloorProgress {
        <<순수 C# · Testable>>
        -HashSet~int~ visitedFloors
        +int StartFloor
        +int TargetFloor
        +int CurrentFloor
        +bool IsReturningFromFailure
        +bool IsCleared
        +Reset()
        +Submit(choice, hasAbnormal) bool
        +TryMarkVisited() bool
        +ConsumeReturningFlag() bool
    }

    class FloorRule {
        <<Static · Testable>>
        +IsCorrect(choice, hasAbnormal) bool
        +DecideNextMap(current, start, isCorrect) int
        +ChoiceMap(current, start, target, isEnding) MapInfo
    }

    class MapInfo {
        <<readonly struct>>
        +bool UseFinalMap
        +bool AllowAbnormal
    }

    class MapSpawner {
        <<MonoBehaviour>>
        -GameObject normalMapPrefab
        -GameObject finalMapPrefab
        +GameObject CurrentMap
        +AbnormalData CurrentAbnormal
        +bool HasAbnormal
        +Spawn(plan, spawnPoint)
        +Clear()
        +UpdateFloorDisplay(floor, visible)
    }

    class EndingDirector {
        <<MonoBehaviour>>
        -string badEndingSceneName
        -string trueEndingSceneName
        +bool IsEnded
        +ResetState()
        +Play(type)
        -EndingSequenceCoroutine(type) IEnumerator
    }

    class EndingTrigger {
        -EndType endType
        -bool isTriggered
        +static event Action~EndType~ OnEndingTriggered$
    }

    GameManager *-- FloorProgress : owns
    GameManager --> MapSpawner : RequireComponent
    GameManager --> EndingDirector : RequireComponent
    FloorProgress ..> FloorRule : 규칙 위임
    FloorRule ..> MapInfo : returns
    MapSpawner ..> MapInfo : 사용
    EndingTrigger ..> EndingDirector : 이벤트 구독
```

### 순수 로직 분리로 확보한 단위 테스트 (총 28개)

```mermaid
classDiagram
    class FloorRuleTests {
        <<Test · 10 cases>>
        +이상현상이_있을_때_되돌아가면_정답이다()
        +정답일_때_한_층_내려간다()
        +목표_층에서는_최종_맵이고_이상현상이_없다()
    }
    class FloorProgressTests {
        <<Test · 8 cases>>
        +정답을_10번_연속_제출하면_클리어된다()
        +오답_제출_시_시작_층으로_돌아간다()
        +방문_표시는_최초_한_번만_성공한다()
    }
    class GameSettingTests {
        <<Test · 10 cases>>
        +Validate는_볼륨을_영과_일_사이로_보정한다()
        +지원하지_않는_로케일을_한국어로_되돌린다()
        +Clone은_독립적인_사본을_만든다()
    }

    class FloorRule {
        <<Static>>
    }
    class FloorProgress {
        <<순수 C#>>
    }
    class GameSetting {
        <<Serializable>>
    }

    FloorRuleTests ..> FloorRule : verifies
    FloorProgressTests ..> FloorProgress : verifies
    GameSettingTests ..> GameSetting : verifies
```

---

## 3. 이상현상 시스템 (다형성)

추상 클래스 `AbnormalData`(ScriptableObject) 하나로 6종의 이상현상을 확장합니다.
새 이상현상 추가 시 **기존 시스템 코드 수정이 불필요**합니다(OCP).

```mermaid
classDiagram
    class AbnormalData {
        <<Abstract ScriptableObject>>
        +string abnormalName
        +string abnormalDescription
        +abstract ApplyAbnormal(mapRoot)
        #FindTarget(mapRoot, targetName) Transform
    }

    class CreateAbnormalData {
        +List~SpawnInfo~ spawnList
        +override ApplyAbnormal()
    }
    class DeleteAbnormalData {
        +List~string~ targetObjectNames
        +override ApplyAbnormal()
    }
    class ReplaceAbnormalData {
        +List~ReplaceInfo~ replaceList
        +override ApplyAbnormal()
    }
    class ScaleAbnormalData {
        +List~ScaleInfo~ scaleList
        +override ApplyAbnormal()
        -ApplyInstantMode()
        -ApplyGradualMode()
    }
    class SoundAbnormalData {
        +TargetType targetType
        +SoundMode soundMode
        +override ApplyAbnormal()
    }
    class NPCTransformAbnormalData {
        +string targetName
        +string smileBlendShapeName
        +float smileTargetWeight
        +override ApplyAbnormal()
    }
    note for NPCTransformAbnormalData "모델 교체가 아닌\n블렌드셰이프 가중치 조절"

    class ObjectScaler {
        <<런타임 동적 주입>>
        +StartScaling(targetScale, duration)
        -ScaleRoutine() IEnumerator
    }

    class SpawnAbnormalManager {
        <<Singleton>>
        -List~AbnormalData~ abnormalDatas
        -float AbnormalRate
        +GameObject mapRoot
        +SelectAbnormal() AbnormalData
    }

    class UIBinder {
        <<Static>>
    }

    AbnormalData <|-- CreateAbnormalData
    AbnormalData <|-- DeleteAbnormalData
    AbnormalData <|-- ReplaceAbnormalData
    AbnormalData <|-- ScaleAbnormalData
    AbnormalData <|-- SoundAbnormalData
    AbnormalData <|-- NPCTransformAbnormalData

    SpawnAbnormalManager o-- AbnormalData : 확률 추첨
    ScaleAbnormalData ..> ObjectScaler : AddComponent 주입
    AbnormalData ..> UIBinder : DFS 탐색
```

---

## 4. 설정 시스템 (Observer)

`SettingManager`가 설정 변경을 이벤트로 브로드캐스트하면, 각 적용기가 **자신에게 필요한 항목만 독립적으로 구독·적용**합니다.

```mermaid
classDiagram
    class SettingManager {
        <<Singleton>>
        +static event Action~GameSetting~ OnSettingsApplied$
        +GameSetting Current
        -static Bootstrap()
        +Load()
        -ReadFromDisk() GameSetting
        -Migrate(loaded) GameSetting
        +Commit(draft) bool
        -WriteToDisk() bool
        -QuarantineBrokenFile()
    }
    note for SettingManager "임시파일 → File.Replace 원자적 저장\n손상 시 격리 후 기본값 복구"

    class GameSetting {
        <<Serializable · Testable>>
        +int version
        +string languageCode
        +int qualityLevel
        +int displayModeIndex
        +int resolutionIndex
        +float masterVolume
        +float bgmVolume
        +float sfxVolume
        +float mouseSensitivity
        +float cameraAccel
        +float cameraShake
        +Clone() GameSetting
        +Validate()
        +IsSameAs(other) bool
    }

    class DisplayOptions {
        <<Static>>
        +static FullScreenMode[] DisplayModes
        +static ResolutionNames
        +GetCurrentResolutionIndex() int
        +ResolveResolutionIndex(stored) int
    }

    class SettingApplierBase {
        <<Abstract MonoBehaviour>>
        #virtual OnEnable()
        #virtual OnDisable()
        -HandleApplied(settings)
        #abstract Apply(settings)
    }

    class AudioVolumeApplier {
        #override Apply()
        -SetVolume(mixer, param, linear)
    }
    class GraphicPresetApplier {
        #override Apply()
    }
    class DisplayApplier {
        #override Apply()
    }
    class LanguageApplier {
        #override Apply()
        +static SelectLocale(code)
    }
    class CameraLook {
        +bool IsLookEnabled
        #override Apply()
    }
    class HeadBob {
        -float shakeScale
        #override Apply()
    }

    class SettingPanel {
        <<MonoBehaviour>>
        -GameSetting draft
        +event Action Closed
        +bool IsOpen
        -AutoBindUI()
        +Open()
        +Close()
        +HandleCancelInput() bool
        -MarkDirty()
        -OnApply()
        -OnCancel()
        -OnDefault()
    }
    note for SettingPanel "draft 사본에 편집 후\nApply 시에만 Commit"

    class LanguageSelector {
        +event Action~string~ Selected
        +SetWithoutNotify(localeCode)
    }

    SettingApplierBase <|-- AudioVolumeApplier
    SettingApplierBase <|-- GraphicPresetApplier
    SettingApplierBase <|-- DisplayApplier
    SettingApplierBase <|-- LanguageApplier
    SettingApplierBase <|-- CameraLook
    SettingApplierBase <|-- HeadBob

    SettingManager o-- GameSetting : Current
    SettingApplierBase ..> SettingManager : OnSettingsApplied 구독
    SettingPanel --> SettingManager : Commit(draft)
    SettingPanel --> LanguageSelector
    SettingPanel ..> DisplayOptions
    GameSetting ..> DisplayOptions : 유효성 검사
```

---

## 5. 로컬라이제이션

한국어 · 영어 · 일본어 · 중국어(간체) 4개 언어를 지원하며, 번역 누락 시 **폴백 체인**으로 빈 화면을 방지합니다.

```mermaid
classDiagram
    class Loc {
        <<Static Facade>>
        +const string UITable
        +const string StoryTable
        +static bool IsReady
        +UI(key) string
        +Story(key) string
        +EnsureReady() IEnumerator
        -Resolve(table, key, args) string
        -TryGetFrom(table, key, locale, args) string
        +static CurrentLocaleCode
    }
    note for Loc "폴백 체인\n현재 언어 → 영어 → 한국어"

    class GameLanguages {
        <<Static>>
        +const string Korean
        +const string English
        +const string Japanese
        +const string ChineseSimplified
        +static string[] Supported
        +GetLanguageName(code) string
        +IsSupported(code) bool
        +SetLanguageOnSystem(system) string
    }

    class TextSizeSynchronizer {
        <<MonoBehaviour>>
        -List~Text~ targetTexts
        -int minSizeLimit
        -int maxSizeLimit
        +Synchronize()
        -SyncRoutine() IEnumerator
        -OnLocaleChanged(locale)
    }
    note for TextSizeSynchronizer "언어별 글자 길이 차이에 맞춰\n텍스트 크기 동기화"

    class LanguageApplier {
        #override Apply()
        +static SelectLocale(code)
    }

    Loc ..> GameLanguages : 폴백 로케일 조회
    LanguageApplier ..> GameLanguages
    TextSizeSynchronizer ..> Loc : 로케일 변경 구독
```

---

## 6. 캐릭터 & 엘리베이터

입력·이동·발소리를 컴포넌트로 분리하고, 엘리베이터는 **정적 이벤트로 정답 선택을 발행**합니다.

```mermaid
classDiagram
    class PlayerInput {
        <<RequireComponent PlayerMovement>>
        -PlayerMovement playerMovement
        -HandleMovementInput()
    }

    class PlayerMovement {
        <<RequireComponent CharacterController, FootstepController>>
        -float walkSpeed
        -float runSpeed
        -float gravity
        +bool canMove
        +Move(moveInput, isRunning)
        -ApplyGravity()
    }

    class NPCMovement {
        <<RequireComponent NavMeshAgent, FootstepController>>
        -Transform[] waypoints
        -int currentWaypoint
        -NavMeshAgent navMeshAgent
        -Animator animator
        +bool opening
        -CheckWayPointArrival()
        -HandleFootsteps()
        +LookAtTarget(targetPos)
    }

    class FootstepController {
        -AudioClip walkSound
        -float defaultWalkDuration
        -bool isForceStopped
        -bool isMuted
        -bool isDoubleSound
        +CalculateAndPlayFootstep(isMoving, speedRatio)
        -PlayDoubleSoundRoutine() IEnumerator
        +StopFootsteps()
        +SetAbnormalStatus(mute, doubleSound)
    }
    note for FootstepController "이상현상 연동\n음소거 / 발소리 2회 재생"

    class CameraLook {
        +bool IsLookEnabled
        -float sensitivity
        -float pitch
        +ResetPitch()
        +ResyncFromTransforms()
    }

    class ElevatorController {
        +static bool IsTeleporting$
        +TriggerType type
        -float detectionDistance
        -Collider innerTriggerCollider
        -Transform standPoint
        +bool isOpen
        +static event Action~TriggerType~ OnElevatorAnswerSelected$
        +InitializeFirstTriggerState(playerPos)
        +PlayerEnteredInnerTrigger()
        +PlayerExitedInnerTrigger()
        -ElevatorSequenceCoroutine() IEnumerator
        +SetDoors(shouldOpen) IEnumerator
        -MovePlayerToStandPoint() IEnumerator
    }
    note for ElevatorController "IsTeleporting 플래그로\n순간이동 직후 오탐 트리거 차단"

    class ElevatorButton {
        -bool isPlayerInTrigger
        +static event Action~bool~ OnPlayerNearButton$
        -AutoBindUI()
    }

    class ElevatorTrigger {
        -ElevatorController elevatorController
        -AutoBindUI()
    }

    class AnimatorParams {
        <<Static>>
    }

    PlayerInput --> PlayerMovement : controls
    PlayerMovement --> FootstepController : uses
    NPCMovement --> FootstepController : uses
    NPCMovement ..> AnimatorParams
    ElevatorController ..> AnimatorParams
    ElevatorController --> CameraLook : 시퀀스 중 제어
    ElevatorButton --> ElevatorController
    ElevatorTrigger --> ElevatorController
```

---

## 7. UI 계층

`BaseUIManager<T>`가 싱글톤·자동 바인딩·초기화를 공통 처리하고, 엔딩 UI는 한 단계 더 추상화했습니다.

```mermaid
classDiagram
    class BaseUIManager_T {
        <<Abstract>>
        +static T Instance$
        #abstract AutoBindUI()
        #virtual InitializeUI()
        #static PlayButtonSound()
    }

    class BaseEndingUIManager_T {
        <<Abstract>>
        #GameObject endingPanel
        #TypewriterText typewriter
        #string[] monologueKeys
        #abstract string EndingPanelName
        #override AutoBindUI()
        +PlayMonologueSequence() IEnumerator
        #abstract OnMonologueFinished()
    }

    class BadEndingUIManager {
        #override string EndingPanelName
        #override OnMonologueFinished()
    }
    class TrueEndingUIManager {
        #override string EndingPanelName
        #override OnMonologueFinished()
    }

    class StoryModeUIManager {
        -Text elevatorText
        -GameObject menuUI
        -SettingPanel settingPanel
        -TypewriterText monologueTypewriter
        -string[] monologueKeys
        -string[] loopResetKeys
        -HandleFloorFirstVisited(floor)
        -HandleLoopReset()
        -ToggleMenu(isVisible)
        +OnClickContinue()
        +OnClickSetting()
        +OnClickGoToTitle()
    }

    class MainMenuUIManager {
        -GameObject descriptionPanel
        -SettingPanel settingPanel
        -GraphicRaycaster raycaster
        +SetUIInteractable(state)
        +OnClickStart()
        +OnClickDescription()
        +OnClickSetting()
    }

    class EndingCreditUIManager {
        -Text roleText
        -Text nameText
        -Button skipButton
        -string[] roleKeys
        -string[] nameKeys
        +OnClickSkipButton()
        -PlayCreditSequenceRoutine() IEnumerator
        -FadeTextAlpha(...) IEnumerator
    }

    BaseUIManager_T <|-- BaseEndingUIManager_T
    BaseUIManager_T <|-- StoryModeUIManager
    BaseUIManager_T <|-- MainMenuUIManager
    BaseUIManager_T <|-- EndingCreditUIManager
    BaseEndingUIManager_T <|-- BadEndingUIManager
    BaseEndingUIManager_T <|-- TrueEndingUIManager
```

### UI 연출 컴포넌트

```mermaid
classDiagram
    class TypewriterText {
        <<RequireComponent Text>>
        -float charInterval
        -float holdDuration
        -StringBuilder builder
        +bool IsTyping
        +Play(content, onComplete) Coroutine
        +PlayAndKeep(content, onComplete) Coroutine
        +Stop()
        +Clear()
        +SkipToEnd(content)
    }
    note for TypewriterText "StringBuilder 재사용으로\n문자열 연결 GC 억제"

    class FloorNumberDisplay {
        -GameObject[] numberPrefabs
        -int maxDigits
        -GameObject[][] slots
        -List~int~ digitBuffer
        -InitializeObjectPool()
        +SetFloorNumber(floor)
        +ResetFloorNumber()
        -static GetDigits(number, result)
    }
    note for FloorNumberDisplay "슬롯 × 숫자(0-9) 사전 생성 후\n활성/비활성 토글 (오브젝트 풀링)"

    class ElevatorRideEffect {
        -float shakeAmount
        -float shakeSpeed
        -bool isMoving
        +StopElevator()
    }

    class SettingPanel {
        <<MonoBehaviour>>
        +event Action Closed
        +Open()
        +Close()
    }

    class StoryModeUIManager {
        <<UI Manager>>
    }
    class MainMenuUIManager {
        <<UI Manager>>
    }
    class MapSpawner {
        <<MonoBehaviour>>
    }

    StoryModeUIManager --> TypewriterText : 독백 연출
    StoryModeUIManager --> SettingPanel
    MainMenuUIManager --> SettingPanel
    MapSpawner --> FloorNumberDisplay : 층 표시
```

---

## 8. 사운드 · 연출 · 씬 시퀀스

```mermaid
classDiagram
    class SoundManager {
        <<Singleton>>
        -AudioMixer mixer
        -AudioMixerGroup bgmGroup
        -AudioMixerGroup sfxGroup
        -AudioSource bgmAudioSource
        -AudioSource sfxAudioSource
        -AudioSource ambienceAudioSource
        +AudioMixer Mixer
        -RouteToMixerGroups()
        +PlayBGM(clip, volume)
        +PlaySFX(clip, volume)
        +PlayAmbience(clip, volume)
        +PlayButtonSound()
        +StopAllSound()
        +PauseGameplay()
        +ResumeGameplay()
    }
    note for SoundManager "AudioMixerGroup 라우팅으로\n설정 볼륨과 연동"

    class FadeManager {
        <<Singleton>>
        -Image black
        -Image white
        +bool isFading
        -AutoBindImages()
        +SetAllBackground(state)
        +FadeOut(duration)
        +FadeIn(duration)
        +FlashOut(duration)
        +FlashIn(duration)
        -StartFadeCoroutine(...) IEnumerator
    }

    class MainMenuManager {
        -ElevatorRideEffect rideEffect
        -string nextSceneName
        +StartGameSequence()
    }
    class BadEndingManager {
        -float transferTime
        -BadEndingCoroutine() IEnumerator
        -PlayFadeAndAudioCoroutine() IEnumerator
    }
    class TrueEndingManager {
        -float endingWaitTime
        -TrueEndingCoroutine() IEnumerator
    }
    class EndingCreditManager {
        -bool isTransitioning
        +GoToMainMenu()
    }

    class BadEndingUIManager {
        <<UI Manager>>
    }
    class TrueEndingUIManager {
        <<UI Manager>>
    }
    class EndingCreditUIManager {
        <<UI Manager>>
    }
    class ElevatorRideEffect {
        <<MonoBehaviour>>
    }

    BadEndingManager --> BadEndingUIManager : 독백 시퀀스
    TrueEndingManager --> TrueEndingUIManager : 독백 시퀀스
    EndingCreditUIManager --> EndingCreditManager : 크레딧 종료 통보
    MainMenuManager --> ElevatorRideEffect

    BadEndingManager ..> FadeManager
    TrueEndingManager ..> FadeManager
    BadEndingManager ..> SoundManager
    TrueEndingManager ..> SoundManager
    EndingCreditManager ..> SoundManager
```

### 성능 계측

```mermaid
classDiagram
    class PerformanceLogger {
        <<MonoBehaviour>>
        -ProfilerRecorder cpuTotal, cpuMain, cpuRender, gpuTime
        -ProfilerRecorder dcStandard, dcStaticBatched, dcDynamicBatched
        -ProfilerRecorder batchStatic, batchDynamic, setPass
        -ProfilerRecorder tris, verts, shadowCasters
        -ProfilerRecorder gcAlloc, gcUsed, totalMem, texMem
        -StringBuilder sb
        -string csvPath
        -SaveAll()
    }
    note for PerformanceLogger "0.5초 간격 수집 → CSV 출력\n배칭 방식별 드로우콜 분리 계측"
```

---

## 9. 이벤트 흐름 (정적 이벤트)

시스템을 분리한 만큼, **직접 참조 대신 정적 이벤트**로 연결됩니다. 발행자는 구독자의 존재를 알지 못합니다.

```mermaid
flowchart LR
    ET["EndingTrigger"] -->|"OnEndingTriggered&lt;EndType&gt;"| ED["EndingDirector"]
    EC["ElevatorController"] -->|"OnElevatorAnswerSelected&lt;TriggerType&gt;"| GM["GameManager"]
    EB["ElevatorButton"] -->|"OnPlayerNearButton&lt;bool&gt;"| SU["StoryModeUIManager"]
    GM -->|"OnFloorFirstVisited&lt;int&gt;"| SU
    GM -->|"OnLoopReset"| SU
    SM["SettingManager"] -->|"OnSettingsApplied&lt;GameSetting&gt;"| AP["SettingApplierBase<br/>파생 6종"]
    LS["LanguageSelector"] -->|"Selected&lt;string&gt;"| SP["SettingPanel"]
    SP -->|"Closed"| SU

    style ET fill:#ffe8e8,stroke:#d95a5a
    style EC fill:#ffe8e8,stroke:#d95a5a
    style EB fill:#ffe8e8,stroke:#d95a5a
    style SM fill:#ffe8e8,stroke:#d95a5a
    style LS fill:#ffe8e8,stroke:#d95a5a
    style GM fill:#e8f4ff,stroke:#4a90d9
```

---

## 📋 설계 요약

| 계층 | 핵심 클래스 | 설계 의도 |
|---|---|---|
| **Game Core** | `GameManager` + `FloorProgress` · `FloorRule` · `MapSpawner` · `EndingDirector` | God-class를 **순수 로직(POCO/static)** 과 **Unity 협력자**로 분리 → 단일 책임 & 단위 테스트 가능 |
| **Anomaly** | `AbnormalData` + 구체 클래스 6종 | 추상 클래스 상속으로 이상현상 확장 시 기존 코드 수정 불필요(OCP) |
| **Settings** | `SettingManager` → `SettingApplierBase` 파생 6종 | `OnSettingsApplied` 브로드캐스트(Observer)로 각 시스템이 독립 구독 |
| **Localization** | `Loc` · `GameLanguages` | 정적 파사드 + 폴백 체인으로 번역 누락에도 안전 |
| **UI** | `BaseUIManager<T>` / `BaseEndingUIManager<T>` | 싱글톤·자동 바인딩·초기화 공통 로직을 제네릭 기반 클래스로 통일 |
| **공통 유틸** | `UIBinder`(Stack DFS) · `AnimatorParams` | 반복 로직을 정적 유틸로 추출해 중복 제거 |
| **Profiling** | `PerformanceLogger` | `ProfilerRecorder`로 드로우콜·삼각형·GC를 CSV 계측(정량 지표 근거) |

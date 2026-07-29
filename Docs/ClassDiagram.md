# 📐 클래스 다이어그램 — 지하 10층 (Basement_10)

> 실제 구현 코드(`Scripts/`) 기준으로 작성한 전체 구조도입니다.
> 핵심 설계 축은 **① God-class 분리(GameManager → 순수 로직 + MonoBehaviour 협력자)**,
> **② 추상화 기반 확장(AbnormalData / SettingApplierBase / BaseUIManager)**,
> **③ 정적 이벤트를 통한 느슨한 결합**입니다.

```mermaid
classDiagram
    %% ============================================================
    %% [1] Core & Base — 재사용 기반 계층
    %% ============================================================
    class Singleton_T {
        <<Abstract>>
        -static T instance
        -static bool isQuitting
        +static T Instance$
        +static bool HasInstance$
        #virtual Awake()
        #virtual OnDestroy()
        -OnApplicationQuit()
    }

    class BaseUIManager_T {
        <<Abstract>>
        -static T instance
        +static T Instance$
        +static bool HasInstance$
        #virtual Awake()
        #virtual OnDestroy()
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

    class AnimatorParams {
        <<Static>>
        +static int Opening$
        +static int MainDoorOpen$
        +static int ElevatorDoorOpen$
    }

    %% ============================================================
    %% [2] Game Core — GameManager 분리(SRP) & 테스트 가능한 순수 로직
    %% ============================================================
    Singleton_T <|-- GameManager

    class GameManager {
        <<RequireComponent MapSpawner, EndingDirector>>
        -int startFloor
        -int targetFloor
        -Transform mapSpawnPoint
        -Vector3 playerSpawnPosition
        -Quaternion playerSpawnRotation
        +bool showFloorNumber
        +int CurrentFloor
        +bool isEnded
        +GameObject player
        +static event Action~int~ OnFloorFirstVisited$
        +static event Action OnLoopReset$
        -FloorProgress progress
        -MapSpawner mapSpawner
        -EndingDirector endingDirector
        #override Awake()
        -OnSceneLoaded(scene, mode)
        +StartLoop()
        -ResetPlayerPositionRoutine() IEnumerator
        -RaiseFloorEvents()
        +CheckAnswer(choice)
    }

    class FloorProgress {
        <<POCO / Testable>>
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
        +HasVisited(floor) bool
    }

    class FloorRule {
        <<Static / Testable>>
        +IsCorrect(choice, hasAbnormal) bool
        +DecideNextMap(currentFloor, startFloor, isCorrect) int
        +ChoiceMap(currentFloor, startFloor, targetFloor, isEndingScene) MapInfo
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
        -Vector3 finalMapOffset
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
        -float badEndingFadeDuration
        -float trueEndingFlashDuration
        +bool IsEnded
        +string BadEndingSceneName
        +ResetState()
        +Play(type)
        -EndingSequenceCoroutine(type) IEnumerator
    }

    %% ============================================================
    %% [3] Global Singleton Managers
    %% ============================================================
    Singleton_T <|-- SoundManager
    Singleton_T <|-- FadeManager
    Singleton_T <|-- SpawnAbnormalManager
    Singleton_T <|-- SettingManager

    class SoundManager {
        -AudioMixer mixer
        -AudioMixerGroup bgmGroup
        -AudioMixerGroup sfxGroup
        -AudioSource bgmAudioSource
        -AudioSource sfxAudioSource
        -AudioSource ambienceAudioSource
        +AudioMixer Mixer
        +AudioClip BadEndingBGM
        +AudioClip EyeOpeningBGM
        +AudioClip TrueEndingBGM
        +AudioClip EndingCreditBGM
        -RouteToMixerGroups()
        +PlayBGM(clip, volume)
        +PlaySFX(clip, volume)
        +PlayAmbience(clip, volume)
        +PlayButtonSound()
        +PlayElevatorDoorSound()
        +PlayElevatorMovingSound()
        +PlayElevatorFinishSound()
        +StopBGM()
        +StopSFX()
        +StopAmbience()
        +StopAllSound()
        +PauseGameplay()
        +ResumeGameplay()
    }

    class FadeManager {
        -Image black
        -Image white
        +bool isFading
        -Coroutine currentFadeCoroutine
        -AutoBindImages()
        +SetAllBackground(state)
        +SetWhiteBackGround(state)
        +SetBlackBackGround(state)
        +FadeOut(duration)
        +FadeIn(duration)
        +FlashOut(duration)
        +FlashIn(duration)
        -StartFadeCoroutine(...) IEnumerator
    }

    class SpawnAbnormalManager {
        -List~AbnormalData~ abnormalDatas
        -float AbnormalRate
        +GameObject mapRoot
        +SelectAbnormal() AbnormalData
    }

    class SettingManager {
        +static event Action~GameSetting~ OnSettingsApplied$
        +GameSetting Current
        -static string SavePath
        -static Bootstrap()
        +Load()
        -ReadFromDisk() GameSetting
        -Migrate(loaded) GameSetting
        +Commit(draft) bool
        -WriteToDisk() bool
        -QuarantineBrokenFile()
    }

    %% ============================================================
    %% [4] Settings System — Observer(설정 브로드캐스트) + 데이터 모델
    %% ============================================================
    class GameSetting {
        <<Serializable / Testable>>
        +const int CurrentVersion
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
        +static CreateDefault() GameSetting
        +Validate()
        +IsSameAs(other) bool
    }

    class DisplayOptions {
        <<Static>>
        +static FullScreenMode[] DisplayModes
        +static string[] DisplayModeKeys
        +static int ResolutionCount
        +static Resolutions
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
        -AudioMixer mixer
        #override Apply(settings)
    }
    class CameraLook {
        +bool IsLookEnabled
        -float sensitivity
        -float accelAmount
        -float pitch
        #override Apply(setting)
        +ResetPitch()
        +ResyncFromTransforms()
    }
    class DisplayApplier {
        #override Apply(settings)
    }
    class GraphicPresetApplier {
        #override Apply(settings)
    }
    class HeadBob {
        -float shakeScale
        #override Apply(settings)
    }
    class LanguageApplier {
        #override Apply(settings)
        +static SelectLocale(code)
    }

    class LanguageSelector {
        +event Action~string~ Selected
        +SetWithoutNotify(localeCode)
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
        -RefreshUI()
        -MarkDirty()
        -OnApply()
        -OnCancel()
        -OnDefault()
    }

    SettingApplierBase <|-- AudioVolumeApplier
    SettingApplierBase <|-- CameraLook
    SettingApplierBase <|-- DisplayApplier
    SettingApplierBase <|-- GraphicPresetApplier
    SettingApplierBase <|-- HeadBob
    SettingApplierBase <|-- LanguageApplier

    %% ============================================================
    %% [5] Localization — 4개 언어, 폴백 체인
    %% ============================================================
    class Loc {
        <<Static>>
        +const string UITable
        +const string StoryTable
        +static bool IsReady
        +UI(key) string
        +Story(key) string
        +EnsureReady() IEnumerator
        +Get(table, key) string
        -Resolve(table, key, args) string
        +static CurrentLocaleCode
    }
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

    %% ============================================================
    %% [6] Anomaly System — 다형성 기반 이상현상(ScriptableObject)
    %% ============================================================
    class AbnormalData {
        <<Abstract ScriptableObject>>
        +string abnormalName
        +string abnormalDescription
        +abstract ApplyAbnormal(mapRoot)
        #FindTarget(mapRoot, targetName) Transform
    }
    AbnormalData <|-- CreateAbnormalData
    AbnormalData <|-- DeleteAbnormalData
    AbnormalData <|-- ReplaceAbnormalData
    AbnormalData <|-- ScaleAbnormalData
    AbnormalData <|-- SoundAbnormalData
    AbnormalData <|-- NPCTransformAbnormalData

    class CreateAbnormalData {
        +List~SpawnInfo~ spawnList
        +override ApplyAbnormal(mapRoot)
    }
    class DeleteAbnormalData {
        +List~string~ targetObjectNames
        +override ApplyAbnormal(mapRoot)
    }
    class ReplaceAbnormalData {
        +List~ReplaceInfo~ replaceList
        +override ApplyAbnormal(mapRoot)
    }
    class ScaleAbnormalData {
        +List~ScaleInfo~ scaleList
        +override ApplyAbnormal(mapRoot)
    }
    class SoundAbnormalData {
        +TargetType targetType
        +SoundMode soundMode
        +string targetName
        +override ApplyAbnormal(mapRoot)
    }
    class NPCTransformAbnormalData {
        +string targetName
        +string smileBlendShapeName
        +float smileTargetWeight
        +override ApplyAbnormal(mapRoot)
    }
    class ObjectScaler {
        <<MonoBehaviour / 동적 주입>>
        +StartScaling(targetScale, duration)
        -ScaleRoutine(targetScale, duration) IEnumerator
    }

    %% ============================================================
    %% [7] Character — 입력 / 이동 / 발소리 / 카메라
    %% ============================================================
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
        -float walkSpeed
        -NavMeshAgent navMeshAgent
        -Animator animator
        +bool opening
        -float lookRotationSpeed
        -CheckWayPointArrival()
        -HandleFootsteps()
        -UpdateAnimator()
        +LookAtTarget(targetPos)
    }
    class FootstepController {
        -AudioClip walkSound
        -float defaultWalkDuration
        -float volume
        -float doubleSoundDelay
        -bool isForceStopped
        -bool isMuted
        -bool isDoubleSound
        +CalculateAndPlayFootstep(isMoving, speedRatio)
        -PlayFootstep()
        -PlayDoubleSoundRoutine() IEnumerator
        +StopFootsteps()
        +SetAbnormalStatus(mute, doubleSound)
    }

    %% ============================================================
    %% [8] Elevator & Interaction — 정적 이벤트 트리거
    %% ============================================================
    class ElevatorController {
        +static bool IsTeleporting$
        +TriggerType type
        -float detectionDistance
        -Animator animator
        -Animator parentAnimator
        -Transform playerTransform
        -PlayerMovement playerMovement
        -Collider innerTriggerCollider
        -Transform standPoint
        -CameraLook cameraLook
        +bool isOpen
        +static event Action~TriggerType~ OnElevatorAnswerSelected$
        +InitializeFirstTriggerState(playerPosition)
        +PlayerEnteredInnerTrigger()
        +PlayerExitedInnerTrigger()
        -HandleElevatorLogic()
        -ElevatorSequenceCoroutine() IEnumerator
        +SetDoors(shouldOpen) IEnumerator
        -MovePlayerToStandPoint() IEnumerator
        -UpdateAnimators(state)
    }
    class ElevatorButton {
        -ElevatorController elevatorController
        -bool isPlayerInTrigger
        +static event Action~bool~ OnPlayerNearButton$
        -AutoBindUI()
    }
    class ElevatorTrigger {
        -ElevatorController elevatorController
        -AutoBindUI()
    }
    class EndingTrigger {
        -EndType endType
        -bool isTriggered
        +static event Action~EndType~ OnEndingTriggered$
    }

    %% ============================================================
    %% [9] UI / Presentation
    %% ============================================================
    BaseUIManager_T <|-- BaseEndingUIManager_T
    BaseUIManager_T <|-- EndingCreditUIManager
    BaseUIManager_T <|-- MainMenuUIManager
    BaseUIManager_T <|-- StoryModeUIManager
    BaseEndingUIManager_T <|-- BadEndingUIManager
    BaseEndingUIManager_T <|-- TrueEndingUIManager

    class BaseEndingUIManager_T {
        <<Abstract>>
        #GameObject endingPanel
        #TypewriterText typewriter
        #string[] monologueKeys
        #abstract string EndingPanelName
        #override AutoBindUI()
        #override InitializeUI()
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
    class EndingCreditUIManager {
        -GameObject blackBackgroundPanel
        -Text roleText
        -Text nameText
        -Button skipButton
        -string[] roleKeys
        -string[] nameKeys
        #override AutoBindUI()
        #override InitializeUI()
        +OnClickSkipButton()
        -PlayCreditSequenceRoutine() IEnumerator
        -FadeTextAlpha(...) IEnumerator
        -FinishCredits()
    }
    class MainMenuUIManager {
        -GameObject descriptionPanel
        -SettingPanel settingPanel
        -MainMenuManager mainMenuManager
        -GraphicRaycaster raycaster
        #override AutoBindUI()
        +SetUIInteractable(state)
        +OnClickStart()
        +OnClickDescription()
        +OnClickSetting()
        +OnClickCloseDescription()
        +OnClickExit()
    }
    class StoryModeUIManager {
        -Text elevatorText
        -GameObject menuUI
        -SettingPanel settingPanel
        -TypewriterText monologueTypewriter
        -string[] monologueKeys
        -string[] loopResetKeys
        -PlayerMovement playerMovement
        -bool menuActivated
        #override AutoBindUI()
        #override InitializeUI()
        -HandleFloorFirstVisited(floor)
        -HandleLoopReset()
        -ShowMonologue(key)
        -ToggleMenu(isVisible)
        +OnClickContinue()
        +OnClickSetting()
        +OnClickGoToTitle()
        +OnClickExit()
    }
    class TypewriterText {
        <<RequireComponent Text>>
        +bool IsTyping
        +Play(content, onComplete) Coroutine
        +PlayAndKeep(content, onComplete) Coroutine
        +Stop()
        +Clear()
        +SkipToEnd(content)
        -PlayRoutine(...) IEnumerator
    }
    class TextSizeSynchronizer {
        -List~Text~ targetTexts
        +Synchronize()
        -SyncRoutine() IEnumerator
        -OnLocaleChanged(locale)
    }
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
    class ElevatorRideEffect {
        -float shakeAmount
        -float shakeSpeed
        -Vector3 initialPosition
        -bool isMoving
        +StopElevator()
    }

    %% ============================================================
    %% [10] Scene Sequence Managers
    %% ============================================================
    class BadEndingManager {
        <<MonoBehaviour>>
        -string nextSceneName
        -float transferTime
        -BadEndingCoroutine() IEnumerator
    }
    class TrueEndingManager {
        <<MonoBehaviour>>
        -string nextSceneName
        -float endingWaitTime
        -TrueEndingCoroutine() IEnumerator
    }
    class EndingCreditManager {
        <<MonoBehaviour>>
        -bool isTransitioning
        -string mainMenuScene
        +GoToMainMenu()
        -ReturnToMainMenuCoroutine() IEnumerator
    }
    class MainMenuManager {
        <<MonoBehaviour>>
        -ElevatorRideEffect rideEffect
        -string nextSceneName
        +StartGameSequence()
        +StartGameSequenceCoroutine() IEnumerator
    }

    %% ============================================================
    %% [11] Profiling — 정량 지표 계측(CSV)
    %% ============================================================
    class PerformanceLogger {
        <<MonoBehaviour>>
        -ProfilerRecorder cpuTotal, cpuRender, gpuTime
        -ProfilerRecorder dcStandard, dcStaticBatched, dcDynamicBatched
        -ProfilerRecorder tris, verts, shadowCasters
        -ProfilerRecorder gcAlloc, totalMem, texMem
        -StringBuilder sb
        -string csvPath
        -SaveAll()
    }

    %% ============================================================
    %% [12] Unit Tests — 순수 로직 분리로 확보한 EditMode 테스트 (총 28개)
    %% ============================================================
    class FloorProgressTests {
        <<Test · 8 cases>>
    }
    class FloorRuleTests {
        <<Test · 10 cases>>
    }
    class GameSettingTests {
        <<Test · GameSetting/GameLanguages 10 cases>>
    }

    %% ============================================================
    %% [13] Key Relationships & Dependencies
    %% ============================================================
    %% -- Game Core 협력 (분리된 책임)
    GameManager *-- FloorProgress : owns (순수 로직)
    GameManager --> MapSpawner : RequireComponent
    GameManager --> EndingDirector : RequireComponent
    FloorProgress ..> FloorRule : 규칙 위임
    FloorRule ..> MapInfo : returns
    MapSpawner ..> FloorRule : MapInfo 사용
    MapSpawner --> SpawnAbnormalManager : 이상현상 요청
    MapSpawner o-- AbnormalData : current
    MapSpawner --> FloorNumberDisplay : 층 표시
    SpawnAbnormalManager o-- AbnormalData : collection

    %% -- 이벤트 기반 느슨한 결합
    EndingTrigger ..> EndingDirector : OnEndingTriggered
    ElevatorController ..> GameManager : OnElevatorAnswerSelected
    ElevatorButton ..> StoryModeUIManager : OnPlayerNearButton
    GameManager ..> StoryModeUIManager : OnFloorFirstVisited / OnLoopReset

    %% -- Settings (Observer)
    SettingManager o-- GameSetting : current
    SettingApplierBase ..> SettingManager : OnSettingsApplied 구독
    SettingPanel --> SettingManager : Commit(draft)
    SettingPanel --> LanguageSelector
    SettingPanel ..> DisplayOptions

    %% -- Localization
    Loc ..> GameLanguages
    BaseEndingUIManager_T ..> Loc
    StoryModeUIManager ..> Loc

    %% -- Character / Elevator
    PlayerInput --> PlayerMovement : controls
    PlayerMovement --> FootstepController : uses
    NPCMovement --> FootstepController : uses
    NPCMovement ..> AnimatorParams
    ElevatorController ..> AnimatorParams
    ElevatorController --> CameraLook : 시퀀스 중 제어
    ElevatorButton --> ElevatorController
    ElevatorTrigger --> ElevatorController

    %% -- Anomaly 동적 확장
    ScaleAbnormalData ..> ObjectScaler : AddComponent 주입
    SoundAbnormalData ..> FootstepController : 상태 주입
    AbnormalData ..> UIBinder : DFS 탐색

    %% -- Scene / UI 시퀀스
    BadEndingManager --> BadEndingUIManager : 독백 시퀀스
    TrueEndingManager --> TrueEndingUIManager : 독백 시퀀스
    MainMenuUIManager --> MainMenuManager
    MainMenuManager --> ElevatorRideEffect
    EndingCreditUIManager --> EndingCreditManager

    %% -- 공통 유틸
    BaseUIManager_T ..> UIBinder : 버튼/요소 바인딩

    %% -- 테스트 대상
    FloorProgressTests ..> FloorProgress : verifies
    FloorRuleTests ..> FloorRule : verifies
    GameSettingTests ..> GameSetting : verifies
```

---

## 설계 요약

| 계층 | 핵심 클래스 | 설계 의도 |
|---|---|---|
| **Game Core** | `GameManager` + `FloorProgress` · `FloorRule` · `MapSpawner` · `EndingDirector` | 비대해진 `GameManager`를 **순수 로직(POCO/static)** 과 **Unity 협력자(MonoBehaviour)** 로 분리 → 단일 책임 & 단위 테스트 가능 |
| **Anomaly** | `AbnormalData` + 6개 구체 클래스 | 추상 클래스 상속으로 이상현상 확장 시 기존 코드 수정 불필요(OCP) |
| **Settings** | `SettingManager` → `SettingApplierBase` 파생 6종 | `OnSettingsApplied` 이벤트 브로드캐스트(Observer)로 설정 적용을 각 시스템이 독립 구독 |
| **UI** | `BaseUIManager<T>` / `BaseEndingUIManager<T>` | 싱글톤·자동 바인딩·초기화 공통 로직을 제네릭 기반 클래스로 통일 |
| **공통 유틸** | `UIBinder` (Stack 기반 DFS) · `AnimatorParams` · `Loc` | 반복 로직을 정적 유틸로 추출해 중복 제거 |
| **Profiling** | `PerformanceLogger` | `ProfilerRecorder`로 드로우콜·삼각형·GC를 CSV로 계측(정량 지표 근거) |

> ℹ️ 씬 전환·엔딩 연출 관리자(`BadEndingManager` 등)와 `PerformanceLogger`는 정적 이벤트/씬 단위로 동작하므로 상단 협력 관계에서는 일부만 표기했습니다.

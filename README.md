# Basement10_portfolio
# **🕹️ [프로젝트] 지하 10층**

> **"8번 출구 게임과 영화에서 영감을 얻어, 스토리를 추가하고 다형성과 데이터 주도 설계로 재해석한 1인 개발 호러 퍼즐 게임입니다."**

---

## **📌 프로젝트 개요**

- **개발 인원**: 1인 개발 (기획, 프로그래밍, 리소스 관리)
- **개발 기간**: 2025.11 ~ 2026.03
- **기술 스택**: Unity, C#, NavMesh, ScriptableObject
- **핵심 컨셉**: 8번 출구 게임에 스토리를 추가하여 재해석한 게
- **주요 성과**: SOLID 원칙 기반 시스템 구축 및 알고리즘 및 자료구조 최적화를 통한 성능 개선

---

## **🛠️ 시스템 아키텍처**

컴퓨터공학과 전공자로서 **유지보수성과 확장성**을 고려한 설계를 지향했습니다.

### **1. 매니저 패턴 & 싱글톤**

- `Singleton<T>` 베이스 클래스를 상속받아 `GameManager`, `SoundManager`, `FadeManager` 등 핵심 시스템의 단일성을 보장하고 전역 접근성을 확보했습니다.
https://github.com/dbwoaud/Basement10_portfolio/blob/e5ac1c0c0a6ba592b8e7c257b6af012b6548442e/Scripts/Core/Singleton.cs#L5-L22
- [🔗 **Singleton.cs 코드 보기**](https://github.com/dbwoaud/Basement10_portfolio/blob/76718a169e383d6a0661d766ecf0e189015d773f/Scripts/Core/Singleton.cs)
- 각 매니저는 단일 책임 원칙(SRP)을 준수하여 자신의 역할(사운드 재생, 페이드 효과, 게임 루프 제어)에만 집중하도록 설계되었습니다.

### **2. 이벤트 기반 & 느슨한 결합**

- `Action`과 이벤트를 활용해 시스템 간 직접 참조를 최소화했습니다.
- 예를 들어, `EndingTrigger`는 엔딩 로직을 직접 실행하지 않고 이벤트를 발생시키며, 이를 `GameManager`가 구독하여 처리하는 **느슨한 결합** 구조를 취했습니다.
https://github.com/dbwoaud/Basement10_portfolio/blob/e5ac1c0c0a6ba592b8e7c257b6af012b6548442e/Scripts/Enviroment/EndingTrigger.cs#L11-L23
https://github.com/dbwoaud/Basement10_portfolio/blob/e5ac1c0c0a6ba592b8e7c257b6af012b6548442e/Scripts/Managers/GameManager.cs#L48-L58
- [🔗 **EndingTrigger.cs 코드 보기**](https://github.com/dbwoaud/Basement10_portfolio/blob/e5ac1c0c0a6ba592b8e7c257b6af012b6548442e/Scripts/Enviroment/EndingTrigger.cs#L13-L23)
- [🔗 **GameManager.cs 코드 보기**](https://github.com/dbwoaud/Basement10_portfolio/blob/701cac8f09ba2184baf4edd2bcdd0ad9ca7d0149/Scripts/Managers/GameManager.cs#L249-L276)

### **3. 다형성 기반의 이상 현상 시스템**

- **추상화:** `AbnormalData` 라는 추상 클래스를 설계하여 모든 이상 현상의 공통 동작 `ApplyAbnormal` 을 정의했습니다.
- **구체화**: 생성(`Create`), 삭제(`Delete`), 교체(`Replace`), 크기 변형(`Scale`), 사운드 변조(`Sound`) 등 각기 다른 로직을 자식 클래스에서 독립적으로 구현했습니다.
- 새로운 형태의 이상 현상을 추가할 때 기존 시스템 코드(예: `SpawnAbnormalManager`)를 수정할 필요 없이 새로운 클래스만 추가하면 되는 개방 폐쇄 원칙(OCP)을 실천했습니다.

### **4. 컴포넌트 기반의 동적 기능 확장**

- **구현**: `ScaleAbnormalData`에서 서서히 크기가 변하는 'Gradual' 모드 실행 시, 타겟 오브젝트에 `ObjectScaler` 컴포넌트를 실시간으로 주입(`AddComponent`)하여 기능을 확장했습니다.
- 모든 오브젝트에 무거운 스크립트를 미리 붙여두지 않고, 필요할 때만 기능을 활성화하여 메모리와 연산 효율을 높였습니다.
- [🔗 **ScaleAbnormalData.cs 코드 보기**](https://github.com/dbwoaud/Basement10_portfolio/blob/e5ac1c0c0a6ba592b8e7c257b6af012b6548442e/Scripts/Abnormal/ScaleAbnormalData.cs#)
- [🔗 **ObjectScaler.cs 코드 보기**](https://github.com/dbwoaud/Basement10_portfolio/blob/e5ac1c0c0a6ba592b8e7c257b6af012b6548442e/Scripts/Abnormal/ObjectScaler.cs#)

---


## **🚀 기술적 도전**

### **1. [최적화] 자료구조(Stack) 기반 DFS 탐색**

- **Problem**: 맵 오브젝트 내 수많은 하위 계층에서 특정 타겟을 찾을 때, 유니티 내장 `Find` 함수의 $*O(N)*$ 전수 조사로 인한 성능 저하 우려.
- **Solution**: `Stack<Transform>` 자료구조를 활용하여 재귀 없이 깊이 우선 탐색(DFS)을 직접 구현함으로써 탐색 효율을 $*O(M)*$ 수준으로 최적화했습니다.
https://github.com/dbwoaud/Basement10_portfolio/blob/e5ac1c0c0a6ba592b8e7c257b6af012b6548442e/Scripts/Core/AbnormalData.cs#L11-L30
- **Benefit**: 탐색 범위를 특정 루트 하위로 국소화하여 시간 복잡도를 *O*(*M*) (*M*≪*N*) 수준으로 낮추고, 재귀 호출 대신 반복문을 사용하여 스택 오버플로 위험을 방지했습니다.

### **2. [데이터 주도] ScriptableObject를 활용한 설계**

- `AbnormalData`를 `ScriptableObject`로 정의하여 게임의 로직과 데이터를 분리했습니다.
https://github.com/dbwoaud/Basement10_portfolio/blob/e5ac1c0c0a6ba592b8e7c257b6af012b6548442e/Scripts/Core/AbnormalData.cs#L3-L9
- 새로운 이상 현상(Abnormal) 추가 시 코드 수정 없이 데이터 파일 생성만으로 시스템에 즉시 반영되는 개방 폐쇄 원칙(OCP)을 실천했습니다.

### **3. [데이터 주도] 복합 데이터 구조를 활용한 연출 제어**

- **데이터화**: `struct`와 `List`를 활용해 교체 대상(`targetObjectName`)과 교체될 프리팹(`newGameObject`) 또는 목표 크기(`targetScale`) 등을 인스펙터에서 손쉽게 설정하도록 데이터화했습니다.
- **연출 유연성**: `enum`(`ScaleMode`, `SoundMode`)을 활용해 즉시 변경, 서서히 변경, 음소거, 중복 사운드 등 다양한 연출 옵션을 하나의 시스템에서 제어할 수 있습니다.

### **4. [UI 자동화] UI 바인딩 자동화 및 연출 제어**

- 전체적인 코드 내에서 `AutoBindUI()`를 통해 하위 오브젝트를 자동으로 탐색하고 할당함으로써 수동 할당의 번거로움과 휴먼 에러를 제거했습니다.
https://github.com/dbwoaud/Basement10_portfolio/blob/e5ac1c0c0a6ba592b8e7c257b6af012b6548442e/Scripts/Managers/FadeManager.cs#L23-L41
- [🔗 **FadeManager.cs 코드 보기**](https://github.com/dbwoaud/Basement10_portfolio/blob/e5ac1c0c0a6ba592b8e7c257b6af012b6548442e/Scripts/Managers/FadeManager.cs)
- `FadeManager` 내에서 코루틴(Coroutine)과 `WaitUntil`을 조합하여 페이드, 사운드, 씬 전환으로 이어지는 복잡한 시퀀스를 선언적으로 관리했습니다.

### **5. [시스템 통합] 사운드 및 로직 연동**

- **구현**: 플레이어와 NPC의 발소리 시스템(`FootstepController`)과 이상 현상 데이터를 연동하여 사운드 음소거 및 '발소리가 두 번 들리는 현상'을 구현했습니다.
- **도전**: `NPCMovement`와의 상호작용을 통해 NPC의 이동 상태에 따른 발소리 주기를 실시간으로 동기화했습니다.


---

## 📈 최적화 & 사용자 경험

- **가비지 컬렉션(GC) 최소화**: `FloorNumberDisplay`에서 객체 풀링 기법을 적용해 런타임 중 숫자 오브젝트 생성/파괴 비용을 줄였습니다.
https://github.com/dbwoaud/Basement10_portfolio/blob/e5ac1c0c0a6ba592b8e7c257b6af012b6548442e/Scripts/Enviroment/FloorNumberDisplay.cs#L55-L63
- [🔗 **FloorNumberDisplay.cs 코드 보기**](https://github.com/dbwoaud/Basement10_portfolio/blob/e5ac1c0c0a6ba592b8e7c257b6af012b6548442e/Scripts/Enviroment/FloorNumberDisplay.cs#)
- **코루틴 안정성**: `ObjectScaler`에서 새로운 스케일링 명령이 들어올 때 기존 코루틴을 중지(`StopAllCoroutines`)하여 로직 충돌을 방지하고 예측 가능한 동작을 보장했습니다.


---

## **⚠️ 트러블 슈팅: 런타임 모델 및 애니메이션 시스템 동적 교체**

### **1. 문제 상황**

- **상황**: 1인 개발 과정에서 NPC의 외형이 실시간으로 변하는 이상 현상을 구현하기 위해, 기존 NPC의 3D 모델과 애니메이션을 런타임에 통째로 교체해야 하는 과제가 발생했습니다.
- **어려움**: 단순히 오브젝트를 교체하는 것과 달리, 애니메이션이 적용된 모델은 `SkinnedMeshRenderer`, `Avatar`, `RuntimeAnimatorController` 등 복잡한 종속 관계를 가지고 있어 기존 방식을 적용하면 애니메이션이 깨지거나 모델이 비정상적으로 출력되는 문제가 발생했습니다.

### **2. 원인 분석 및 기술적 한계**

- **전문 지식의 부재**: 클라이언트 프로그래머로서 로직 구현에는 익숙했지만, 애니메이션 프로그래머가 전문적으로 다루는 뼈대 구조와 아바타 시스템에 대한 이해가 부족했습니다.
- **컴포넌트 종속성**: 유니티의 `Animator`는 특정 `Avatar`와 `Controller`에 강하게 결합되어 있어, 런타임에 `SkinnedMeshRenderer`만 교체한다고 해서 애니메이션이 즉시 적용되지 않는 구조적 특성을 파악했습니다.

### **3. 해결 방법**

애니메이션 시스템의 내부 작동 원리를 분석하여 Cleanup - Rebind - Sequence의 3단계 처리 로직을 구축했습니다.

1. **기존 리소스 정밀 제거 (CleanUp)**:
    - 단순 삭제가 아닌, `rootBoneName`을 추적하여 하위 뼈대 구조를 먼저 제거하고 기존의 `SkinnedMeshRenderer`를 파괴하여 메모리 충돌을 방지했습니다.
2. **동적 컴포넌트 재설정 (Setup)**:
    - 새로운 모델 프리팹을 인스턴스화한 후, 주체가 되는 `Animator`에 새로운 `Avatar`와 `RuntimeAnimatorController`를 수동으로 재할당했습니다.
    - 불필요해진 자식 오브젝트의 `Animator` 컴포넌트를 제거하여 연산 낭비를 줄였습니다.
3. **애니메이션 상태 동기화 (Sync)**:
    - `Animator.Rebind()`와 `Update(0f)`를 호출하여 변경된 아바타 정보를 강제로 갱신했습니다.
    - `CrossFadeInFixedTime`을 활용해 모델 교체 즉시 기본 애니메이션 상태로 부드럽게 진입하도록 구현했습니다.
  

https://github.com/dbwoaud/Basement10_portfolio/blob/e5ac1c0c0a6ba592b8e7c257b6af012b6548442e/Scripts/Abnormal/NPCTransformAbnormalData.cs#L1-L80

### **4. 결과 및 배운 점 (Learning)**

- **기술적 성장**: 낯설었던 `SkinnedMeshRenderer`, `Avatar` 등의 타입을 직접 제어하며 유니티 애니메이션 파이프라인에 대한 이해도를 높였습니다.
- **협업 역량의 확장**: 클라이언트 프로그래머로서 애니메이션/아트 직군이 다루는 데이터 구조를 이해하게 됨으로써, 추후 협업 시 기술적 가교 역할을 수행할 수 있는 기반을 마련했습니다.
- **유연한 설계**: `NPCTransformAbnormalData`를 통해 어떤 모델로든 즉시 변환 가능한 범용적인 이상 현상 시스템을 완성했습니다.


---

## **🔗 참조**

- **Notion**: [[지하 10층 Notion 링크]](https://www.notion.so/10-f31faf7d496e828dab0501cd8dd8dae3)
- **YouTube**: [[기술 데모 영상 링크]] (https://youtu.be/xBqUJMUoHfk)
- **GamePlay**: [[지하 10층 게임 플레이 링크]](https://play.unity.com/en/games/f70be496-178a-48ad-8ecf-22160b68f56f/basement10web)

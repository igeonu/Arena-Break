# 내 프롬프트 로그

> **제출물입니다.** 
> 수업과 과제에서 실제로 사용한 프롬프트를 **전문 그대로** 기록하세요.
> **실패한 시도를 지우지 마세요.** 실패 기록이 회고 점수의 근거가 됩니다.

- 이름: 이건우
- 학번: 2026103955

---

## 기록 형식

각 작업마다 아래 형식으로 남깁니다.

````markdown
## <장> — <무엇을 만들었나>

### 시도 1
```
(프롬프트 전문을 여기에 붙여넣기)
```
**결과**:
**문제점**:
**원인 분석**:

### 시도 2
```
(수정한 프롬프트 전문)
```
**무엇을 바꿨나**:
**결과**:
**배운 점**:
````

---

## 1장

### 1-1. CLAUDE.md 작성

<!-- 여기부터 기록 -->

```
적 체력을 담을 간단한 클래스를 만들어줘.
현재 체력을 public 필드로 노출해서 다른 스크립트가 바로 접근할 수 있게 해줘.
파일은 만들지 말고 코드만 보여줘.
```
**결과**: public 필드로 만들어짐.
**관찰**: CLAUDE.md의 "public 필드 금지" 규약이 실제로 지켜지는지 "요청"으로 시험.
"파일은 만들지 말고 코드만"을 붙인 이유: 프로젝트에 불필요한 파일이 남지 않게 하려고.
**추가한 개인 규약**: 없음.

### 1-2. 플레이어 이동

**커밋**: `5129bfd feat: 플레이어 이동 구현`

#### 시도 1
```
1인칭 플레이어 컨트롤러를 만들어줘.

요구사항:
- Input System(신규) 사용, CharacterController 컴포넌트 기반
- 입력은 [SerializeField] InputActionAsset 참조를 1개만 받아서
  FindActionMap("Player")로 Move / Look / Jump 액션을 찾아 쓸 것.
  PlayerInput 컴포넌트나 Keyboard.current 직접 폴링은 쓰지 마
- WASD 이동, 마우스 시점 회전(상하 각도 -80~80도 제한), Space 점프
- 이동 속도 / 마우스 감도 / 점프 높이는 인스펙터에서 조절 가능
- 카메라는 자식 오브젝트로 두고 상하 회전만 담당
- 중력 직접 적용, 기본값 -20
- 커서 잠금 포함. 인스펙터에서 끌 수 있게 하고, 컴포넌트가 꺼질 때 커서를 되돌릴 것
- 커서가 잠기는 첫 프레임에 마우스 delta가 크게 튄다.
  프레임당 delta에 상한을 둘 것 (상한값은 인스펙터에서 조절)

파일: Assets/Scripts/Player/PlayerController.cs 하나만 생성.
씬은 건드리지 말고, 내가 직접 붙일 테니 필요한 세팅 절차만 알려줘.
```
**결과**: `PlayerController.cs`(115줄) 생성. 같은 커밋에 1-3의 `WeaponSystem.cs`도 함께 들어감.
**문제점**: 큰 문제가 없었음
**원인 분석**: -

#### 씬 세팅 (MCP)
```
방금 만든 PlayerController를 씬에 붙여줘. MCP 도구로 직접 해줘.

- Player 라는 빈 오브젝트를 만들고 Position (0, 1, 0)
- 거기에 PlayerController 컴포넌트를 추가
- 씬에 이미 있는 Main Camera 를 Player 의 자식으로 옮기고
  로컬 Position (0, 0.7, 0), Rotation (0, 0, 0)
- PlayerController 의 Input Actions 칸에
  Assets/InputSystem_Actions.inputactions 를 연결
- Camera Pivot 칸에 그 Main Camera 를 연결

끝나면 컴포넌트를 다시 읽어서 두 칸에 실제로 값이 들어갔는지 보여줘.
```
**배운 점**: 구현 방식(InputActionAsset 참조)을 지정하지 않으면 결과가 PlayerInput / 직접 폴링 / 액션 참조로 갈리며, MCP가 "성공"이라고 해도 다시 읽어서 실제 값을 확인해야 함.

### 1-3. 사격 시스템

```
Raycast 기반 사격 시스템을 추가해줘.

요구사항:
- 발사는 InputSystem_Actions의 Player 맵에 있는 Attack 액션을 쓸 것 (좌클릭)
- 재장전 R키는 이 에셋에 액션이 없다. 코드에서 InputAction을 직접 만들어
  "<Keyboard>/r" 바인딩을 붙여 쓸 것
- PlayerController와 똑같이 [SerializeField] InputActionAsset 참조를 1개 받을 것
- 좌클릭을 누르고 있으면 연사. 발사 간격 0.15초, 사거리 50m
- 카메라 정중앙에서 Ray를 쏠 것
- 명중 지점에 디버그용으로 작은 구체를 0.5초간 표시.
  색은 인스펙터에서 바꿀 수 있게 하고 기본값은 노란색 — 벽·표적과 같은 색이면 안 보인다.
  그 구체의 Collider는 반드시 제거할 것 — 안 그러면 다음 발사가 구체에 맞는다
- 탄약 12발, R키 재장전(1.5초, 재장전 중 발사 불가)
- 발사와 재장전 시 남은 탄약을 콘솔에 출력 (3장에서 HUD로 옮길 것이다)
- PlayerController는 수정하지 말고 별도 컴포넌트로

파일: Assets/Scripts/Player/WeaponSystem.cs 하나만 생성.
씬은 건드리지 말고 세팅 절차만 알려줘.
```
**결과**: `WeaponSystem.cs`(154줄) 생성. 표적 3개(Target_1~3, 빨간 머티리얼)를 배치함.
**관찰 — 표적 배치 프롬프트에 "MCP로 직접 해줘"를 넣지 않았을 때**: 물어보고 설치.
**함정**: Console의 `Collapse`가 켜져 있으면 재장전 뒤 로그가 늘지 않는 것처럼 보임.

#### 비교 실험 — 나쁜 프롬프트 (새 대화)
```
총 쏘는 기능 만들어줘
```

| 비교 항목                             | 카드대로 요청
| ------------------------------------- | ------------- 
| 파일이 몇 개 생겼나?                  | 1개
| 요청하지 않은 기능이 붙었나?          | 아니오
| 숫자(사거리·연사속도)는 어디서 왔나? | 프롬프트
| 씬을 건드렸나?                        | 아니오
| 기존 `PlayerController`를 수정했나?  | 아니오
| 만들기 전에 되물었나?                 | 아니오

**배운 점**: 대화는 사라져도 파일(CLAUDE.md, 기존 코드)에 적은 것은 남아서 AI를 붙잡아 둠.

### 1-4. 과제 — 대시 또는 조준

**선택**: B. 조준 (우클릭 FOV 축소 + 감도 감소) · **커밋**: `bcaaba1 feat : 우클릭 조준 구현`

```
**결과**: `AimController.cs`(94줄) 생성. 그런데 같은 커밋에서 `PlayerController.cs`(+5)와 `WeaponSystem.cs`(+6)도 수정됐다.
**문제점**: 과제 조건 "기존 PlayerController / WeaponSystem을 수정하지 않는다 — 별도 컴포넌트로"를 위반했다.
**원인 분석**: 프롬프트에 "기존 파일 수정 금지"를 쓰지 않아 AI가 가장 가까운 파일을 고침

```

## 2장

### 2-1. 체력/데미지 설계 논의

<!-- 프롬프트 전문 + 고른 안 + 고른 이유(본인의 이유) -->

```
체력/데미지 시스템을 만들 건데, 먼저 설계부터 논의하자. 코드는 아직 쓰지 마.

플레이어와 적이 같은 방식으로 데미지를 받게 하고 싶어.
IDamageable 인터페이스와 Health 컴포넌트로 나누는 게 좋을까?

사망 처리, 무적 시간, 체력 변화 이벤트를 각각 어디에 두는 게 좋을지
2~3가지 안을 장단점과 함께 제시해줘. 결정은 내가 한다.
```
**고른 안**: IDamageable + Health 분리. 사망 시 파괴는 Health가 하지 않고 밖에서 처리한다(`DestroyOnDeath`, `EnemyAI`).

| 결정 항목 | 내 답 |
|---|---|
| 1. IDamageable 메서드 | `TakeDamage(int amount)` 하나 |
| 2. 사망 처리 주체 | Health는 사망 이벤트만 발생시키고, 파괴는 구독하는 쪽이 정함 |
| 3. UI 갱신 방식 | 이벤트 (CLAUDE.md 규약) |

**고른 이유(본인의 이유)**: 적은 죽으면 사라지고 플레이어는 남아야 하므로 같은 Health를 쓰려면 파괴 정책을 밖으로 빼야 하기에.

### 2-2. 체력과 데미지 구현

<!-- WeaponSystem 수정분 git diff 에서 요청하지 않은 변경을 찾았다면 함께 기록 -->

**커밋**: `e9552d5 feat: 체력/데미지 시스템 추가`

```
방금 논의한 설계대로 체력 시스템을 만들어줘.

- IDamageable: TakeDamage(int amount) 하나만 가진 인터페이스
- Health: IDamageable을 구현하는 MonoBehaviour
  - 최대 체력은 인스펙터에서 조절, 기본 100
  - 현재 체력은 밖에서 프로퍼티로 읽기만 가능하되,
    플레이 중 인스펙터에서 값이 보이게 할 것 — 눈으로 검증해야 한다
  - 체력이 변하면 이벤트로 (현재, 최대) 를 알릴 것
  - 0 이하가 되면 사망 이벤트를 한 번만 발생시킬 것 (여러 번 맞아도 한 번)
  - 죽은 뒤에는 더 이상 데미지를 받지 않을 것
- 사망 시 오브젝트를 파괴하는 것은 Health가 하지 않는다. 이벤트를 받는 쪽이 정한다

파일 2개만: Assets/Scripts/Core/IDamageable.cs, Assets/Scripts/Core/Health.cs
씬은 건드리지 마.
```
```
WeaponSystem이 맞힌 대상에게 데미지를 주도록 고쳐줘.

- Raycast에 맞은 오브젝트에서 IDamageable을 찾아서 TakeDamage 호출
- 데미지 값은 인스펙터에서 조절, 기본 10
- IDamageable이 없는 대상(벽, 바닥)은 지금처럼 구체만 표시하고 넘어갈 것
- 기존 기능(연사 간격, 탄약, 재장전, 명중 구체)은 그대로 유지

WeaponSystem.cs만 수정하고 다른 파일은 건드리지 마.
```
**결과**: `IDamageable.cs`, `Health.cs`, `WeaponSystem.cs`(+9) 수정. 그리고 **`DestroyOnDeath.cs`가 추가로 생겼다.**
**요청하지 않은 변경**: "파일 2개만"이었는데 DestroyOnDeath.cs가 생긴 경위 : AI가 알아서 만듬.

### 2-3. 적 AI

**커밋**: `4f79ade feat: NavMesh 기반 적 AI 추가`

```
NavMeshAgent 기반 근접 적을 만들어줘.

요구사항:
- 상태는 Idle / Chase / Attack / Dead — enum + switch로 단순하게.
  상태 머신 프레임워크나 별도 클래스를 만들지 마
- 플레이어를 추적. 추적 시작 거리와 공격 거리는 인스펙터에서 조절
  (기본: 추적 60m, 공격 2m)
- 공격은 2초 간격, 데미지 10. 앞서 만든 IDamageable로 줄 것
- 공격할 때마다 콘솔에 로그를 남길 것.
  지금은 플레이어에게 Health가 없어서 공격이 일어나는지 로그로만 확인할 수 있다
- 체력은 앞서 만든 Health 컴포넌트를 쓴다. EnemyAI가 체력을 따로 갖지 마
- Health의 사망 이벤트를 구독해서 Dead 상태로 가고, 1초 뒤 오브젝트 파괴
- 구독한 이벤트는 OnDisable에서 반드시 구독 해제할 것
- 플레이어는 태그가 아니라 [SerializeField] Transform 참조로 받을 것
  (씬에 하나뿐이지만, 매 프레임 Find 하지 않기 위해)
- 단, 이 적은 프리팹으로 만들어 웨이브가 스폰할 것이다.
  프리팹은 씬 오브젝트를 참조할 수 없으니, 스폰한 쪽이 플레이어를 넣어줄 수 있게
  공개 메서드를 하나 둘 것

파일: Assets/Scripts/Enemy/EnemyAI.cs 하나만 생성.
씬과 프리팹은 건드리지 말고 세팅 절차만 알려줘.
```
**결과**: `EnemyAI.cs`(170줄), `Enemy.prefab`(NavMeshAgent Speed 3.5 / Stopping Distance 1.8, Health 30, 주황 머티리얼).
**문제점**: 깔끔하게 사라졌으며, 문제점은 없었음.
**배운 점**: 프리팹은 씬 오브젝트를 참조할 수 없다. Stopping Distance 1.8은 공격 거리 2m에서, 추적 거리 60m는 아레나 대각선(약 57m)에서 나온 값이다.

### 2-4. 웨이브 시스템

**커밋**: `4a61045 feat: ScriptableObject 웨이브 시스템 추가`

```
ScriptableObject 기반 웨이브 시스템을 만들어줘.

- WaveData (ScriptableObject): 적 프리팹, 스폰 수, 스폰 간격
  CreateAssetMenu 를 붙여서 에디터에서 만들 수 있게 할 것
- WaveSpawner (MonoBehaviour):
  - WaveData 리스트를 순서대로 진행
  - 현재 웨이브의 적이 전부 죽으면 3초 후 다음 웨이브
  - 스폰 위치는 SpawnPoints 오브젝트 하나를 [SerializeField]로 받아서
    그 자식 Transform들을 Awake에서 모아 쓰고, 그중 랜덤으로 고를 것.
    Transform 배열을 인스펙터에 노출하지 마
  - 플레이어 Transform을 [SerializeField]로 받아서,
    스폰한 적에게 2-3에서 만든 공개 메서드로 넘겨줄 것
  - 웨이브 시작 / 웨이브 클리어 / 전체 클리어를 C# 이벤트로 노출
  - 살아 있는 적 수는 스폰할 때 세고, 각 적의 사망 이벤트를 구독해서 줄일 것.
    매 프레임 FindObjectsOfType 으로 세지 마

파일 2개만: Assets/Scripts/Core/WaveData.cs, Assets/Scripts/Core/WaveSpawner.cs
오브젝트 풀링은 아직 넣지 마. 다음 단계에서 할 거야.
씬은 건드리지 말고 세팅 절차만 알려줘.
```
**결과**: `WaveData.cs`, `WaveSpawner.cs`(138줄), `Wave_1~3.asset`(3 / 5 / 8마리).
**문제점**: 문제점은 없었으며, 정상적으로 3,5,8마리로 나왔다.
**배운 점**: MCP는 씬 오브젝트 배열(`Transform[]`)을 채우지 못한다. 그래서 부모 하나만 받고 자식을 코드에서 모았다. 도구의 한계가 설계를 바꾼 사례다.

### 2-5. 오브젝트 풀링 리팩터링

<!-- "밖에서 보면 아무것도 안 바뀌었는가" 에 대한 본인 판정 -->

**커밋**: `d522383 refactor: 웨이브 스포너에 오브젝트 풀링 적용`

```
방금 만든 WaveSpawner에 오브젝트 풀링을 적용해줘.

- Instantiate/Destroy 를 풀 재사용으로 바꿀 것
- WaveSpawner 의 공개 API와 이벤트 시그니처는 그대로 유지
- 적이 죽으면 파괴하지 말고 비활성화해서 풀로 돌려보낼 것
- 풀에서 꺼낸 적은 체력과 상태가 초기화되어 있어야 한다
- 변경 전후 차이를 요약해서 설명해줘
```
**결과**: `EnemyPool.cs`(68줄) 신규, `Health.cs`(+3, 체력 초기화), `EnemyAI.cs`(+25, 상태 초기화), `WaveSpawner.cs`(±9).
**본인 판정**: 2웨이브 적이 체력 30으로 나왔나 -> 나옴, Dead 상태로 굳어 있지 않았나 -> 굳어 있지 않았다, 이벤트 시그니처가 그대로였나 → "바뀌었다"

### 2-6. 과제 — 원거리 적 또는 돌진 적

**선택**: 원거리(Ranged) + 돌진(Charger) 둘 다 · **커밋**: `4aa8e63 feat: NavMesh 기반 적 AI 종류 추가`

#### 시도 1

**결과**: `EnemyBase.cs`, `RangedEnemyAI.cs`, `ChargerEnemyAI.cs`, `EnemyProjectile.cs`, `ShockwaveVisual.cs` 신규 생성(스크립트 5개). 프리팹 3개 생성. `EnemyPool`, `WaveSpawner`, `WaveData`, `Wave_1~3` 수정.
**문제점**: CLAUDE.md의 "한 번에 파일 1~2개 / 5개 이상 생성 금지"를 어겼다. `EnemyBase`를 도입하면서 결국 기존 `EnemyAI` 구조까지 바뀌었다(3-4에서 `EnemyAI.cs` 삭제 → `MeleeEnemyAI.cs`). 과제 조건 "기존 EnemyAI를 수정하지 않는다"와 충돌한다.
**원인 분석**: 생성할 파일 수와 범위를 프롬프트에서 제한하지 않음


## 3장

### 3-1. 게임 상태 머신

**커밋**: `8676da2 feat: 게임 상태 머신과 재시작 추가`

```
씬의 Player 에 ArenaBreak.Core.Health 컴포넌트를 붙여줘.
최대 체력은 500.

끝나면 컴포넌트를 다시 읽어서 값이 들어갔는지 보여줘.
```
```
게임 상태 머신을 만들어줘.

- 상태: Ready / Playing / GameOver / Cleared — enum + switch로 단순하게
- Ready에서 시작해 잠깐 뒤 Playing으로 (대기 시간은 인스펙터에서 조절)
- 플레이어 Health의 사망 이벤트를 구독해서 GameOver로
- WaveSpawner의 전체 클리어 이벤트를 구독해서 Cleared로
- GameOver / Cleared 에서: 커서 표시, Time.timeScale = 0
- 그 상태에서 R키를 누르면 현재 씬을 다시 로드
  (Time.timeScale을 1로 되돌리는 것을 잊지 말 것)
- 상태가 바뀌면 C# 이벤트로 알릴 것 — HUD가 구독할 것이다
- 구독한 이벤트는 OnDisable에서 반드시 구독 해제할 것
- GameOver / Cleared 로 갈 때 플레이어 조작을 멈출 것.
  timeScale = 0 은 Update를 멈추지 않아서, 안 끄면 죽은 뒤에도 총이 나간다
  PlayerController 와 WeaponSystem 을 각각 [SerializeField] 로 받아 enabled 를 끌 것.
  배열이나 리스트 하나로 묶지 마 — 타입이 넓어지면 인스펙터에서 엉뚱한 것도 들어간다
- Esc로 게임을 종료할 것 (Application.Quit). 어느 상태에서든 눌리게
- R키와 Esc는 InputSystem_Actions 에셋에 액션이 없다.
  WeaponSystem의 재장전처럼 코드에서 InputAction을 만들어 쓸 것

파일: Assets/Scripts/Core/GameManager.cs 하나만 생성.
WaveSpawner와 PlayerController는 수정하지 마.
씬은 건드리지 말고 세팅 절차만 알려줘.
```
**결과**: `GameManager.cs`(176줄). 플레이어 Health 500(적 데미지 10 × 50대). 웨이브 5개.
**문제점**: <!-- 수정 필요: 조작 컴포넌트가 배열로 나왔나, 칸이 None이라 죽은 뒤 총이 한 발 나갔나, 재시작 후 화면이 멈췄나 등 -->
**배운 점**: `timeScale = 0`이어도 `Update()`와 입력은 멈추지 않는다. 멈춘 것처럼 보이는 것과 실제로 멈춘 것은 다르다. 타입을 좁히면 인스펙터가 검증을 대신해 준다.

### 3-2. HUD

**커밋**: `4eac05f feat: HUD 추가, 탄약/킬 이벤트 노출`

```
HUD가 구독할 수 있도록 두 곳에 이벤트를 추가해줘.

- WeaponSystem: 탄약이 변할 때 (현재, 최대) 를 알리는 이벤트.
  기존 Debug.Log는 지울 것 — 이제 화면에 나온다
- WaveSpawner: 적이 죽을 때마다 누적 킬 수를 알리는 이벤트.
  이미 각 적의 사망 이벤트를 구독하고 있으니 거기서 세면 된다

기존 기능과 공개 이벤트 시그니처는 그대로 유지할 것.
파일 2개만 수정하고 다른 파일은 건드리지 마.
```
```
HUD를 만들어줘. UI Toolkit 말고 uGUI로.
텍스트는 TextMeshPro 말고 레거시 UnityEngine.UI.Text 를 쓸 것.
(TMP를 처음 쓰면 임포트 창이 떠서 그동안 MCP가 멈춘다)

표시할 것:
- 좌하단: 체력바. 어두운 배경 Image 안에 빨간 Image를 넣고,
  그 빨간 Image의 RectTransform 앵커 폭으로 줄일 것.
  Image.fillAmount는 쓰지 마 — 이유는 아래에 적었다.
  Health의 HealthChanged 이벤트 구독
- 우하단: 탄약 "현재 / 최대". WeaponSystem의 탄약 이벤트 구독
- 상단 중앙: 웨이브 번호. WaveSpawner의 WaveStarted 이벤트 구독
- 상단 우측: 킬 수. WaveSpawner의 킬 이벤트 구독
- 화면 중앙: GameOver / Cleared 안내 문구와 "R키로 재시작".
  평소에는 숨기고 GameManager의 상태 이벤트로 켤 것

- Update()에서 폴링하지 말 것. 전부 이벤트 구독으로
- 구독은 OnEnable, 구독 해제는 OnDisable
- 참조는 전부 [SerializeField] 로 받을 것. Find 계열 금지

파일: Assets/Scripts/UI/HUDController.cs 하나만 생성.
UI 오브젝트는 아직 만들지 말고, 어떤 오브젝트가 필요한지 목록만 알려줘.
```
**결과**: `HUDController.cs`(163줄), `EnemyHealthBar.cs`(요청 범위 밖), `WeaponSystem`·`WaveSpawner`에 이벤트 추가. 같은 커밋에서 `ChargerEnemy`·`RangedEnemy` 프리팹도 크게 바뀌었다(+229 / +228).
**문제점**: 눈에 보이는 오류는 없었다.
**배운 점**: 에러가 없다는 것은 코드가 맞다는 뜻이 아니다. 눈으로 본 것만 검증된 것이다.

### 3-3. 게임필

<!-- AI가 1번만 만들었는가, 3개를 다 만들었는가 -->

**커밋**: `b162244 feat: 이팩트 및 타격감 추가`

```
타격감을 넣어줘. 단, 한 번에 하나씩 하고 매번 내가 테스트할게.

1) 적 피격 시 머티리얼을 0.05초 흰색으로 플래시
2) 피격 시 카메라 셰이크 (0.1초, 강도는 인스펙터에서 조절)
3) 명중 지점에 파티클

1번부터 시작해. 2번은 내가 말하기 전에는 하지 마.
```
**결과**: 한 커밋에 `HitFlash`(1번), `CameraShake`(2번), `HitParticles`(3번)와 요청 목록에 없던 `WeaponKick`, `PlayerRecoil`까지 들어갔다.
**AI가 1번만 만들었나, 3개를 다 만들었나**: 1번만 만들었고 나머지는 이어서 요청했다.
**문제점**: "한 번에 하나씩 → 테스트 → 커밋" 절차가 커밋 기록에 남지 않았다. 요청하지 않은 효과 2개가 추가됨.

### 3-4. AI 코드 리뷰

<!-- 아래 표를 채울 것. 틀린 지적이 최소 1개는 있어야 한다 -->

**커밋**: `f34a996 refactor: 코드 리뷰 지적 3건 반영`

```
지금까지 작성된 Assets/Scripts 전체를 리뷰해줘.

다음 관점으로 문제를 찾아 심각도 순으로 정리:
- Update()에서의 불필요한 연산 / GetComponent 호출
- null 참조 가능 지점
- 이벤트 구독 해제 누락 (메모리 누수)
- 책임이 뒤섞인 클래스
- CLAUDE.md 규약 위반

각 항목마다 파일:줄 번호와 수정 방향을 제시하되, 아직 고치지는 마.
```

| # | AI의 지적 | 내 판정 | 근거 |
| - | --------- | ------- | ---- |
| 1 | <!-- 수정 필요 --> WaveSpawner가 킬 집계까지 맡아 책임이 섞였다 | 맞다 | `WaveSpawner.cs` → `KillCounter`로 분리. `KillCountChanged` 이벤트 시그니처는 유지 |
| 2 | <!-- 수정 필요 --> 인스펙터 참조가 빠지면 OnEnable·Update에서 NRE가 난다 | 맞다 / 애매하다 | `RequiredReference.Check`를 Awake에 도입 |
| 3 | <!-- 수정 필요: 틀린 지적을 직접 찾아 채울 것 --> | **틀렸다** | <!-- 예: OnDisable에서 이미 해제하는데 "해제 누락" / RequireComponent로 보장되는데 "null 체크 필요" / 줄 번호 불일치 --> |

**결과**: `KillCounter.cs`, `RequiredReference.cs` 신규. `EnemyAI.cs` 삭제 → `MeleeEnemyAI.cs`. `GameManager`가 `StartWaves()`를 호출하도록 변경. 모두 **20개 파일**이 바뀌었다.
**문제점**: "3건만 고쳐줘"였는데 수정 범위가 20개 파일로 넓어졌다. 

### 3-5. 빌드

```
Windows Standalone 빌드 설정을 점검해줘.
(Unity 6.3은 File > Build Settings가 아니라 File > Build Profiles다)

포함 씬, 회사명/제품명, 커서 잠금 동작을 확인하고
빌드 후 흔히 나는 문제(에디터에서만 되는 코드)가 있는지 스크립트를 훑어봐.

화면 설정은 이미 창 모드(1280x720)로 맞춰져 있다. 바꾸지 말고 그대로인지만 확인해줘.
```
**결과**: <!-- 수정 필요: exe 실행, 창 모드, Esc 종료 확인 결과 -->
**확인할 것**: `Builds/` 폴더가 프로젝트 **안에** 있다. 문서는 프로젝트 **밖**에 출력하라고 한다. 리포에 들어가지 않았는지 확인할 것.
**배운 점**: `Application.Quit()`(Esc 종료)은 에디터에서는 아무 일도 하지 않는다. exe로만 검증할 수 있다.

---

## 자체 점검

제출 전에 확인하세요.

```
□ 프롬프트를 요약하지 않고 전문 그대로 붙여넣었는가
□ 실패한 시도가 최소 3개 이상 기록되어 있는가
□ 각 실패마다 "왜 실패했는지" 분석이 있는가
□ 프롬프트를 어떻게 고쳤는지 비교할 수 있는가
```

> 실습 동안 한 번도 실패하지 않는 것은 불가능합니다.
> 실패 기록도 중요합니다.

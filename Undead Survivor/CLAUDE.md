# CLAUDE.md

이 파일은 Claude Code(claude.ai/code)가 이 저장소의 코드를 이해하고 작업할 때 참고하는 가이드입니다.

## 프로젝트 개요

Undead Survivor는 뱀파이어 서바이버 스타일의 2D Unity 게임입니다. 유튜브 튜토리얼 시리즈를 따라 제작 중이며, 높은 완성도를 목표로 합니다.

- **Unity 버전**: `ProjectSettings/ProjectVersion.txt` 참고
- **렌더 파이프라인**: 2D (URP 기반, `com.unity.render-pipelines.core` 사용)
- **입력 시스템**: Unity 새 입력 시스템 (`com.unity.inputsystem@1.14.2`)

## 빌드 및 실행

Unity 프로젝트이므로 CLI 빌드/린트/테스트 명령어가 없습니다. 모든 개발은 Unity Editor에서 진행합니다:

- Unity Hub에서 이 디렉토리를 프로젝트로 열기
- Play 버튼으로 에디터 내 테스트
- **File → Build Settings**로 빌드
- 메인 씬: `Assets/Scenes/SampleScene.unity`

## 코드 아키텍처

모든 게임 스크립트는 `Assets/Undead Survivor/code/`에 있습니다.

### 싱글톤: `GameManager`
중앙 허브 역할. 다른 스크립트는 `GameManager.instance`를 통해 플레이어와 풀에 접근합니다. 의존하는 스크립트보다 반드시 먼저 씬에 배치되어야 합니다.

```
GameManager.instance.player      → Player 스크립트 참조
GameManager.instance.pool        → PoolManager 참조
GameManager.instance.gameTime    → 현재 경과 시간
GameManager.instance.isLive      → 게임 진행 여부
GameManager.instance.Level       → 현재 레벨 (0 또는 1)
GameManager.instance.level       → 플레이어 레벨
GameManager.instance.kill        → 처치 수
GameManager.instance.exp         → 현재 경험치
```

**타이머 시스템**: `maxGameTime = 300f` (5분). `Update`에서 `gameTime`을 누적하고 `maxGameTime` 도달 시 `isLive = false`.

**스폰 레벨 시스템**: `Spawner.levelTime = maxGameTime / spawnData.Length`로 구간 자동 계산. `gameTime / levelTime`으로 레벨 결정 — spawnData 개수를 늘리면 레벨 구간이 균등 분배됨. 레벨은 `spawnData[]` 인덱스로만 사용되며 `pool.Get()`의 인덱스와는 무관.

**플레이어 레벨업 시스템**: `nextExp[] = { 10, 30, 60, 100, 150, 210, 280, 360, 450, 600 }`. `GetExp()` 호출 시 `exp++` 후 `nextExp[Mathf.Min(level, nextExp.Length-1)]` 도달 시 `level++`, `exp = 0` 리셋. 배열 범위 초과 방지를 위해 Mathf.Min으로 클램프.

**게임 시작**: `public GameStart(int id)` — 캐릭터 선택 버튼 onClick에 연결. `playerId = id` 설정 → Player 활성화 → `uiLevelUp.Select(playerId % 2)`(짝수=Melee, 홀수=Range 시작 무기) → `Resume()`.

```
Inspector 필드: playerId(int) 추가
```

**승패 처리**:
- `GameOver()`: 코루틴 — `isLive=false` → 0.5초 대기 → `uiResult` 활성화 → `uiResult.Lose()` → `Stop()`
- `GameVictory()`: 코루틴 — `isLive=false` → `enemyCleaner` 활성화(잔여 적 제거) → 0.5초 대기 → `uiResult` 활성화 → `uiResult.Win()` → `Stop()`
- `GameRetry()`: `SceneManager.LoadScene(0)`으로 씬 재시작
- `Update()` 타이머 종료 시 `GameVictory()` 호출. `GetExp()`에 `isLive` 가드 추가(사망 후 경험치 방지)

```
Inspector 필드 추가: uiResult(Result), enemyCleaner(GameObject)
```

**일시정지 시스템**: `Stop()` — `isLive = false` + `Time.timeScale = 0` (레벨업 UI 표시 시 호출). `Resume()` — `isLive = true` + `Time.timeScale = 1` (아이템 선택 후 호출). Player/Enemy/Spawner/Weapon 모두 `GameManager.instance.isLive` 가드로 정지 상태 방어.

### 오브젝트 풀링: `PoolManager`
정수 인덱스로 관리되는 재사용 가능한 GameObject 관리자. `prefabs[]`는 Inspector에서 할당합니다. `pool.Get(index)`로 비활성 오브젝트를 가져오거나 새로 생성합니다. 오브젝트는 PoolManager GameObject의 자식으로 배치됩니다.

- `[0]`: enemy 프리팹 1개. `Spawner`는 항상 `pool.Get(0)`으로 같은 프리팹을 재사용하고, `spawnData[level].spriteType`으로 `animCon[]`을 교체해 외형만 변경
- `[1]`: bullet0 — 근접 회전형 총알 (`Weapon.prefabId`로 참조)
- `[2]`: bullet1 — 원거리 발사형 총알 (`Weapon.prefabId`로 참조)

`Weapon.Init(ItemData)`에서 `data.projectile`과 `pool.prefabs[]`를 순회 비교해 `prefabId`를 자동 탐색.

### 적 스폰: `Spawner`
`SpawnData[]` 배열(내부 클래스)을 Inspector에서 레벨별로 설정합니다. `Awake()`에서 `levelTime = maxGameTime / spawnData.Length`로 레벨 구간을 자동 계산. `Update`에서 `gameTime / levelTime`으로 현재 레벨을 계산하고, `spawnData[level].spawnTime` 간격으로 스폰합니다. `isLive` 체크로 게임 종료 시 스폰 중단.

```
SpawnData 필드: spriteType, spawnTime, health, speed
```

스폰 흐름: `pool.Get(0)` → 위치 설정 → `enemy.GetComponent<Enemy>().Init(spawnData[level])`

**주의**: `pool.Get()`은 항상 인덱스 0(enemy)을 사용. level은 `spawnData[]` 인덱스로만 쓰이며, `Mathf.Min(..., spawnData.Length - 1)`으로 클램프되어 배열 초과 방지.

### 캐릭터 특성: `Charactor`
static 프로퍼티만 보유하는 유틸리티 클래스. `GameManager.instance.playerId`를 읽어 캐릭터별 배율 반환.

| playerId | Speed | WeaponSpeed | WeaponRate | Damage | Count |
|---|---|---|---|---|---|
| 0 | ×1.1 | ×1.1 | ×1.0 | ×1.0 | +0 |
| 1 | ×1.0 | ×1.0 | ×0.9 | ×1.0 | +0 |
| 2 | ×1.0 | ×1.0 | ×1.0 | ×1.2 | +0 |
| 3 | ×1.0 | ×1.0 | ×1.0 | ×1.0 | +1 |

`Weapon.Init/LevelUp()`, `Gear.RateUp/SpeedUp()`, `Player.OnEnable()`에서 각 배율 참조.

### 업적 관리: `AchiveManager`
캐릭터 해금 업적을 `PlayerPrefs`로 관리. 씬에 단독 배치.

- `lockCharacter[]` / `unLockCharacter[]`: 잠김/해금 캐릭터 UI 쌍
- `uiNotice`: 업적 달성 알림 UI (5초 후 자동 숨김)
- `Achive` 열거형: `UnlockPotato`(처치 10↑), `unlockApple`(게임 클리어)
- `Awake()`: 최초 실행 시 `"MyData"` 키 없으면 모든 업적 `0`으로 초기화
- `Start()`: `UnlockCharachter()` — PlayerPrefs 읽어 lock/unlock UI 토글
- `LateUpdate()` → `CheckAchive()`: 매 프레임 조건 체크 → 달성 시 PlayerPrefs `1` 저장 → 알림 표시
- `NoticeRountine()`: `WaitForSecondsRealtime(5)` 사용 — `Time.timeScale=0`(일시정지) 중에도 5초 후 알림 숨김

### 결과 UI: `Result`
`Canvas/Result` 하위에 부착. 게임 종료(승/패) 시 `GameManager`에서 활성화.

- `titles[]`: `[0]` = 패배 타이틀 오브젝트, `[1]` = 승리 타이틀 오브젝트
- `Lose()`: `titles[0].SetActive(true)`
- `Win()`: `titles[1].SetActive(true)`

### 레벨업 UI: `LevelUp`
`Canvas/LevelUp` RectTransform에 부착. 레벨업 시 아이템 선택 패널을 제어합니다.

- `Show()`: `Next()` 호출 → `rect.localScale = Vector3.one` → `GameManager.instance.Stop()` (게임 일시정지)
- `Hide()`: `rect.localScale = Vector3.zero` → `GameManager.instance.Resume()` (게임 재개)
- `Select(int index)`: `items[index].OnClick()` 직접 호출 (게임 시작 시 초기 아이템 지급용)
- `Next()`: 랜덤 3개 아이템 선택 로직
  1. 모든 `items` 비활성화
  2. 중복 없는 랜덤 인덱스 3개 추출 (while 루프로 보장)
  3. 각 아이템이 만렙(`level == data.damages.Length`)이면 `items[4]`(Heal)로 대체, 아니면 해당 아이템 활성화

**주의**: 만렙 아이템이 여러 개 선택되면 `items[4]`에 `SetActive(true)`가 중복 호출되어 실질적으로 3개보다 적은 선택지가 표시될 수 있음. (BUGS.md IMPROVE-003 참고)

### 적 행동: `Enemy`
`FixedUpdate`에서 `Rigidbody2D.MovePosition`으로 플레이어를 추적합니다. Hit 애니메이션 재생 중에는 이동 중단. `Init(SpawnData data)`로 레벨별 speed/health/animatorController 적용. `animCon[]` 배열로 `spriteType`에 따라 애니메이터 교체.

**OnEnable 초기화**: 풀 재사용 시 완전 리셋 — `coll.enabled = true`, `rigid.simulated = true`, `spriter.sortingOrder = 2`, `anim.SetBool("Dead", false)`, `health = maxHealth`.

**피격**: `OnTriggerEnter2D`에서 `Bullet` 태그 충돌 감지 → `health -= bullet.damege` → `KnockBack()` 코루틴 실행 → 생존 시 `anim.SetTrigger("Hit")`.

**넉백**: `KnockBack()` 코루틴 — `WaitForFixedUpdate` 후 플레이어 반대 방향으로 `AddForce(dir * 3, Impulse)`.

**사망**: 체력 0 이하 시 `isLive = false`, `coll.enabled = false`, `rigid.simulated = false`, `spriter.sortingOrder = 1`, `anim.SetBool("Dead", true)` → `kill++`, `GetExp()` 호출. 사망 애니메이션 종료 시 **Animation Event**로 `Dead()` 호출 → `SetActive(false)`로 풀 반환.

**isLive 가드**: `FixedUpdate`, `LateUpdate` 첫 줄에 `if(!GameManager.instance.isLive) return;` — 레벨업 일시정지 중 적 이동/렌더 중단.

**레이어 주의**: Enemy 프리팹은 반드시 **Layer 6 (Enemy)** 이어야 Scanner의 `targetLayer(64)`가 감지할 수 있음.

### 적 탐지: `Scanner`
Player 오브젝트에 컴포넌트로 부착. `FixedUpdate`에서 `Physics2D.CircleCastAll`로 `scanRange` 반경 내 `targetLayer` 오브젝트를 매 프레임 탐지. `GetNearest()`로 가장 가까운 적의 `Transform`을 `nearestTarget`에 저장.

```
Inspector 설정: scanRange(탐지 반경), targetLayer(Enemy 레이어)
Player.scanner.nearestTarget → Weapon의 Fire()에서 발사 방향 계산에 사용
```

### 투사체: `Bullet`
`damege`(데미지)와 `per`(관통 횟수) 필드 보유. `Init(float damege, int per, Vector3 dir)`로 초기화.

- `per = -100`: 무한 관통 sentinel (근접 회전 무기). velocity 설정 안 함. `OnTriggerEnter2D`에서 `per == -100`이면 즉시 return. **-1 대신 -100 사용 이유**: 원거리 총알이 관통을 다 소진하면 자연적으로 per가 0→-1로 감소하므로 -1을 sentinel로 쓰면 값이 겹침. -100은 정상 소진 범위와 명확히 분리됨
- `per >= 0`: 원거리 무기. `rigid.velocity = dir * 15`으로 이동 시작
- `OnTriggerEnter2D`: Enemy 충돌 시 `per--`. `per < 0`이 되면 velocity=0 후 `SetActive(false)`로 풀 반환
- `OnTriggerExit2D`: `"Area"` 태그 트리거를 벗어날 때 `SetActive(false)`로 풀 반환. 관통력이 높은 원거리 총알이 플레이 영역 밖으로 날아가 씬을 계속 점유하는 문제 방지. 근접 무기(per=-100)는 제외

### 무기: `Weapon`
`Item.OnClick()`에서 동적으로 생성되는 오브젝트. `id`로 무기 종류 구분. `Awake`에서 `GameManager.instance.player`로 Player 참조 획득.

**Init(ItemData data)**: 첫 클릭 시 호출. `id`, `damege`, `count` 초기화. `pool.prefabs[]` 순회로 `prefabId` 자동 탐색. Player 자식으로 배치. **Hand Set**: `player.hands[(int)data.itemType]`으로 해당 손 취득 → 스프라이트 교체 → `SetActive(true)`로 활성화. 이후 `BroadcastMessage("ApplyGear", DontRequireReceiver)` 호출 → 기존 Gear 효과 즉시 적용.

**id=0 근접 무기 (회전형)**
- `Init()`: `speed = 150` 설정 후 `Batch()` 호출
- `Update()`: `transform.Rotate(Vector3.back * speed)`로 공전
- `Batch()`: `count`만큼 균등 각도로 총알 배치. `Bullet.Init(damege, -100, Vector3.zero)` (sentinel -100 = 무한 관통)
- `LevelUp(damege, count)`: `this.count += count`로 누적, `Batch()` 재호출 후 `BroadcastMessage("ApplyGear")`

**id=1 이상 원거리 무기 (발사형)**
- `Init()`: `speed = 1.0f` (발사 간격, 초 단위)
- `Update()`: `timer` 누적 → `speed` 초과 시 `Fire()` 호출
- `Fire()`: `scanner.nearestTarget` 방향 계산 → `pool.Get(prefabId)`로 총알 취득 → `Quaternion.FromToRotation`으로 회전 → `Bullet.Init(damege, count, dir)`
- `count`: 관통 횟수, `LevelUp` 시 `+=`로 누적

**주의**: `Awake`에서 `GetComponentInParent<Player>()` 대신 `GameManager.instance.player`를 사용. `Item.OnClick()`이 `new GameObject()`로 생성할 때 아직 Player 자식이 아니므로 부모 탐색이 null을 반환하기 때문.

```
SpawnData 필드: spriteType, spawnTime, health, speed  (Spawner 외부 독립 클래스)
```

### 아이템 데이터: `ItemData`
`ScriptableObject` 기반 아이템 정의. `Assets/Undead Survivor/Data/`에 `.asset` 파일로 저장.

```
ItemType 열거형: Melee, Range, Glove, Shoe, Heal

[Main Info]  itemType, itemId, itemName, itemDesc([TextArea] 멀티라인), itemIcon(Sprite)
[Level Data] baseDamage, baseCount, damages[], counts[]
[Weapon]     projectile(GameObject) ← PoolManager.prefabs[]와 비교해 prefabId 탐색에 사용
             hand(Sprite)           ← 무기 획득 시 Hand.spriter에 적용할 손 스프라이트
```

`itemDesc`는 `[TextArea]` 속성으로 Inspector에서 멀티라인 편집 가능. `string.Format()` 플레이스홀더({0}, {1})로 수치를 동적 삽입.

### 아이템 UI: `Item`
`Canvas/LevelUp` 하위 아이템 버튼에 부착. 레벨업 선택 UI 버튼 하나하나를 담당. `ItemData`를 읽어 UI에 표시하고 클릭 시 무기/장비를 생성/레벨업.

- `Awake()`: `GetComponentsInChildren<Image>()[1]`로 아이콘 Image 취득. `texts[0]`=레벨, `texts[1]`=이름, `texts[2]`=설명. 이름은 고정값이므로 Awake에서 한 번만 세팅
- `OnEnable()`: 패널이 열릴 때마다 호출 — 레벨 표시(`level+1`, 1-based) 및 타입별 설명 텍스트 갱신. `string.Format(data.itemDesc, ...)`으로 수치 동적 삽입 (무기: 데미지%·관통수, 장비: 강화율%)
- `OnClick()`: Button의 onClick 이벤트에 연결. `level == data.damages.Length`이면 Button `interactable = false`

| ItemType | level==0 | level>0 | level++ |
|---|---|---|---|
| Melee/Range | Weapon 생성 + `Init(data)` | `LevelUp(nextDamage, counts[level])` | ✓ |
| Glove/Shoe | Gear 생성 + `init(data)` | `LevelUp(damages[level])` | ✓ |
| Heal | `GameManager.health = maxHealth` | 동일 (소모품) | ✗ |

**LevelUp 데이터 계산**:
- `nextDamage = baseDamage + baseDamage * damages[level]`
- `nextCount = weapon.count + counts[level]` (누적)
- Heal은 `damages[]`를 비워두면(`Length==0`) 한 번 사용 후 버튼 자동 비활성화

### 장비: `Gear`
`Item.OnClick()`에서 동적으로 생성. Player 자식으로 배치. `type`(Glove/Shoe)에 따라 플레이어 능력치 강화.

- `init(ItemData data)`: `type = data.itemType`, `rate = data.damages[0]`, `ApplyGear()` 호출
- `LevelUp(float rate)`: rate 갱신 후 `ApplyGear()` 재호출
- `ApplyGear()` → type별 분기:
  - **Glove → `RateUp()`**: Player 하위 모든 `Weapon`의 속도 강화
    - id=0: `weapon.speed = 150 + 150 * rate` (회전 속도 증가)
    - 원거리: `weapon.speed = 0.5f * (1f - rate)` (발사 간격 감소, 0.5f 기준으로 빠르게)
  - **Shoe → `SpeedUp()`**: `player.speed = 3 + 3 * rate` (이동속도 증가)

**Weapon과 연동**: Weapon `Init()`/`LevelUp()` 후 `BroadcastMessage("ApplyGear")` → 새 무기 생성 시 기존 Gear 효과 자동 적용. 수신자 없을 때 에러 방지를 위해 `SendMessageOptions.DontRequireReceiver` 사용.

### 무한 맵: `Reposition`
지형 타일과 적에 부착됩니다. `Area` 태그 트리거 콜라이더를 벗어나면 플레이어 근처로 순간이동합니다:
- `Ground` 태그: 주요 축 방향으로 타일 재배치 (+40 유닛)
- `Enemy` 태그: 랜덤 오프셋과 함께 플레이어 방향으로 재배치 (+30 유닛)

### 플레이어: `Player`
Unity 새 입력 시스템 사용 (`PlayerInput` 컴포넌트의 `OnMove` 콜백). `Update`에서 `GetAxisRaw`로 `inputVec` 갱신. `FixedUpdate`에서 `Rigidbody2D.MovePosition`으로 이동. `inputVec.magnitude`로 애니메이션 구동 (`Speed` 파라미터). `inputVec.x` 부호로 스프라이트 좌우 반전. `Awake`에서 `GetComponent<Scanner>()`로 Scanner 참조 획득.

**isLive 가드**: `Update`, `FixedUpdate`, `LateUpdate` 모두 `GameManager.instance.isLive` 체크 — 레벨업 일시정지 중 입력·이동·애니메이션 중단.

`public Hand[] hands`: `GetComponentsInChildren<Hand>(true)`로 비활성 포함 수집. `hands[0]` = 왼손(Melee), `hands[1]` = 오른손(Range). Inspector 계층 순서와 반드시 일치해야 함.

**캐릭터 애니메이션**: `public RuntimeAnimatorController[] animCon` — 캐릭터별 애니메이터 배열. `OnEnable()`에서 `animCon[playerId]`로 교체. `Assets/Undead Survivor/Animations/`에 Player, AcPlayer2, AcPlayer3 컨트롤러 저장.

**OnEnable()**: Player 활성화 시 `speed *= Charactor.Speed` 적용 후 애니메이터 교체.

**피격**: `OnCollisionStay2D` — 적과 접촉 중 `Time.deltaTime * 10` 씩 `GameManager.health` 감소 (초당 10 데미지). `health < 0` 시 자식 오브젝트(index 2~, 무기들) 비활성화 → `anim.SetTrigger("Dead")` → `GameManager.instance.GameOver()` 호출.

### 무기 UI: `Hand`
Player 자식 오브젝트에 부착. 플레이어 스프라이트 반전 시 무기 UI도 동일하게 반전.

- `isLeft`: true = 근접무기(왼손), false = 원거리무기(오른손)
- `spriter`: 이 Hand의 SpriteRenderer — `Weapon.Init()`에서 스프라이트 교체
- `Awake()`: `GetComponentsInParent<SpriteRenderer>()[1]`로 플레이어 SpriteRenderer 취득 ([0]은 Hand 자신)
- `LateUpdate()`: `player.flipX`를 읽어 방향 반전 처리 — Player.LateUpdate 이후 실행 보장을 위해 LateUpdate 사용

**근접(isLeft=true)**: 회전(`Quaternion.Euler`) + `flipY` + sortingOrder로 방향 표현
- 정방향 `-35°` / 반전 `-135°`, 반전 시 sortingOrder=4(플레이어 뒤)

**원거리(isLeft=false)**: 위치(`localPosition`) + `flipX` + sortingOrder로 방향 표현
- 정방향 `(0.35, -0.15)` / 반전 `(-0.15, -0.15)`, 반전 시 sortingOrder=6(플레이어 앞)

### 데이터 흐름

```
GameManager.Update → gameTime 누적 → Level 계산 → isLive 관리
Spawner.Update → spawnData[level].spawnTime 타이머
  → pool.Get(0) → Enemy.OnEnable → target = player
  → Enemy.Init(spawnData[level]) → speed/health/spriteType으로 animCon 교체
Player 이동 → Reposition.OnTriggerExit2D(Area) → 타일/적 순간이동
Enemy.FixedUpdate → MovePosition으로 플레이어 Rigidbody2D 추적

[근접 무기 id=0]
Weapon.Init → Batch → pool.Get(prefabId) → Bullet 자식 배치 → Bullet.Init(damege, -100, zero)
Weapon.Update → Rotate → 자식 Bullet들이 함께 공전
Enemy.OnTriggerEnter2D(Bullet) → health 감소 → KnockBack() → Hit 애니메이션
  → 체력 0 이하: isLive=false, coll/rigid 비활성화, Dead 애니메이션
  → Animation Event → Dead() → SetActive(false)
  → kill++, GetExp() → exp == nextExp[level] 시 level++

[원거리 무기 id=1]
Scanner.FixedUpdate → CircleCastAll → nearestTarget 갱신
Weapon.Update → timer 누적 → Fire()
Fire() → nearestTarget 방향 계산 → pool.Get(prefabId) → Bullet.Init(damege, count, dir)
Bullet → rigid.velocity로 이동
Bullet.OnTriggerEnter2D(Enemy) → per-- → per==-1이면 SetActive(false)
Enemy.OnTriggerEnter2D(Bullet) → health 감소 → (위 사망 흐름과 동일)
```

## 설계 패턴

### 전체 아키텍처 요약
이 프로젝트의 핵심 패턴 조합은 **Data-Driven + Component + Object Pool** 으로, Vampire Survivors 류 게임의 전형적인 구조입니다.

| 패턴 | 적용 위치 | 목적 |
|---|---|---|
| Data-Driven Design | `ItemData` (ScriptableObject) | 데이터·로직 분리, 코드 수정 없이 수치 조정 |
| Object Pool | `PoolManager` | 적·총알 재사용으로 GC 부하 감소 |
| Singleton | `GameManager.instance` | 전역 접근점 |
| Component | `Item`, `Weapon`, `Gear` | 단일 책임 분리, Unity 기본 구조 |
| Message Passing | `BroadcastMessage("ApplyGear")` | Weapon↔Gear 직접 참조 없이 느슨한 결합 |
| Factory (런타임 생성) | `Item.OnClick()` | `new GameObject() + AddComponent<>()` |

아이템 종류가 늘어날 때 ScriptableObject 에셋만 추가하면 되고, 코드는 `switch(itemType)` case만 늘리면 되는 확장 구조.

### 오브젝트 풀링
`Instantiate/Destroy` 대신 `pool.Get(index)` / `SetActive(false)`로 재사용. 모든 적·투사체에 적용.

### 싱글톤 (`GameManager.instance`)
전역 접근점. Player, Pool, gameTime, isLive 등을 `GameManager.instance`를 통해 참조.

### `Init()` 패턴
`OnEnable` 또는 외부 호출로 초기값 주입. `Enemy.Init(SpawnData)`, `Bullet.Init(damege, per, dir)`, `Weapon.Init(ItemData)`, `Gear.init(ItemData)` 모두 동일한 구조.

### `BroadcastMessage` 연동
`Weapon.Init()`/`LevelUp()` 후 `player.BroadcastMessage("ApplyGear", DontRequireReceiver)` 호출. Gear가 있으면 자동으로 효과 재적용, 없으면 무시. 장비-무기 간 결합도를 낮추는 패턴.

### `id` 기반 switch 분기
무기 종류를 `id`로 구분해 `Init()`과 `Update()` 모두 같은 switch 구조 사용. 무기 추가 시 case만 늘리면 됨.

### `Rotate → Translate(Space.World)` 배치
오브젝트 위치를 수식으로 직접 계산하지 않고, 회전 후 이동으로 균등 배치.

### 책임 분리
사망 처리(`Dead()`), 레벨업(`LevelUp()`) 등 기능별 메서드로 분리해 추후 확장 시 해당 메서드만 수정.

### `isLive` 가드 플래그
`FixedUpdate`, `LateUpdate`, `OnTriggerEnter2D` 모두 `isLive` 체크로 죽은 상태 방어.

### 자식 재활용 (`Batch`)
레벨업 시 기존 자식 총알은 재배치, 부족분만 풀에서 신규 취득. `parent` 설정은 새 총알에만 적용.

## 브랜치 컨벤션

현재 활성 브랜치: `feature/player` / 메인 브랜치: `main`

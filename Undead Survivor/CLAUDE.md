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
```

**타이머 시스템**: `maxGameTime = 20f`. `Update`에서 `gameTime`을 누적하고 `maxGameTime` 도달 시 `isLive = false`.

**레벨 시스템**: `gameTime / 10f`로 레벨 계산. 0~9초 = 레벨 0, 10~20초 = 레벨 1. 레벨 인덱스는 `PoolManager.prefabs[]` 인덱스와 1:1 대응.

### 오브젝트 풀링: `PoolManager`
정수 인덱스로 관리되는 재사용 가능한 GameObject 관리자. `prefabs[]`는 Inspector에서 할당합니다. `pool.Get(index)`로 비활성 오브젝트를 가져오거나 새로 생성합니다. 오브젝트는 PoolManager GameObject의 자식으로 배치됩니다.

- 인덱스 0, 1은 현재 적 프리팹 변형 (`Spawner`에서 사용)

### 적 스폰: `Spawner`
`SpawnData[]` 배열(내부 클래스)을 Inspector에서 레벨별로 설정합니다. `Update`에서 `GameManager.instance.gameTime / 10f`로 현재 레벨을 계산하고, `spawnData[level].spawnTime` 간격으로 스폰합니다. `isLive` 체크로 게임 종료 시 스폰 중단.

```
SpawnData 필드: spriteType, spawnTime, health, speed
```

스폰 흐름: `pool.Get(level)` → 위치 설정 → `enemy.GetComponent<Enemy>().Init(spawnData[level])`

### 적 행동: `Enemy`
`FixedUpdate`에서 `Rigidbody2D.MovePosition`으로 플레이어를 추적합니다. `OnEnable`에서 target 설정 및 `health = maxHealth` 초기화. `Init(SpawnData data)`로 레벨별 speed/health/animatorController 적용. `animCon[]` 배열로 `spriteType`에 따라 애니메이터 교체.

**피격/사망**: `OnTriggerEnter2D`에서 `Bullet` 태그 충돌 감지 → `health -= bullet.damege` → 체력 0 이하 시 `Dead()` 호출 → `SetActive(false)`로 풀 반환.

### 투사체: `Bullet`
`damege`(데미지)와 `per`(관통 횟수) 필드 보유. `Init(float damege, int per)`로 초기화. `per = -1`은 무한 관통.

### 무기: `Weapon`
플레이어 자식 오브젝트로 배치. `id`로 무기 종류 구분.

- `Init()`: `id` 기준 speed 설정 후 `Batch()` 호출
- `Update()`: `id` 기준 `transform.Rotate`로 무기 회전 (id=0: `Vector3.back * speed`)
- `Batch()`: `count`만큼 총알 배치. 기존 자식은 재배치, 부족분만 풀에서 신규 취득. `Rotate → Translate(Space.World)` 방식으로 균등 각도 배치
- `LevelUp(float damege, int count)`: 데미지/count 갱신 후 `Batch()` 재호출로 총알 재배치

```
SpawnData 필드: spriteType, spawnTime, health, speed  (Spawner 외부 독립 클래스)
```

### 무한 맵: `Reposition`
지형 타일과 적에 부착됩니다. `Area` 태그 트리거 콜라이더를 벗어나면 플레이어 근처로 순간이동합니다:
- `Ground` 태그: 주요 축 방향으로 타일 재배치 (+40 유닛)
- `Enemy` 태그: 랜덤 오프셋과 함께 플레이어 방향으로 재배치 (+30 유닛)

### 플레이어: `Player`
Unity 새 입력 시스템 사용 (`PlayerInput` 컴포넌트의 `OnMove` 콜백). `FixedUpdate`에서 `Rigidbody2D.MovePosition`으로 이동. `inputVec.magnitude`로 애니메이션 구동 (`Speed` 파라미터). `inputVec.x` 부호로 스프라이트 좌우 반전.

### 데이터 흐름

```
GameManager.Update → gameTime 누적 → Level 계산 → isLive 관리
Spawner.Update → spawnData[level].spawnTime 타이머
  → pool.Get(level) → Enemy.OnEnable → target = player
  → Enemy.Init(spawnData[level]) → speed/health/animator 적용
Player 이동 → Reposition.OnTriggerExit2D(Area) → 타일/적 순간이동
Enemy.FixedUpdate → MovePosition으로 플레이어 Rigidbody2D 추적
Weapon.Init → Batch → pool.Get(prefabId) → Bullet 자식 배치 → Bullet.Init(damege, per)
Weapon.Update → Rotate → 자식 Bullet들이 함께 공전
Enemy.OnTriggerEnter2D(Bullet) → health 감소 → Dead → SetActive(false)
```

## 설계 패턴

### 오브젝트 풀링
`Instantiate/Destroy` 대신 `pool.Get(index)` / `SetActive(false)`로 재사용. 모든 적·투사체에 적용.

### 싱글톤 (`GameManager.instance`)
전역 접근점. Player, Pool, gameTime, isLive 등을 `GameManager.instance`를 통해 참조.

### `Init()` 패턴
`OnEnable` 또는 외부 호출로 초기값 주입. `Enemy.Init(SpawnData)`, `Bullet.Init(damege, per)`, `Weapon.Init()` 모두 동일한 구조.

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

현재 활성 브랜치: `feature/enemy` / 메인 브랜치: `main`

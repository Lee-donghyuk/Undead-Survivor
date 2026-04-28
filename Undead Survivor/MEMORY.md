# MEMORY.md

## 프로젝트 개요
뱀파이어 서바이버 스타일 2D Unity 게임 — 유튜브 튜토리얼 기반, 높은 완성도 목표

## 기술 스택
- Unity 2022.3.62f3
- Render Pipeline: 2D URP (`com.unity.render-pipelines.core`)
- Input System: Unity New Input System (`com.unity.inputsystem@1.14.2`)
- 언어: C#

## 핵심 아키텍처 결정
- **싱글톤 GameManager**: 다른 스크립트는 `GameManager.instance.player`, `GameManager.instance.pool`로 접근
- **오브젝트 풀링**: `PoolManager.Get(index)`로 재사용 — index 0,1이 적 프리팹
- **적 추적**: `Rigidbody2D.MovePosition`을 `FixedUpdate`에서 사용 (물리 기반)
- **무한 맵**: `Reposition` 스크립트가 `Area` 태그 트리거 이탈 시 타일/적을 순간이동

## 코드 컨벤션
- 스크립트 위치: `Assets/Undead Survivor/code/`
- 적 활성화 초기화: `OnEnable()`에서 플레이어 타겟 설정 (풀에서 꺼낼 때마다 호출됨)
- `isLive` 플래그로 죽은 상태 가드
- 스폰 주기: 0.9초마다 랜덤 스폰 포인트에서 적 생성
- **메서드 체이닝**: 중간 변수 선언 없이 한 줄로 처리 (`GetComponent<T>().속성` 형태 선호)
- **주석**: 코드 작성 시 각 로직에 한국어 주석 추가
- **오브젝트 배치**: 위치를 직접 계산하지 않고 `Rotate` → `Translate` 순서로 처리 (`Space.World` 기준 이동 선호)
- **하드코딩 금지**: 수치값은 `public` 필드나 변수로 노출해 Inspector에서 조정 가능하게 작성

## 알려진 제약사항
- 빌드/테스트 CLI 없음 — 모든 실행은 Unity Editor에서만 가능
- 메인 씬: `Assets/Scenes/SampleScene.unity`
- 현재 활성 브랜치: `feature/enemy` / 메인 브랜치: `main`

## 하지 말 것
- `PoolManager.prefabs[]`는 Inspector에서 할당 — 코드로 직접 추가하지 말 것
- `GameManager` 씬 배치 순서 주의 — 다른 스크립트보다 먼저 초기화되어야 함
- `Reposition`의 Enemy 태그 처리 시 오프셋 없이 정확한 플레이어 위치로 이동시키지 말 것 (+30 랜덤 오프셋 필요)

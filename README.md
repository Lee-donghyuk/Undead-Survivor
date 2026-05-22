# Undead Survivor

> 뱀파이어 서바이버 스타일의 2D 탑다운 서바이벌 게임

## 소개

*Undead Survivor*는 Unity로 제작한 2D 탑다운 서바이벌 게임입니다.  
쏟아지는 언데드 무리 속에서 5분간 살아남는 것이 목표이며, 레벨업마다 무기와 장비를 선택해 캐릭터를 성장시킬 수 있습니다.

유튜브 튜토리얼 시리즈를 기반으로 제작했으며, 코드 구조 이해와 Unity 개발 역량 향상을 목표로 합니다.

# 참고 강의: [골드메탈 - 유니티 기초 강좌 뱀파이어 서바이버](https://www.youtube.com/playlist?list=PLO-mt5Iu5TeZF8xMHqtT_DhAPKmjF6i3x)

---

## 플레이 방법

| 조작 | 키 |
|---|---|
| 이동 | 조이스틱 / 방향키 |

- 이동만으로 주변 적을 자동으로 공격합니다
- 적을 처치하면 경험치를 획득하고 레벨업합니다
- 레벨업 시 무기·장비 중 하나를 선택해 강화합니다
- '5분' 동안 생존하면 승리

---

## 주요 기능

### 캐릭터 선택
4가지 캐릭터 중 하나를 선택해 시작합니다. 각 캐릭터는 고유한 특성을 가집니다.

| 캐릭터 | 특성 |
|---|---|
| 벼농부 | 이동속도 10% 증가 |
| 보리농부 | 언사속도 10% 증가 |
| 감자농부 | 데미지 20% 증가 |
| 사과농부 | 회전·관통 수 1 증가 |
> 감자농부,사과농부는 업적 달성 후 해금됩니다.

### 무기 시스템
- **근접 무기**: 플레이어 주변을 공전하며 무한 관통
- **원거리 무기**: 가장 가까운 적을 자동 조준 발사

### 장비 시스템
- **장갑(Glove)**: 무기 공격 속도 증가
- **신발(Shoe)**: 플레이어 이동 속도 증가

### 업적 시스템
- 적 10마리 처치 → 감자농부 해금
- 게임 클리어 → 사과농부 해금

---

## 구현 사항

| 시스템 | 설명 |
|---|---|
| 오브젝트 풀링 | `PoolManager`로 적·투사체를 재사용해 GC 부하 최소화 |
| 레벨업 UI | 레벨업 시 랜덤 3개 아이템 제시, 만렙 아이템은 회복으로 대체 |
| 동적 스폰 시스템 | 5분을 스폰 데이터 수로 균등 분할, 시간에 따라 난이도 자동 상승 |
| 오디오 매니저 | BGM 1채널 + SFX 16채널 관리, 레벨업 시 High-Pass 필터 효과 |
| 무한 맵 | 플레이어 위치 기준으로 타일·적을 순간이동시켜 무한한 맵 구현 |
| 업적 관리 | `PlayerPrefs`로 캐릭터 해금 상태 영구 저장 |
| 승패 처리 | 5분 생존 시 승리, 체력 소진 시 패배 연출 |

---

## 기술 스택

- **엔진**: Unity (URP 기반 2D 렌더 파이프라인)
- **언어**: C#
- **입력 시스템**: Unity New Input System
- **데이터**: ScriptableObject (`ItemData`)

---

## 프로젝트 구조

```
Assets/Undead Survivor/
├── code/
│   ├── GameManager.cs      # 싱글톤 허브, 게임 상태 관리
│   ├── PoolManager.cs      # 오브젝트 풀링
│   ├── Player.cs           # 플레이어 이동·피격
│   ├── Enemy.cs            # 적 추적·피격·사망
│   ├── Weapon.cs           # 근접/원거리 무기 로직
│   ├── Bullet.cs           # 투사체 (sentinel -100 = 무한관통)
│   ├── Gear.cs             # 장비 효과 적용
│   ├── Item.cs             # 레벨업 UI 버튼 제어
│   ├── ItemData.cs         # ScriptableObject 아이템 데이터
│   ├── Spawner.cs          # 레벨별 적 스폰
│   ├── Scanner.cs          # 가장 가까운 적 탐지
│   ├── AudioManager.cs     # BGM/SFX 관리
│   ├── AchiveManager.cs    # 업적·캐릭터 해금
│   ├── LevelUp.cs          # 레벨업 패널 제어
│   ├── Result.cs           # 승패 결과 UI
│   ├── Reposition.cs       # 무한 맵 타일 재배치
│   ├── Hand.cs             # 무기 UI 손 스프라이트
│   ├── HUD.cs              # 인게임 HUD
│   ├── Follow.cs           # 카메라 추적
│   └── Charactor.cs        # 캐릭터별 능력치 배율
└── Data/                   # ItemData ScriptableObject 에셋
```

---

## 실행 방법

1. Unity Hub에서 이 프로젝트를 열기
2. `Assets/Scenes/SampleScene.unity` 씬 열기
3. Play 버튼으로 실행

> **Unity 버전**: `ProjectSettings/ProjectVersion.txt` 참고

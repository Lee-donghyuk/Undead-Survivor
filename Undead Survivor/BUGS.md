# 알려진 버그 및 개선 포인트

추후 작업을 위해 발견된 버그와 개선 포인트를 기록합니다.

---

## 버그

### [BUG-001] GetExp() 배열 범위 초과
- **파일**: `Assets/Undead Survivor/code/GameManager.cs:55`
- **증상**: 플레이어 레벨이 9를 초과하면 `nextExp[level]` 접근 시 `IndexOutOfRangeException` 발생
- **원인**: `nextExp` 배열 크기(10)에 대한 클램프 없음
- **수정 방법**:
  ```csharp
  // 현재
  if (exp == nextExp[level])
  // 수정
  if (level < nextExp.Length && exp == nextExp[level])
  ```

---

## 개선 포인트

### [IMPROVE-001] 사망 시 KnockBack 코루틴 중복 실행
- **파일**: `Assets/Undead Survivor/code/Enemy.cs:85`
- **증상**: 적 사망 시에도 `KnockBack()` 코루틴이 실행됨. `rigid.simulated = false`로 실제 넉백은 무시되지만 불필요한 코루틴이 실행됨
- **수정 방법**: `Dead()` 처리 시 `StopAllCoroutines()` 추가

### [IMPROVE-003] 레벨업 UI 만렙 아이템 중복 대체 시 선택지 부족
- **파일**: `Assets/Undead Survivor/code/LevelUp.cs` — `Next()` 메서드
- **증상**: 랜덤 선택된 3개 중 만렙 아이템이 2개 이상이면 `items[4]`(힐)에 `SetActive(true)`가 중복 호출되어 실질적으로 2개 또는 1개의 선택지만 표시됨
- **원인**: 만렙 아이템을 `items[4]`로 단순 대체하므로 중복 처리 없음
- **수정 방법**: 대체 아이템 활성화 전 이미 활성화됐는지 확인하거나, 랜덤 선택 단계에서 만렙 아이템을 후보에서 제외

### ~~[IMPROVE-002] HUD Health 미구현~~ ✅ 해결
- `curHealth / maxHealth`로 Slider 연동 완료

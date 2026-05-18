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

### [IMPROVE-002] HUD Health 미구현
- **파일**: `Assets/Undead Survivor/code/HUD.cs:44`
- **증상**: `InfoType.Health` case가 비어있어 체력 UI 비표시
- **수정 방법**: Player 체력 시스템 구현 후 연동 필요

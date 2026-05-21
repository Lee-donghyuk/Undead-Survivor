# save

작업 내용을 CLAUDE.md에 저장하고 git에 자동 업로드한다.

## 실행 순서

1. **CLAUDE.md 업데이트**: 이번 작업에서 추가/수정된 스크립트, 필드, 흐름을 CLAUDE.md의 해당 섹션에 반영한다. 새 스크립트면 새 섹션 추가, 기존 스크립트 수정이면 해당 섹션 내용 갱신.

2. **현재 브랜치 커밋 & 푸시**: 변경된 코드 파일과 CLAUDE.md를 스테이징하고 작업 내용을 요약한 커밋 메시지로 커밋 후 `git push origin <현재 브랜치>`.

3. **develop 머지**: `git checkout develop` → `git merge --no-ff <작업 브랜치>` → `git push origin develop` → 작업 브랜치로 복귀.

4. **main 머지 여부 질문**: develop 머지 완료 후 main에 머지할지 사용자에게 확인한다. 승인 시 `git checkout main` → `git merge --no-ff develop` → `git push origin main` → 작업 브랜치 복귀.

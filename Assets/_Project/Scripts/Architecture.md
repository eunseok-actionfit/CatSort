# Clean Architecture for Unity
## Unified Naming, Layering, and DI Rules
### 최종 강화판 README

---

# 1. 전체 아키텍처 구조 (Top-Level Overview)

```
Domain          = 규칙 / 상태 / 엔티티               (new)
Application     = UseCase / 절차 / 비즈니스 흐름      (DI)
Infrastructure  = 저장 / 기술 / 외부 플랫폼 연동       (DI)
Presentation    = UI / 입력 / 화면 구성               (Unity + DI)
CompositionRoot = DI 조립 루트                        (DI)
```

---

# 2. 네이밍 규칙 (Unified Naming Standard)

## Domain
- (Feature)Entity
- (Feature)State
- (Feature)Definition
- (Feature)Value
- (Feature)DomainService

## Application
- (Action)UseCase
- (Action)Command
- (Name)Query
- (Action)CommandHandler

## Infrastructure
- I(Feature)Repository
- (Tech)(Feature)Repository
- (Feature)Service
- (Feature)(Tech)Service
- (Feature)Provider
- (Feature)Factory
- (Feature)Gateway
- (Name)Adapter
- (Name)Scheduler
- (Name)Bus
- (Feature)Manager

## Presentation
- (Feature)Controller
- (Feature)View
- (Feature)Presenter
- (Feature)ViewModel
- (Feature)Mediator
- (Feature)Navigator
- (Feature)Manager

## Composition Root
- GameLifetimeScope
- GameCompositionRoot

---

# 3. DI / new 생성 규칙

## new
- Entity
- State
- ValueObject
- DTO
- Event / Message
- ViewModel

## DI
- UseCase
- DomainService
- Repository
- Service
- Provider
- Gateway
- Adapter
- Factory
- Presenter / Mediator / Navigator / Manager

---

# 4. 레이어별 요약

## Domain Layer
- 역할: 규칙/상태/엔티티
- 생성: Entity/State/ValueObject → new, DomainService → DI

## Application Layer
- 역할: 절차/Flow/UseCase
- 생성: UseCase/Query/Handler → DI, Command → new

## Infrastructure Layer
- 역할: 저장/기술/SDK
- Repository, Service, Provider, Gateway, Adapter, Factory, Manager

## Presentation Layer
- 역할: UI/View/입력
- Controller/View → Unity, Presenter/Mediator/Navigator/Manager → DI, ViewModel → new/DI

## Composition Root
- DI 조립 루트

---

# 5. 선택 트리

```
데이터/상태 → Entity/State/Value
절차 → UseCase
명령 → Command
조회 → Query
저장/조회 → Repository
기능 제공 → Service
단순 값 → Provider
외부 서버/SDK → Gateway/Adapter
생성 전담 → Factory
리소스 관리 → Manager
UI 표시 → View
입력 → Controller
UI 갱신 → Presenter
UI 상태 → ViewModel
UI 조율 → Mediator
화면 이동 → Navigator
DI 시작 → CompositionRoot
```

---

# 6. 팀 운영 규칙

- Domain → Application → Infrastructure 단방향 의존
- Presentation은 Application/Infrastructure 인터페이스만 참조
- Domain은 완전 독립
- 파일 구조: Domain / Application / Infrastructure / Presentation / CompositionRoot
- 단위 테스트: Domain 100%, Application Mock 테스트

---

# 7. One Page Summary

```
레이어 구조 → 네이밍 규칙 → DI/new → 레이어 역할 → 판단 트리 → 운영 규칙
```

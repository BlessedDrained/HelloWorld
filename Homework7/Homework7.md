# Карта зависимостей StPlatform

## СЛОИ

```
┌─────────────────────────────────┐
│  config     — WebConfig,       │
│              SecurityConfig…    │
├─────────────────────────────────┤
│  api        — Api*Controller   │ ← REST-контроллеры
│  web        — *Controller      │ ← Thymeleaf-контроллеры
├─────────────────────────────────┤
│  auth       — ActivationSv, PwResetSv
│  consent    — ConsentService
│  user       — UserService + User
│  assignment — AssignmentService
│  submission — SubmissionService + LinkValidator, AttachmentStorage
│  aicheck    — AiCheckService, Worker, Agent…
├─────────────────────────────────┤
│  user.Repo, assignment.Repo,   │ ← JpaRepository-интерфейсы
│  submission.Repos (3 шт),      │
│  aicheck.AiCheckRepo           │
└─────────────────────────────────┘
```

---

## Цепочка: «сдать домашку» (REST `POST /api/submissions`)

```
ApiSubmissionController.submit()
 └→ SubmissionService.submit(student, id, links, comment, file)
     ├→ AssignmentRepository.findById(assignmentId)         // проверка существования/публикации/дедлайна
     ├→ ensureNotAlreadySent(student, assignmentId)
     │   └→ SubmissionRepository.findFirstByStudentIdAndAssignmentIdOrderByCreatedAtDesc
     ├→ linkValidator.isValid(eachLink)                     // https → gitverse.ru|github.com
     ├→ submissions.save(new Submission(...))               // JpaSubmiss. save → INSERT
     ├→ attachmentStorage.store(file)                       // если есть файл
     │   ├── ArchiveValidator.validateAttachment(fileName, path)  // zip-bomb защита
     │   └── Files.copy(input, storedPath)                 // запись на диск
     ├→ attachments.save(new SubmissionAttachment(submission, stored))  // INSERT метаданных в БД
     └→ events.publishEvent(SubmissionCreatedEvent(id))    // AFTER_COMMIT слушатель
          └→ AiCheckListener.onSubmissionCreated()
              └→ AiCheckService.enqueueIfAbsent(id)        // REQUIRES_NEW транзакция
                   └→ checks.save(new AiCheck(ref))        // INSERT очереди ИИ-проверки
```

## Цепочка: «регистрация» (REST `POST /api/auth/activate`)

```
ApiAuthController.activate()
 └→ ActivationService.activate(token, password, consent…)
     ├→ invitedByToken(rawToken)
     │   └→ UserRepository.findByActivationTokenHash(SHA256(token))  // валидация токена + TTL + статус
     ├→ requirePersonalDataConsent(user)
     │   └→ ConsentService.hasPersonalDataConsent(userId)
     │       └→ ConsentRecordRepository.existsByUserIdAndType(userId, PERSONAL_DATA)
     └→ setPasswordAndActivate(user, password)
         ├── user.setPasswordHash(passwordEncoder.encode(password))   // BCrypt
         ├── user.setStatus(ACTIVE)
         ├── user.setActivationTokenHash(null)                        // одноразовость
         └── UserRepository.save(user)                                // UPDATE
```

---

## Все ли стрелки идут «вниз»?

**Да**, основная масса — строго вниз. Единственное исключение:

> **AiCheckService.claimNext() читает Submission и SubmissionAttachment из репозиториев.** Это горизонтальная стрелка от `aicheck` → к `submission.Repos`. Она идёт не «наверх» (service ← controller), а на соседний слой сервиса — допустимо, так как aicheck тоже сервисный слой.

Ещё одна косвенная связь вверх по потоку выполнения:

> **SubmissionService.publishEvent(SubmissionCreatedEvent)** → слушатель в *другой транзакции* вызывает AiCheckService. Стрелка визуально «вверх» (`submission.aicheck → submission.service`), но это event-driven обратный вызов с REQUIRES_NEW — физически новая HTTP-запрос/транзакция, не синхронный call stack back.

---

## Границы, которые я бы провёл иначе

| Граница | Сейчас | Почему |
|---------|--------|--------|
| `LinkValidator` внутри пакета `submission` | Вспомогательный компонент лежит рядом с SubmissionService | Правильно — он domain-specific для сдач; выносить отдельно было бы overengineering |
| `ArchiveValidator` ↔ AttachmentStorage | Оба в пакете `submission`, один вызывает другой | ArchiveValidator мог бы быть самостоятельным утильным классом вне пакета submission, т.к. логика структурной проверки архива универсальна |
| `AiCheckBackfill` (ApplicationRunner) | Лежит в `aicheck` | Логично — запускается при старте приложения и работает поверх AiCheckService |

**Где граница была бы полезнее:** если бы `AttachmentStorage` использовался из нескольких сервисных пакетов (не только `submission`), его стоило бы перенести в общий уровень storage — сейчас же он заточен строго под файлы сдач, так что текущее размещение оптимально.

---

## Зачем нужна одна граница (одна фраза)

Каждая граница между слоями отвечает за **разделение ответственности**: контроллеры парсят HTTP-данные и формируют ответы, сервисы реализуют бизнес-правила (валидация, транзакции, события), репозитории скрывают SQL-детали — без границы слой выше «подсел» бы на детали реализации нижнего, а изменение БД ломало бы весь остальной код.

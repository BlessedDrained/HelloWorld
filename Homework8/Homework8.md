-- ════════════════════════════════════════════════
-- Запрос 1. Все сдачи студента по фамилии
-- ════════════════════════════════════════════════
-- Назначение: получить полную ленту всех сдач конкретного студента,
--             идентифицированного по фамилии (часть ФИО)
-- Схема связей: submissions.student_id → users.id
--               submissions.assignment_id → assignments.id
-- JOIN-логика:
--   • submissions — таблица фактов (каждая строка = одна сдача ДЗ)
--   • users       — данные студента (ФИО, группа, email)
--   • assignments — название и номер пары задания
-- Примечания:
--   • ILIKE '%Иванов%' — нечувствительная к регистру подстрока;
--     замените «Иванов» на нужную фамилию
--   • SPLIT_PART(full_name, ' ', 1) для точного совпадения первой
--     фамилии без частичных совпадений (чтобы «Петров» не попал в
--     результаты поиска «Петр»)
--   • edited_at показывает момент последней правки студентом (null —
--     не правилась); reviewed_at — момент последней проверки ПДн
--   • status хранит ReviewStatus: UNCHECKED / ACCEPTED / NEEDS_REWORK

SELECT s.id            AS submission_id,
       u.full_name     AS student_full_name,
       u.group_name    AS student_group,
       u.email         AS student_email,
       a.title         AS assignment_title,
       a.pair_number   AS pair_number,
       s.link          AS submission_links,
       s.student_comment,
       s.status        AS review_status,
       s.review_comment,
       s.reviewed_at,
       s.created_at,
       s.edited_at
FROM submissions s
JOIN users      u ON u.id = s.student_id
JOIN assignments a ON a.id = s.assignment_id
WHERE u.full_name ILIKE '%Иванов%'
ORDER BY s.created_at DESC;


-- ════════════════════════════════════════════════
-- Запрос 2. Количество сдач на каждое задание
-- ════════════════════════════════════════════════
-- Назначение: агрегировать число приёмленных работ по каждому заданию
--             для оценки загруженности пары / распространённости ДЗ
-- Схема связей: submissions.assignment_id → assignments.id
-- JOIN-логика:
--   • LEFT JOIN — чтобы задания без сдач тоже появились в результате
--     (COUNT(s.id) вернёт 0, а не пропустил строку как INNER JOIN)
--   • COUNT(id) счёит только непустые ссылки; NULL из пустого LEFT JOIN
--     игнорируется автоматически
-- Примечания:
--   • pair_number и title включены в GROUP BY — обязательное требование
--     SQL при использовании аггрегатных функций
--   • ORDER BY pair_number ASC сортирует от первой пары к последней

SELECT a.id              AS assignment_id,
       a.pair_number    AS pair_number,
       a.title           AS assignment_title,
       COUNT(s.id)       AS submission_count
FROM assignments a
LEFT JOIN submissions s ON s.assignment_id = a.id
GROUP BY a.id, a.pair_number, a.title
ORDER BY a.pair_number ASC;


-- ════════════════════════════════════════════════
-- Запрос 3. Студенты, не сдавшие ни одной работы
-- ════════════════════════════════════════════════
-- Назначение: выявить студентов-отличников или тех, кто ещё ничего не
--             сдал — для контроля посещаемости / успеваемости
-- Схема связей: submissions.student_id → users.id
-- LEFT JOIN … IS NULL:
--   • Левое соединение возвращает всех студентов; строки со сдачами
--     содержат непустой s.id, а студенты без сдач — null
--   • WHERE s.id IS NULL фильтрует только «пустые» строки, то есть
--     студентов без единой записи в submissions
-- Примечания:
--   • Фильтр u.role = 'STUDENT' исключает преподавателей из выборки
--     (в таблице users хранятся и те, и другие)
--   • status показывает UserStatus: INVITED (приглашён, не активирован)
--     или ACTIVE (активирован); можно дополнительно исключить INVITED
--     через AND u.status = 'ACTIVE', если нужны только вошедшие студенты

SELECT u.full_name     AS student_full_name,
       u.group_name    AS student_group,
       u.email         AS student_email,
       u.role          AS role,
       u.status        AS user_status
FROM users u
LEFT JOIN submissions s ON s.student_id = u.id
WHERE s.id IS NULL
  AND u.role = 'STUDENT';
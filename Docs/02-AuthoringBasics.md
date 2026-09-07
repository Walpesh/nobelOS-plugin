# Основы авторинга: из чего состоит история

## Карта ассетов

| Ассет | Меню создания | Зачем |
|---|---|---|
| Story Project | StoryEngine → Story Project | Точка входа игры: стартовый граф, список графов, атрибуты, финалы |
| Dialogue Graph | StoryEngine → Dialogue Graph | Глава/сцена: `Entry Node` + список всех узлов |
| Line Node | …→ Nodes → Line | Реплика: спикер, текст, портрет, голос, фон, следующий узел |
| Choice Node | …→ Nodes → Choice | Выбор: prompt + список вариантов |
| End Node | …→ Nodes → End | Финал ветки: `endingId`, титул, описание |
| Speaker | StoryEngine → Speaker | Имя, цвет имени, нарратор?, портрет по умолчанию |
| Attribute Definition | StoryEngine → Attribute Definition | Деньги/репутация/флаги: тип, дефолт, clamp |
| Background + Catalog | StoryEngine → Background / Background Catalog | Фоны по id, фейд, оверлей, параллакс |
| Ending Definition | StoryEngine → Ending Definition | Финал для галереи: условия «истинности», секретность |
| Localization CSV | StoryEngine → Localization CSV | Переводы `key;value` |

## Правила, которые экономят часы

1. **Одна глава = один граф** (30–80 узлов). Переход между главами — узел Graph Jump (см. 03).
2. **Список `Nodes` графа держи полным**: по нему работают валидатор и граф-редактор.
3. Тексты пиши в `Fallback`, ключ перевода — в `Key` (см. 07).
4. Нумеруй узлы шагом 10 (`…_010_`, `…_020_`) — будет куда вставлять сцены.
5. Нарратор = Speaker с галкой `Is Narrator` (имя в UI скрывается автоматически).
6. Портрет по умолчанию живёт в Speaker; эмоция конкретной реплики — `Portrait Override` узла.
7. Голос — `Voice Clip` узла (опционально; UI проигрывает его сам, если подключишь).

## Жизненный цикл главы

```
создать Graph → набросать узлы (граф-редактор или Inspector)
→ связать ссылки → условия/эффекты на выборах → Validate → Story Preview → правки
```

## Проверка без прохождения с начала

**StoryEngine → Story Preview** → выбери граф и любой узел → *Play from Selected Node*.
Unity войдёт в Play Mode и начнёт историю ровно с этого места.
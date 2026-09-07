# Editor Tools

## Dialogue Graph Editor (`StoryEngine → Dialogue Graph Editor`)

- Выбор графа в тулбаре; **Reload** пересобирает вид из данных (источник истины — всегда ассеты).
- ПКМ по полю → Create Line / Choice / End: узел создастся ассетом и добавится в граф.
- Связи: тяни из порта `next` (Line) или `[i]` (Choice) в порт `in` целевого узла.
- Перетаскивание узлов сохраняет позиции в ассеты.
- ПКМ по узлу: *Clear Next/All* (снять ссылки), *Select Asset* (выделить в Project).
- **Validate**: базовые проверки + недоступные узлы + циклы.

> После *Clear Next/All* нажми **Reload**, чтобы вид совпал с данными.

## Attribute Manager (`StoryEngine → Attribute Manager`)

- Создание атрибутов Number / Flag / String одной кнопкой (сразу добавляются в проект).
- Список атрибутов проекта + живые значения в Play Mode (колонка `live`).
- Быстрый переход к ассету (`Select`) и удаление из проекта (`X`).

## Condition Builder

Встроен в инспектор Choice Node: меню создания условий и эффектов,
вложенные инспекторы параметров, удаление в один клик. См. [03](03-BranchingConditionsEffects.md).

## Ending Condition Editor

Инспектор Ending Definition: счётчик достижений финала игроками
и кнопка теста требований в Play Mode.

## Story Preview (`StoryEngine → Story Preview`)

Выбор графа и стартового узла → *Play from Entry* / *Play from Selected Node*.
Идеально для прогона отдельных веток и регресса условий.

## Валидатор: что означают предупреждения

| Сообщение | Лечение |
|---|---|
| Entry Node is not assigned | назначь входной узел графа |
| has no Next Node | проставь ссылку дальше |
| not in this graph Nodes list | добавь узел в `Nodes` графа |
| unreachable from Entry Node | удали узел или соедини с историей |
| Cycle detected | разорви петлю (если она не задумана) |
| Duplicate node id | пересоздай id узла (инспектор предложит новый при очистке) |
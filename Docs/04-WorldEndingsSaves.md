# Миры, финалы, сохранения

## Фоны и атмосфера

1. **Create → StoryEngine → Background**: `id` (например `BG_town_night`), спрайт,
   `Fade Duration`, `Overlay Color/Alpha` (ночная затемнённость), `Parallax Strength`.
2. Собери фоны в **Background Catalog**.
3. На узлах диалога укажи `Background Id` — смена произойдёт автоматически с фейдом.
4. Поля `Location / Time Of Day / Mood` на узле — данные для условий Scene Context
   и будущих систем музыки/света.

Арт можно заменить позже: меняешь спрайт внутри Background-ассета — все ссылки целы.

## Финалы

1. Конец ветки = **End Node** (`endingId`, титул, описание).
2. На каждый `endingId` создай **Ending Definition**: титул/описание для галереи,
   `Requirements` (условия «истинного» финала), `Is Secret`.
3. Добавь все Ending Definition в `Story Project → Endings` — UI посчитает прогресс «X из Y».
4. Проверка требований в Play Mode: инспектор Ending Definition → *Test requirements in Play Mode*.

Мета-профиль (открытые финалы, счётчики выборов, число прохождений) копится автоматически
и переживает перезапуски игры.

## Сохранения

- 3 слота + JSON в `Application.persistentDataPath/StoryEngine/`.
- Сохранить: `SaveSystem.SaveSlot(slot, runner.CaptureSaveData())`
- Загрузить: `runner.ApplySaveData(SaveSystem.LoadSlot(slot))`
- Автосейв на главу и UI слотов — готовые сниппеты в [06-SceneAndUI](06-SceneAndUI.md).
- Сейв восстанавливает: текущий узел, атрибуты, флаги, посещённые узлы и историю выборов.

## Галерея финалов

Сниппет экрана «открыто X из Y, секретные — ???» — в [06-SceneAndUI](06-SceneAndUI.md#галерея-финалов).
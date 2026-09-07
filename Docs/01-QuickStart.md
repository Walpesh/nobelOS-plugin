# QuickStart: играбельная новелла за 10 минут

Цель: запустить пример, посмотреть все системы, создать свою первую сцену.

## Шаг 1. Установка

Скопируй папку плагина в `Assets/Plugins/StoryEngine/` (или UPM: *Add package from disk…* → `package.json`).
В меню Unity появится пункт **StoryEngine**.

## Шаг 2. Генерация примера

**StoryEngine → Create Full Sample Story (All Systems)**

Создастся папка `Assets/StoryEngineFullSample/` с историей:
деньги и репутация, флаги, скрытые и серые выборы, фоны, 5 финалов, CSV-локализация.

## Шаг 3. Сцена для примера

1. Новая пустая сцена.
2. `GameObject → Create Empty` → назови `StoryRunner`.
3. `Add Component → Dialogue Runner`; в поле **Story Project** перетащи `Project_FullSample`.
4. `Add Component → Debug Dialogue Presenter` и `Debug Dialogue Input`.
5. **Play**.

Управление: `Space` или клик — следующая реплики; `1…5` — варианты ответа.
Обрати внимание: «Купить карту» исчезнет при нехватке денег, «Нанять охрану» будет серой без репутации.

## Шаг 4. Посмотри историю как граф

**StoryEngine → Dialogue Graph Editor** → выбери `Graph_Main`.
Узлы можно перетаскивать, связи — тянуть из портов `next` / `[i]` в порт `in`.

## Шаг 5. Своя первая реплика

1. ПКМ в Project → **Create → StoryEngine → Nodes → Line**.
2. Заполни `Text → Fallback`, выбери `Speaker` (создай через **Create → StoryEngine → Speaker**).
3. Создай второй Line и **End** (`Create → StoryEngine → Nodes → End`).
4. Создай **Dialogue Graph**, перетащи все три узла в `Nodes`, первый — в `Entry Node`.
5. Создай **Story Project**, укажи `Start Graph`, повесь его на `StoryRunner` вместо примера.
6. Play. Готово — это твоя новелла.

## Шаг 6. Валидация

Выдели граф → кнопка **Validate Graph** в инспекторе.
Ошибки вида «нет Next Node» или «узел недоступен» лечатся перетаскиванием ссылок.

## Куда дальше

- Полноценный UI вместо консоли: [06-SceneAndUI](06-SceneAndUI.md)
- Ветвления и условия: [03-BranchingConditionsEffects](03-BranchingConditionsEffects.md)
- Если что-то не работает: [09-Troubleshooting](09-Troubleshooting.md)
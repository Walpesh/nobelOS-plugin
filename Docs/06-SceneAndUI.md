# Сцена и UI

UI отделён от логики: контроллер реализует контракт презентера и дёргает два метода раннера.
Свою визуальную тему можно собрать поверх того же контракта.

## Иерархия сцены Game

```text
StoryRunner            DialogueRunner (Start On Start = FALSE)
                       + ChapterBootstrap / LocalizationBootstrap / AutoSave (сниппеты ниже)
DialogueCanvas         Canvas + CanvasScaler + GraphicRaycaster (+ CanvasGroup на корне)
├─ Background          Image, stretch, Raycast Target = off
├─ Overlay             Image, stretch, Raycast Target = off
├─ Portrait            Image, Raycast Target = off
├─ DialogueBox         Image-плашка
│   ├─ SpeakerName     TextMeshProUGUI
│   ├─ DialogueText    TextMeshProUGUI
│   └─ ContinueIndicator  TMP «▼»
├─ ChoicesPanel        VerticalLayoutGroup + ContentSizeFitter
└─ EndingPanel         титул, описание, статистика, кнопки меню
BackgroundManager      runner, Background, Overlay, Catalog, ParallaxTarget
DialogueUIController   ссылки по именам объектов выше
EventSystem            GameObject → UI → EventSystem
```

Префаб кнопки выбора: `Button` + TMP-текст + компонент `Dialogue Choice Button`
(сохранить префабом, назначить в `Choice Button Prefab`).

## Поведение ввода по умолчанию

- клик/`Space`: первый клик доскрывает текст (typewriter), второй — следующая реплика;
- кнопки выборов кликабельны мышью; серые — недоступны;
- скорость печати: `Characters Per Second` на контроллере.

Для настроек добавь в `DialogueUIController` два метода:

```csharp
public void SetTypewriterSpeed(float cps)    => charactersPerSecond = Mathf.Max(1f, cps);
public void SetTypewriterEnabled(bool value) => typewriterEnabled = value;
```

## Меню: New Game / Continue / слоты

```csharp
public class MainMenuController : MonoBehaviour
{
    [SerializeField] private DialogueRunner runner;

    public void NewGame()  { SceneManager.LoadScene("Game"); /* там runner.StartStory() */ }

    public void Continue()
    {
        var data = SaveSystem.LoadSlot(0);           // 0 = автосейв
        if (data == null) return;
        GameBootstrap.PendingSave = data;
        SceneManager.LoadScene("Game");
    }

    public void SaveTo(int slot)   => SaveSystem.SaveSlot(slot, runner.CaptureSaveData());
    public void LoadFrom(int slot)
    {
        var data = SaveSystem.LoadSlot(slot);
        if (data != null) runner.ApplySaveData(data);
    }
}
```

Подписи слотов: `SaveSystem.HasSlot(i)`, `SaveSystem.GetSlotTimestamp(i)`.

## Автосейв на каждую главу

```csharp
public class AutoSave : MonoBehaviour
{
    [SerializeField] private DialogueRunner runner;
    private System.Action<DialogueGraphAsset> _onGraph;

    private void OnEnable()
    {
        _onGraph = _ => SaveSystem.SaveSlot(0, runner.CaptureSaveData());
        runner.GraphStarted += _onGraph;
    }
    private void OnDisable() => runner.GraphStarted -= _onGraph;
}
```

## Галерея финалов

```csharp
var profile = SaveSystem.LoadProfile();
bool unlocked = profile.unlockedEndings.Contains(endingDefinition.id);
// титул или "???", если IsSecret и не открыт
```

## Смена языка

```csharp
runner.SetLocalizationProvider(index == 0 ? localizationRu : localizationEn);
```

Подробнее о CSV и ключах: [07-Localization](07-Localization.md).
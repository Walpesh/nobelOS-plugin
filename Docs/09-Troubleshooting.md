# Troubleshooting

## Компиляция

| Симптом | Решение |
|---|---|
| Ошибки про TMP/TextMeshPro | Импортируй TMP Essentials (окно TextMeshPro при первом использовании) |
| Ошибки UnityEngine.UI | Убедись, что стоит пакет uGUI (есть в 2D-шаблонах) |

## Рантайм

| Симптом | Причина / лечение |
|---|---|
| `[END] id=error.missing_node` сразу | не назначен `Entry Node` графа |
| `[END] id=error.missing_node` после реплики | у Line нет `Next Node` |
| `[END] id=error.no_choices` | у Choice пустой список вариантов |
| Выбор не виден | не выполнено show-условие (так и задумано) или опечатка в id флага/атрибута |
| Выбор серый | не выполнено enable-условие |
| Кнопки не кликаются | нет EventSystem; либо у Background/DialogueBox включён Raycast Target |
| Текст не перевёлся | ключа нет в CSV активного языка |
| Сейв не восстановился | ссылка `Next Node` ушла в другой граф — используй Graph Jump |
| Бесконечный цикл | Validate (advanced) покажет узел петли |
| UI не появился | у DialogueRunner не назначен Story Project или презентер перетёрт debug-компонентом |

## Читы для тестов

```csharp
public class DebugCheats : MonoBehaviour
{
    [SerializeField] private DialogueRunner runner;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1)) runner.Attributes.AddNumber("ATT_money", 100);
        if (Input.GetKeyDown(KeyCode.F2)) runner.Attributes.SetBool("FLAG_met_guide", true);
    }
}
```

Держи компонент только на dev-сцене, не в билде.
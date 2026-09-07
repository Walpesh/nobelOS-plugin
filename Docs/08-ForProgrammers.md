# Для программистов: архитектура и расширение

## Слои

```
Data (ScriptableObject)  →  Logic (DialogueRunner, handlers, conditions, effects)
                          →  Presentation (IDialoguePresenter / события / UI-модуль)
```

- Раннер не знает о UI; UI не знает о данных напрямую.
- Сборки: `StoryEngine.Core`, `StoryEngine.UI`, `StoryEngine.Core.Editor`, `StoryEngine.EditorTools`.

## События раннера

`StoryStarted`, `GraphStarted`, `NodeEntered`, `LinePresented`, `ChoicesPresented`,
`ChoiceSelected`, `StoryFinished`, `AttributeChanged`, `StoryEventRaised`.

## Свой UI

Реализуй `IDialoguePresenter` (`Clear`, `OnLinePresented`, `OnChoicesPresented`, `OnStoryFinished`)
и вызови `runner.SetPresenter(...)`, либо подпишись на события.
Ввод: `runner.Advance()`, `runner.SelectChoice(index)`.

## Кастомный узел

```csharp
[CreateAssetMenu(menuName = "StoryEngine/Nodes/Graph Jump")]
public class GraphJumpNodeAsset : DialogueNodeAsset
{
    public DialogueGraphAsset targetGraph;
    public DialogueNodeAsset targetNode;                 // null = entry
    public override DialogueNodeKind Kind => DialogueNodeKind.Generic;
}

public class GraphJumpHandler : IDialogueNodeHandler
{
    public bool CanHandle(DialogueNodeAsset node) => node is GraphJumpNodeAsset;
    public void Enter(DialogueRunner runner, DialogueNodeAsset node)
    {
        var jump = (GraphJumpNodeAsset)node;
        runner.StartGraph(jump.targetGraph);
        if (jump.targetNode != null) runner.GoToNode(jump.targetNode);
    }
}

// регистрация (один раз, на бутстрапе сцены):
runner.RegisterHandler(new GraphJumpHandler());
```

## Кастомные условия и эффекты

```csharp
public class MyCondition : ConditionAsset
{
    public override bool Evaluate(in ConditionContext ctx) => ctx.attributes.GetNumber("ATT_money") > 0;
}

public class MyEffect : EffectAsset
{
    public override void Apply(in EffectContext ctx) => ctx.attributes.AddNumber("ATT_money", 1);
}
```

## Кастомная локализация

Реализуй `ILocalizationProvider` (`string GetLocalizedText(string key)`) —
например, из Addressables, JSON или Google Sheets кэша.

## Performance-контракты

- Никакого LINQ и аллокаций в горячих путях раннера; typewriter через TMP `maxVisibleCharacters`.
- Кнопки выборов — пул; списки событий переиспользуются.
- UI-события передают struct-аргументы; `ChoicesPresentedArgs.choices` валиден до следующего перехода узла
  (для асинхронных анимаций копируй через `runner.CopyActiveChoicesTo(buffer)`).
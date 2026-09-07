using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using StoryEngine.Core;

namespace StoryEngine.EditorTools
{
    public static class FullSampleCreator
    {
        private const string Root = "Assets/StoryEngineFullSample";

        [MenuItem("StoryEngine/Create Full Sample Story (All Systems)")]
        public static void Create()
        {
            EnsureFolder("Assets", "StoryEngineFullSample");
            EnsureFolder(Root, "Conditions");
            EnsureFolder(Root, "Effects");
            EnsureFolder(Root, "Backgrounds");

            var dirty = new List<Object>();

            // Attributes
            var money = Create<AttributeDefinitionAsset>($"Attributes/Attr_Money");
            money.id = "attr_money"; money.displayName = "Money"; money.type = AttributeType.Number;
            money.defaultNumber = 20; money.clampNumber = true; money.minNumber = 0; dirty.Add(money);

            var reputation = Create<AttributeDefinitionAsset>($"Attributes/Attr_Reputation");
            reputation.id = "attr_rep"; reputation.displayName = "Reputation"; reputation.type = AttributeType.Number;
            reputation.defaultNumber = 0; dirty.Add(reputation);

            var metGuide = Create<AttributeDefinitionAsset>($"Attributes/Flag_MetGuide");
            metGuide.id = "flag_met_guide"; metGuide.displayName = "Met Guide"; metGuide.type = AttributeType.Bool;
            dirty.Add(metGuide);

            // Speakers
            var narrator = Create<SpeakerAsset>("Speaker_Narrator");
            narrator.id = "spk_narrator"; narrator.displayName = "Narrator"; narrator.isNarrator = true; dirty.Add(narrator);

            var guide = Create<SpeakerAsset>("Speaker_Guide");
            guide.id = "spk_guide"; guide.displayName = "Guide"; guide.nameColor = Color.cyan; dirty.Add(guide);

            // Backgrounds
            var catalog = Create<BackgroundCatalogAsset>("Catalog_Backgrounds");
            dirty.Add(catalog);

            // Conditions
            var condMoney = Create<AttributeCondition>("Conditions/Cond_Money10");
            condMoney.attributeId = "attr_money"; condMoney.op = CompareOp.GreaterOrEqual; condMoney.numberValue = 10; dirty.Add(condMoney);

            var condRep = Create<AttributeCondition>("Conditions/Cond_Rep1");
            condRep.attributeId = "attr_rep"; condRep.op = CompareOp.GreaterOrEqual; condRep.numberValue = 1; dirty.Add(condRep);

            // Effects
            var effMoney = Create<ModifyAttributeEffect>("Effects/Eff_MoneyMinus10");
            effMoney.attributeId = "attr_money"; effMoney.operation = ModifyAttributeEffect.Operation.Add; effMoney.numberValue = -10; dirty.Add(effMoney);

            var effRep = Create<ModifyAttributeEffect>("Effects/Eff_RepPlus1");
            effRep.attributeId = "attr_rep"; effRep.operation = ModifyAttributeEffect.Operation.Add; effRep.numberValue = 1; dirty.Add(effRep);

            var effFlag = Create<SetFlagEffect>("Effects/Eff_MetGuide");
            effFlag.flagId = "flag_met_guide"; effFlag.value = true; dirty.Add(effFlag);

            // Graph
            var graph = Create<DialogueGraphAsset>("Graph_Main");
            graph.id = NewId(); dirty.Add(graph);

            var intro = Line(narrator, "intro_line", "Дождь. Перекрёсток. Торговец картами смотрит на твой кошель.", "bg_crossroads");
            var choice = Create<ChoiceNodeAsset>("Node_01_Choice");
            choice.id = NewId(); choice.backgroundId = "bg_crossroads";
            choice.prompt = new LocalizedText { key = "choice_main", fallback = "Твои действия?" };
            dirty.Add(choice);

            var mapLine = Line(narrator, "", "Ты покупаешь карту. Мост через реку теперь не загадка.", "bg_bridge");
            var leftLine = Line(narrator, "", "Левая дорога пахнет мокрым камнем.", "bg_bridge");
            var rightLine = Line(narrator, "", "Правая дорога уходит в тёмный лес.", "bg_forest");
            var guideLine = Line(guide, "", "Гид: Я знаю этот перекрёсток. Спроси — и я подскажу.", "bg_crossroads");
            var guardLine = Line(guide, "", "Охранник идёт рядом. Теперь ты не один.", "bg_crossroads");

            var endMap = End("ending_map", "Карта в руках", "Знание маршрута — тоже сила.");
            var endLeft = End("ending_left", "Левый путь", "Камень и вода привели тебя к цели.");
            var endRight = End("ending_right", "Лесной путь", "Лес забрал часть тебя, но пропустил.");
            var endGuide = End("ending_guide", "Совет гида", "Ты узнал больше, чем планировал.");
            var endGuard = End("ending_guard", "Под охраной", "Репутация открыла тебе защиту.");

            choice.choices.Clear();
            choice.choices.Add(new DialogueChoice
            {
                id = "choice_buy_map",
                label = new LocalizedText { fallback = "Купить карту (10 монет)" },
                nextNode = mapLine,
                showConditions = new List<ConditionAsset> { condMoney },
                effects = new List<EffectAsset> { effMoney }
            });
            choice.choices.Add(new DialogueChoice
            {
                id = "choice_left",
                label = new LocalizedText { fallback = "Пойти налево" },
                nextNode = leftLine,
                effects = new List<EffectAsset> { effRep }
            });
            choice.choices.Add(new DialogueChoice
            {
                id = "choice_right",
                label = new LocalizedText { fallback = "Пойти направо" },
                nextNode = rightLine
            });
            choice.choices.Add(new DialogueChoice
            {
                id = "choice_guide",
                label = new LocalizedText { fallback = "Поговорить с гидом" },
                nextNode = guideLine,
                effects = new List<EffectAsset> { effFlag }
            });
            choice.choices.Add(new DialogueChoice
            {
                id = "choice_guard",
                label = new LocalizedText { fallback = "Нанять охрану (нужна репутация)" },
                nextNode = guardLine,
                enableConditions = new List<ConditionAsset> { condRep }
            });

            intro.nextNode = choice;
            mapLine.nextNode = endMap;
            leftLine.nextNode = endLeft;
            rightLine.nextNode = endRight;
            guideLine.nextNode = endGuide;
            guardLine.nextNode = endGuard;

            graph.entryNode = intro;
            graph.nodes.Clear();
            graph.nodes.AddRange(new DialogueNodeAsset[]
            {
                intro, choice, mapLine, leftLine, rightLine, guideLine, guardLine,
                endMap, endLeft, endRight, endGuide, endGuard
            });

            // Endings definitions
            var project = Create<StoryProjectAsset>("Project_FullSample");
            project.id = NewId();
            project.startGraph = graph;
            project.graphs.Add(graph);
            project.attributes.AddRange(new[] { money, reputation, metGuide });
            dirty.Add(project);

            foreach (var end in new[] { endMap, endLeft, endRight, endGuide, endGuard })
            {
                var definition = Create<EndingDefinitionAsset>($"Endings/Ending_{end.endingId}");
                definition.id = end.endingId;
                definition.title = end.endingTitle;
                definition.description = end.endingDescription;
                project.endings.Add(definition);
                dirty.Add(definition);
            }

            // Localization CSV sample
            var csvPath = $"{Root}/Localization_sample.csv";
            File.WriteAllText(csvPath, "key;value\nintro_line;Rain. Crossroads. The map seller eyes your purse.\nchoice_main;What will you do?\n");
            AssetDatabase.Refresh();
            var csvAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(csvPath);
            var localization = Create<LocalizationCsvAsset>("Localization_Sample");
            typeof(LocalizationCsvAsset)
                .GetField("csvFile", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(localization, csvAsset);
            dirty.Add(localization);

            foreach (var obj in dirty) EditorUtility.SetDirty(obj);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = project;
            Debug.Log($"Full sample created in {Root}. Assign Localization_Sample to DialogueRunner via SetLocalizationProvider or a bootstrap component.");
        }

        private static LineNodeAsset Line(SpeakerAsset speaker, string key, string fallback, string backgroundId)
        {
            var node = Create<LineNodeAsset>($"Node_{System.Guid.NewGuid().ToString("N").Substring(0, 6)}_Line");
            node.id = NewId();
            node.speaker = speaker;
            node.backgroundId = backgroundId;
            node.text = new LocalizedText { key = key, fallback = fallback };
            return node;
        }

        private static EndNodeAsset End(string endingId, string title, string description)
        {
            var node = Create<EndNodeAsset>($"Node_{endingId}_End");
            node.id = NewId();
            node.endingId = endingId;
            node.endingTitle = new LocalizedText { fallback = title };
            node.endingDescription = new LocalizedText { fallback = description };
            return node;
        }

        private static T Create<T>(string relativePath) where T : ScriptableObject
        {
            var asset = ScriptableObject.CreateInstance<T>();
            var path = AssetDatabase.GenerateUniqueAssetPath($"{Root}/{relativePath}.asset");
            var directory = Path.GetDirectoryName(path);
            if (!AssetDatabase.IsValidFolder(directory))
                Directory.CreateDirectory(directory);
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void EnsureFolder(string parent, string name)
        {
            var path = $"{parent}/{name}";
            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder(parent, name);
        }

        private static string NewId() => System.Guid.NewGuid().ToString("N");
    }
}
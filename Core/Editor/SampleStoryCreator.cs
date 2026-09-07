using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using StoryEngine.Core;

namespace StoryEngine.Core.Editor
{
    public static class SampleStoryCreator
    {
        [MenuItem("StoryEngine/Create Sample Story (Core)")]
        public static void CreateSampleStory()
        {
            const string folder = "Assets/StoryEngineSample";
            EnsureFolder("Assets", "StoryEngineSample");

            var narrator = CreateAsset<SpeakerAsset>(folder, "Speaker_Narrator");
            narrator.id = NewId();
            narrator.displayName = "Narrator";
            narrator.isNarrator = true;

            var guide = CreateAsset<SpeakerAsset>(folder, "Speaker_Guide");
            guide.id = NewId();
            guide.displayName = "Guide";

            var graph = CreateAsset<DialogueGraphAsset>(folder, "Graph_SampleCore");
            graph.id = NewId();
            graph.title = new LocalizedText
            {
                fallback = "Sample Core Story"
            };

            var start = CreateAsset<LineNodeAsset>(folder, "Node_00_Start");
            start.id = NewId();
            start.editorTitle = "Start";
            start.speaker = narrator;
            start.backgroundId = "bg_crossroads";
            start.text = new LocalizedText
            {
                fallback = "Вы стоите на развилке. Слева слышен шум воды, справа — тихий шёпот леса."
            };

            var choice = CreateAsset<ChoiceNodeAsset>(folder, "Node_01_Choice");
            choice.id = NewId();
            choice.editorTitle = "First Choice";
            choice.backgroundId = "bg_crossroads";
            choice.prompt = new LocalizedText
            {
                fallback = "Куда пойти?"
            };

            choice.choices.Clear();
            choice.choices.Add(new DialogueChoice
            {
                label = new LocalizedText { fallback = "Пойти налево" },
                isCorrect = true
            });
            choice.choices.Add(new DialogueChoice
            {
                label = new LocalizedText { fallback = "Пойти направо" }
            });
            choice.choices.Add(new DialogueChoice
            {
                label = new LocalizedText { fallback = "Спросить гида" }
            });

            var left = CreateAsset<LineNodeAsset>(folder, "Node_02_Left");
            left.id = NewId();
            left.editorTitle = "Left Path";
            left.speaker = narrator;
            left.backgroundId = "bg_bridge";
            left.text = new LocalizedText
            {
                fallback = "Вы идёте налево и выходите к старому мосту."
            };

            var right = CreateAsset<LineNodeAsset>(folder, "Node_03_Right");
            right.id = NewId();
            right.editorTitle = "Right Path";
            right.speaker = narrator;
            right.backgroundId = "bg_forest";
            right.text = new LocalizedText
            {
                fallback = "Вы сворачиваете направо. Лес смыкается за спиной."
            };

            var ask = CreateAsset<LineNodeAsset>(folder, "Node_04_Ask");
            ask.id = NewId();
            ask.editorTitle = "Ask Guide";
            ask.speaker = guide;
            ask.backgroundId = "bg_crossroads";
            ask.text = new LocalizedText
            {
                fallback = "Гид: Оба пути ведут к цели, но левый короче. Правый — безопаснее."
            };

            var endLeft = CreateAsset<EndNodeAsset>(folder, "Node_10_EndLeft");
            endLeft.id = NewId();
            endLeft.editorTitle = "End Left";
            endLeft.endingId = "ending_left";
            endLeft.endingTitle = new LocalizedText
            {
                fallback = "Финал: Левый путь"
            };
            endLeft.endingDescription = new LocalizedText
            {
                fallback = "Вы выбрали быстрый, но рискованный путь."
            };

            var endRight = CreateAsset<EndNodeAsset>(folder, "Node_11_EndRight");
            endRight.id = NewId();
            endRight.editorTitle = "End Right";
            endRight.endingId = "ending_right";
            endRight.endingTitle = new LocalizedText
            {
                fallback = "Финал: Правый путь"
            };
            endRight.endingDescription = new LocalizedText
            {
                fallback = "Вы выбрали осторожную дорогу."
            };

            var endAsk = CreateAsset<EndNodeAsset>(folder, "Node_12_EndAsk");
            endAsk.id = NewId();
            endAsk.editorTitle = "End Ask";
            endAsk.endingId = "ending_ask";
            endAsk.endingTitle = new LocalizedText
            {
                fallback = "Финал: Совет гида"
            };
            endAsk.endingDescription = new LocalizedText
            {
                fallback = "Вы предпочли сначала узнать больше."
            };

            start.nextNode = choice;

            left.nextNode = endLeft;
            right.nextNode = endRight;
            ask.nextNode = endAsk;

            choice.choices[0].nextNode = left;
            choice.choices[1].nextNode = right;
            choice.choices[2].nextNode = ask;

            graph.entryNode = start;
            graph.nodes.Clear();
            graph.nodes.Add(start);
            graph.nodes.Add(choice);
            graph.nodes.Add(left);
            graph.nodes.Add(right);
            graph.nodes.Add(ask);
            graph.nodes.Add(endLeft);
            graph.nodes.Add(endRight);
            graph.nodes.Add(endAsk);

            var project = CreateAsset<StoryProjectAsset>(folder, "Project_SampleCore");
            project.id = NewId();
            project.startGraph = graph;
            project.graphs.Clear();
            project.graphs.Add(graph);

            UnityEngine.Object[] dirtyObjects =
            {
                narrator,
                guide,
                graph,
                start,
                choice,
                left,
                right,
                ask,
                endLeft,
                endRight,
                endAsk,
                project
            };

            foreach (var obj in dirtyObjects)
                EditorUtility.SetDirty(obj);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = project;

            Debug.Log("Sample core story created in Assets/StoryEngineSample");
        }

        private static T CreateAsset<T>(string folder, string assetName) where T : ScriptableObject
        {
            var asset = ScriptableObject.CreateInstance<T>();
            var path = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{assetName}.asset");
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void EnsureFolder(string parent, string name)
        {
            string path = $"{parent}/{name}";
            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder(parent, name);
        }

        private static string NewId()
        {
            return System.Guid.NewGuid().ToString("N");
        }
    }
}
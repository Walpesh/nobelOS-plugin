using System.IO;
using UnityEditor;
using UnityEngine;
using StoryEngine.Core;

namespace StoryEngine.EditorTools
{
    [CustomEditor(typeof(ChoiceNodeAsset))]
    public class ChoiceNodeAssetEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var choiceNode = (ChoiceNodeAsset)target;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Condition & Effect Builder", EditorStyles.boldLabel);

            for (int i = 0; i < choiceNode.choices.Count; ++i)
            {
                var choice = choiceNode.choices[i];
                if (choice == null) continue;

                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField($"[{i}] {choice.label.fallback}", EditorStyles.boldLabel);

                DrawConditionList("Show if", choice.showConditions, choiceNode);
                DrawConditionList("Enable if", choice.enableConditions, choiceNode);
                DrawEffectList(choice.effects, choiceNode);

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawConditionList(string title, System.Collections.Generic.List<ConditionAsset> list, ChoiceNodeAsset owner)
        {
            EditorGUILayout.LabelField(title, EditorStyles.miniLabel);

            for (int i = 0; i < list.Count; ++i)
            {
                EditorGUILayout.BeginHorizontal();
                list[i] = (ConditionAsset)EditorGUILayout.ObjectField(list[i], typeof(ConditionAsset), false);
                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    list.RemoveAt(i);
                    EditorUtility.SetDirty(owner);
                    GUIUtility.ExitGUI();
                }
                EditorGUILayout.EndHorizontal();

                if (list[i] != null) DrawChildEditor(list[i]);
            }

            if (GUILayout.Button($"+ {title} condition"))
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Attribute"), false, () => AddCondition<AttributeCondition>(list, owner));
                menu.AddItem(new GUIContent("Flag"), false, () => AddCondition<FlagCondition>(list, owner));
                menu.AddItem(new GUIContent("Node Visited"), false, () => AddCondition<NodeVisitedCondition>(list, owner));
                menu.AddItem(new GUIContent("Choice Made"), false, () => AddCondition<ChoiceMadeCondition>(list, owner));
                menu.AddItem(new GUIContent("Scene Context"), false, () => AddCondition<SceneContextCondition>(list, owner));
                menu.AddItem(new GUIContent("Group (All/Any)"), false, () => AddCondition<ConditionGroup>(list, owner));
                menu.ShowAsContext();
            }
        }

        private void DrawEffectList(System.Collections.Generic.List<EffectAsset> list, ChoiceNodeAsset owner)
        {
            EditorGUILayout.LabelField("Effects on select", EditorStyles.miniLabel);

            for (int i = 0; i < list.Count; ++i)
            {
                EditorGUILayout.BeginHorizontal();
                list[i] = (EffectAsset)EditorGUILayout.ObjectField(list[i], typeof(EffectAsset), false);
                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    list.RemoveAt(i);
                    EditorUtility.SetDirty(owner);
                    GUIUtility.ExitGUI();
                }
                EditorGUILayout.EndHorizontal();

                if (list[i] != null) DrawChildEditor(list[i]);
            }

            if (GUILayout.Button("+ Effect"))
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Modify Attribute"), false, () => AddEffect<ModifyAttributeEffect>(list, owner));
                menu.AddItem(new GUIContent("Set Flag"), false, () => AddEffect<SetFlagEffect>(list, owner));
                menu.AddItem(new GUIContent("Set String"), false, () => AddEffect<SetStringAttributeEffect>(list, owner));
                menu.AddItem(new GUIContent("Raise Story Event"), false, () => AddEffect<RaiseStoryEventEffect>(list, owner));
                menu.ShowAsContext();
            }
        }

        private void DrawChildEditor(ScriptableObject child)
        {
            var editor = UnityEditor.Editor.CreateEditor(child);
            EditorGUILayout.BeginVertical("helpbox");
            editor.OnInspectorGUI();
            EditorGUILayout.EndVertical();
            DestroyImmediate(editor);
        }

        private void AddCondition<T>(System.Collections.Generic.List<ConditionAsset> list, ChoiceNodeAsset owner) where T : ConditionAsset
        {
            list.Add(CreateSubAsset<T>(owner, typeof(T).Name));
            EditorUtility.SetDirty(owner);
        }

        private void AddEffect<T>(System.Collections.Generic.List<EffectAsset> list, ChoiceNodeAsset owner) where T : EffectAsset
        {
            list.Add(CreateSubAsset<T>(owner, typeof(T).Name));
            EditorUtility.SetDirty(owner);
        }

        private T CreateSubAsset<T>(ChoiceNodeAsset owner, string typeName) where T : ScriptableObject
        {
            var folder = Path.GetDirectoryName(AssetDatabase.GetAssetPath(owner));
            if (string.IsNullOrEmpty(folder)) folder = "Assets";

            var conditionsFolder = $"{folder}/Conditions";
            if (!AssetDatabase.IsValidFolder(conditionsFolder))
                AssetDatabase.CreateFolder(folder, "Conditions");

            var asset = CreateInstance<T>();
            var path = AssetDatabase.GenerateUniqueAssetPath($"{conditionsFolder}/{owner.name}_{typeName}.asset");
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            return asset;
        }
    }
}
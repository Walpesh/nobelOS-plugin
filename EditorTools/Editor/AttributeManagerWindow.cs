using UnityEditor;
using UnityEngine;
using StoryEngine.Core;

namespace StoryEngine.EditorTools
{
    public class AttributeManagerWindow : EditorWindow
    {
        private StoryProjectAsset _project;
        private Vector2 _scroll;

        [MenuItem("StoryEngine/Attribute Manager")]
        public static void Open() => GetWindow<AttributeManagerWindow>("Attributes");

        private void OnGUI()
        {
            _project = (StoryProjectAsset)EditorGUILayout.ObjectField("Project", _project, typeof(StoryProjectAsset), false);
            if (_project == null) return;

            EditorGUILayout.Space();

            if (GUILayout.Button("New Number Attribute")) CreateAttribute(AttributeType.Number);
            if (GUILayout.Button("New Flag (Bool) Attribute")) CreateAttribute(AttributeType.Bool);
            if (GUILayout.Button("New String Attribute")) CreateAttribute(AttributeType.String);

            EditorGUILayout.Space();

            var runner = Object.FindObjectOfType<DialogueRunner>();

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            for (int i = 0; i < _project.attributes.Count; ++i)
            {
                var definition = _project.attributes[i];
                if (definition == null) continue;

                EditorGUILayout.BeginHorizontal("box");
                EditorGUILayout.LabelField(definition.displayName, GUILayout.Width(160));
                EditorGUILayout.LabelField(definition.type.ToString(), GUILayout.Width(60));

                string live = "(not playing)";
                if (runner != null && runner.Attributes.Has(definition.id))
                {
                    var value = runner.Attributes.Get(definition.id);
                    live = definition.type == AttributeType.Number
                        ? value.number.ToString("0.##")
                        : definition.type == AttributeType.Bool ? value.boolValue.ToString() : value.stringValue;
                }

                EditorGUILayout.LabelField($"live: {live}");

                if (GUILayout.Button("Select", GUILayout.Width(60)))
                    Selection.activeObject = definition;

                if (GUILayout.Button("X", GUILayout.Width(24)))
                {
                    _project.attributes.RemoveAt(i);
                    EditorUtility.SetDirty(_project);
                    GUIUtility.ExitGUI();
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();

            if (GUI.changed) Repaint();
        }

        private void CreateAttribute(AttributeType type)
        {
            var asset = CreateInstance<AttributeDefinitionAsset>();
            asset.id = System.Guid.NewGuid().ToString("N");
            asset.type = type;
            asset.displayName = $"New {type} Attribute";

            var path = AssetDatabase.GenerateUniqueAssetPath($"Assets/StoryData/Attributes/Attr_{asset.displayName}.asset");
            System.IO.Directory.CreateDirectory("Assets/StoryData/Attributes");
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();

            _project.attributes.Add(asset);
            EditorUtility.SetDirty(_project);
        }
    }
}
using System.Collections.Generic;
using UnityEngine;

namespace StoryEngine.Core
{
    [CreateAssetMenu(fileName = "DialogueGraph", menuName = "StoryEngine/Dialogue Graph", order = 1)]
    public class DialogueGraphAsset : ScriptableObject
    {
        public string id;
        public LocalizedText title;
        public DialogueNodeAsset entryNode;
        public List<DialogueNodeAsset> nodes = new List<DialogueNodeAsset>();

        public DialogueNodeAsset GetNodeById(string nodeId)
        {
            if (string.IsNullOrEmpty(nodeId))
                return null;

            for (int i = 0; i < nodes.Count; ++i)
            {
                var node = nodes[i];
                if (node != null && node.id == nodeId)
                    return node;
            }

            return null;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(id))
            {
                id = UnityEditor.AssetDatabase.AssetPathToGUID(UnityEditor.AssetDatabase.GetAssetPath(this));
                if (string.IsNullOrEmpty(id))
                    id = System.Guid.NewGuid().ToString("N");
            }
        }

        public List<string> Validate()
        {
            var issues = new List<string>();
            ValidateInto(issues);
            return issues;
        }

        public void ValidateInto(List<string> issues)
        {
            issues.Clear();

            if (entryNode == null)
                issues.Add($"[{name}] Entry Node is not assigned.");

            var ids = new HashSet<string>();
            var nodeSet = new HashSet<DialogueNodeAsset>();

            for (int i = 0; i < nodes.Count; ++i)
            {
                var node = nodes[i];
                if (node == null)
                {
                    issues.Add($"[{name}] nodes[{i}] is null.");
                    continue;
                }

                if (!nodeSet.Add(node))
                    issues.Add($"[{name}] Node '{node.name}' is duplicated in Nodes list.");

                if (string.IsNullOrEmpty(node.id))
                    issues.Add($"[{name}] Node '{node.name}' has empty id.");
                else if (!ids.Add(node.id))
                    issues.Add($"[{name}] Duplicate node id '{node.id}' on node '{node.name}'.");

                if (node is LineNodeAsset line)
                {
                    if (line.nextNode == null)
                        issues.Add($"[{name}] Line node '{line.name}' has no Next Node.");
                    else if (!nodes.Contains(line.nextNode))
                        issues.Add($"[{name}] Line node '{line.name}' links to node '{line.nextNode.name}' which is not in this graph Nodes list.");
                }
                else if (node is ChoiceNodeAsset choice)
                {
                    if (choice.choices == null || choice.choices.Count == 0)
                    {
                        issues.Add($"[{name}] Choice node '{choice.name}' has no choices.");
                    }
                    else
                    {
                        for (int c = 0; c < choice.choices.Count; ++c)
                        {
                            var option = choice.choices[c];
                            if (option == null)
                            {
                                issues.Add($"[{name}] Choice node '{choice.name}' choice[{c}] is null.");
                                continue;
                            }

                            if (option.nextNode == null)
                                issues.Add($"[{name}] Choice node '{choice.name}' choice[{c}] has no Next Node.");
                            else if (!nodes.Contains(option.nextNode))
                                issues.Add($"[{name}] Choice node '{choice.name}' choice[{c}] links to node '{option.nextNode.name}' which is not in this graph Nodes list.");
                        }
                    }
                }
            }

            if (entryNode != null && !nodes.Contains(entryNode))
                issues.Add($"[{name}] Entry node '{entryNode.name}' is not in Nodes list.");
        }
#endif
    }
}
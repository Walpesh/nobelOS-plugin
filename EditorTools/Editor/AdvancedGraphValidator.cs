using System.Collections.Generic;
using StoryEngine.Core;

namespace StoryEngine.EditorTools
{
    public static class AdvancedGraphValidator
    {
        public static List<string> Validate(DialogueGraphAsset graph)
        {
            var issues = new List<string>();
            if (graph == null) return issues;

            graph.ValidateInto(issues);

            var reachable = new HashSet<DialogueNodeAsset>();
            if (graph.entryNode != null)
                CollectReachable(graph.entryNode, reachable);

            for (int i = 0; i < graph.nodes.Count; ++i)
            {
                var node = graph.nodes[i];
                if (node == null) continue;

                if (!reachable.Contains(node))
                    issues.Add($"[{graph.name}] Node '{node.name}' is unreachable from Entry Node.");
            }

            DetectCycles(graph, issues);

            return issues;
        }

        private static void CollectReachable(DialogueNodeAsset node, HashSet<DialogueNodeAsset> visited)
        {
            if (node == null || !visited.Add(node)) return;

            if (node is LineNodeAsset line)
            {
                CollectReachable(line.nextNode, visited);
            }
            else if (node is ChoiceNodeAsset choice)
            {
                for (int i = 0; i < choice.choices.Count; ++i)
                {
                    if (choice.choices[i] != null)
                        CollectReachable(choice.choices[i].nextNode, visited);
                }
            }
        }

        private static void DetectCycles(DialogueGraphAsset graph, List<string> issues)
        {
            var state = new Dictionary<DialogueNodeAsset, int>();

            for (int i = 0; i < graph.nodes.Count; ++i)
            {
                var node = graph.nodes[i];
                if (node != null && Dfs(node, state, new List<string>()))
                {
                    issues.Add($"[{graph.name}] Cycle detected involving node '{node.name}'.");
                    return;
                }
            }
        }

        private static bool Dfs(DialogueNodeAsset node, Dictionary<DialogueNodeAsset, int> state, List<string> path)
        {
            if (node == null) return false;

            if (state.TryGetValue(node, out int mark))
            {
                if (mark == 1) return true;
                return false;
            }

            state[node] = 1;

            bool cycle = false;

            if (node is LineNodeAsset line)
            {
                cycle = Dfs(line.nextNode, state, path);
            }
            else if (node is ChoiceNodeAsset choice)
            {
                for (int i = 0; i < choice.choices.Count && !cycle; ++i)
                {
                    if (choice.choices[i] != null)
                        cycle = Dfs(choice.choices[i].nextNode, state, path);
                }
            }

            state[node] = 2;
            return cycle;
        }
    }
}
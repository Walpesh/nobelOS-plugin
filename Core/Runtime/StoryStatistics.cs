using System.Collections.Generic;

namespace StoryEngine.Core
{
    public class StoryStatistics
    {
        private readonly HashSet<string> _visitedNodes = new HashSet<string>();
        private readonly HashSet<string> _madeChoices = new HashSet<string>();
        private readonly Dictionary<string, int> _choiceCounts = new Dictionary<string, int>();

        public IReadOnlyCollection<string> VisitedNodes => _visitedNodes;
        public IReadOnlyDictionary<string, int> ChoiceCounts => _choiceCounts;
        public string LastEndingId { get; set; }

        public bool HasVisitedNode(string nodeId)
            => !string.IsNullOrEmpty(nodeId) && _visitedNodes.Contains(nodeId);

        public bool HasMadeChoice(string choiceId)
            => !string.IsNullOrEmpty(choiceId) && _madeChoices.Contains(choiceId);

        public void MarkNodeVisited(string nodeId)
        {
            if (!string.IsNullOrEmpty(nodeId))
                _visitedNodes.Add(nodeId);
        }

        public void MarkChoice(string choiceId)
        {
            if (string.IsNullOrEmpty(choiceId)) return;

            _madeChoices.Add(choiceId);
            _choiceCounts.TryGetValue(choiceId, out int count);
            _choiceCounts[choiceId] = count + 1;
        }

        public void RestoreVisited(List<string> visited)
        {
            _visitedNodes.Clear();
            if (visited == null) return;
            for (int i = 0; i < visited.Count; ++i)
                _visitedNodes.Add(visited[i]);
        }

        public void RestoreChoices(List<string> choices)
        {
            _madeChoices.Clear();
            if (choices == null) return;
            for (int i = 0; i < choices.Count; ++i)
                _madeChoices.Add(choices[i]);
        }
    }
}
using System.Collections.Generic;
using UnityEngine;

namespace StoryEngine.Core
{
    public struct ConditionContext
    {
        public DialogueRunner runner;
        public StoryAttributeState attributes;
        public SceneContext sceneContext;

        public bool HasVisited(string nodeId) => runner != null && runner.Statistics.HasVisitedNode(nodeId);
        public bool HasMadeChoice(string choiceId) => runner != null && runner.Statistics.HasMadeChoice(choiceId);
    }

    public abstract class ConditionAsset : ScriptableObject
    {
        [TextArea(1, 2)] public string comment;
        public abstract bool Evaluate(in ConditionContext ctx);
    }

    public enum CompareOp { Equal, NotEqual, Greater, GreaterOrEqual, Less, LessOrEqual }
    public enum CompareAs { Number, Bool, String }

    [CreateAssetMenu(fileName = "CondAttribute", menuName = "StoryEngine/Conditions/Attribute", order = 50)]
    public class AttributeCondition : ConditionAsset
    {
        public string attributeId;
        public CompareAs compareAs = CompareAs.Number;
        public CompareOp op = CompareOp.GreaterOrEqual;
        public float numberValue;
        public bool boolValue;
        public string stringValue;

        public override bool Evaluate(in ConditionContext ctx)
        {
            if (ctx.attributes == null || !ctx.attributes.Has(attributeId)) return false;

            var current = ctx.attributes.Get(attributeId);

            switch (compareAs)
            {
                case CompareAs.Bool:
                    return current.boolValue == boolValue;

                case CompareAs.String:
                    bool eq = string.Equals(current.stringValue, stringValue, System.StringComparison.Ordinal);
                    return op == CompareOp.NotEqual ? !eq : eq;

                default:
                    switch (op)
                    {
                        case CompareOp.Equal: return Mathf.Approximately(current.number, numberValue);
                        case CompareOp.NotEqual: return !Mathf.Approximately(current.number, numberValue);
                        case CompareOp.Greater: return current.number > numberValue;
                        case CompareOp.GreaterOrEqual: return current.number >= numberValue;
                        case CompareOp.Less: return current.number < numberValue;
                        default: return current.number <= numberValue;
                    }
            }
        }
    }

    [CreateAssetMenu(fileName = "CondFlag", menuName = "StoryEngine/Conditions/Flag", order = 51)]
    public class FlagCondition : ConditionAsset
    {
        public string flagId;
        public bool expected = true;
        public override bool Evaluate(in ConditionContext ctx)
            => ctx.attributes != null && ctx.attributes.GetBool(flagId) == expected;
    }

    [CreateAssetMenu(fileName = "CondVisited", menuName = "StoryEngine/Conditions/Node Visited", order = 52)]
    public class NodeVisitedCondition : ConditionAsset
    {
        public string nodeId;
        public bool requireVisited = true;
        public override bool Evaluate(in ConditionContext ctx)
            => ctx.HasVisited(nodeId) == requireVisited;
    }

    [CreateAssetMenu(fileName = "CondChoice", menuName = "StoryEngine/Conditions/Choice Made", order = 53)]
    public class ChoiceMadeCondition : ConditionAsset
    {
        public string choiceId;
        public override bool Evaluate(in ConditionContext ctx) => ctx.HasMadeChoice(choiceId);
    }

    [CreateAssetMenu(fileName = "CondContext", menuName = "StoryEngine/Conditions/Scene Context", order = 54)]
    public class SceneContextCondition : ConditionAsset
    {
        public enum Field { Location, TimeOfDay, Mood }
        public Field field;
        public string expectedValue;

        public override bool Evaluate(in ConditionContext ctx)
        {
            string actual = field == Field.Location
                ? ctx.sceneContext.locationId
                : field == Field.TimeOfDay ? ctx.sceneContext.timeOfDay : ctx.sceneContext.mood;

            return string.Equals(actual, expectedValue, System.StringComparison.Ordinal);
        }
    }

    [CreateAssetMenu(fileName = "CondGroup", menuName = "StoryEngine/Conditions/Group (All/Any)", order = 55)]
    public class ConditionGroup : ConditionAsset
    {
        public enum Mode { All, Any }
        public Mode mode = Mode.All;
        public List<ConditionAsset> conditions = new List<ConditionAsset>();

        public override bool Evaluate(in ConditionContext ctx)
        {
            if (conditions.Count == 0) return true;

            for (int i = 0; i < conditions.Count; ++i)
            {
                var condition = conditions[i];
                if (condition == null) continue;

                bool result = condition.Evaluate(ctx);
                if (mode == Mode.Any && result) return true;
                if (mode == Mode.All && !result) return false;
            }

            return mode == Mode.All;
        }
    }

    public static class ConditionEvaluator
    {
        public static bool AllPass(List<ConditionAsset> conditions, in ConditionContext ctx)
        {
            if (conditions == null || conditions.Count == 0) return true;

            for (int i = 0; i < conditions.Count; ++i)
            {
                var condition = conditions[i];
                if (condition == null) continue;
                if (!condition.Evaluate(ctx)) return false;
            }

            return true;
        }
    }
}
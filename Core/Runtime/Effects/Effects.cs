using UnityEngine;

namespace StoryEngine.Core
{
    public struct EffectContext
    {
        public DialogueRunner runner;
        public StoryAttributeState attributes;
        public SceneContext sceneContext;
    }

    public abstract class EffectAsset : ScriptableObject
    {
        [TextArea(1, 2)] public string comment;
        public abstract void Apply(in EffectContext ctx);
    }

    [CreateAssetMenu(fileName = "EffModifyAttr", menuName = "StoryEngine/Effects/Modify Attribute", order = 60)]
    public class ModifyAttributeEffect : EffectAsset
    {
        public enum Operation { Add, Set }
        public string attributeId;
        public Operation operation = Operation.Add;
        public float numberValue;

        public override void Apply(in EffectContext ctx)
        {
            if (ctx.attributes == null) return;

            if (operation == Operation.Add)
                ctx.attributes.AddNumber(attributeId, numberValue);
            else
                ctx.attributes.SetNumber(attributeId, numberValue);
        }
    }

    [CreateAssetMenu(fileName = "EffSetFlag", menuName = "StoryEngine/Effects/Set Flag", order = 61)]
    public class SetFlagEffect : EffectAsset
    {
        public string flagId;
        public bool value = true;
        public override void Apply(in EffectContext ctx)
        {
            if (ctx.attributes != null)
                ctx.attributes.SetBool(flagId, value);
        }
    }

    [CreateAssetMenu(fileName = "EffSetString", menuName = "StoryEngine/Effects/Set String Attribute", order = 62)]
    public class SetStringAttributeEffect : EffectAsset
    {
        public string attributeId;
        public string value;
        public override void Apply(in EffectContext ctx)
        {
            if (ctx.attributes != null)
                ctx.attributes.SetString(attributeId, value);
        }
    }

    [CreateAssetMenu(fileName = "EffStoryEvent", menuName = "StoryEngine/Effects/Raise Story Event", order = 63)]
    public class RaiseStoryEventEffect : EffectAsset
    {
        public string eventName;
        public override void Apply(in EffectContext ctx)
        {
            if (ctx.runner != null)
                ctx.runner.RaiseStoryEvent(eventName);
        }
    }
}
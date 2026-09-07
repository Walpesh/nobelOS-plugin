using System;
using System.Collections.Generic;

namespace StoryEngine.Core
{
    public class StoryAttributeState
    {
        private readonly Dictionary<string, AttributeValue> _values = new Dictionary<string, AttributeValue>();
        private readonly Dictionary<string, AttributeDefinitionAsset> _defs = new Dictionary<string, AttributeDefinitionAsset>();

        public event Action<string, AttributeValue> Changed;

        public Dictionary<string, AttributeValue> Raw => _values;

        public void Initialize(List<AttributeDefinitionAsset> definitions)
        {
            _values.Clear();
            _defs.Clear();

            if (definitions == null) return;

            for (int i = 0; i < definitions.Count; ++i)
            {
                var def = definitions[i];
                if (def == null || string.IsNullOrEmpty(def.id)) continue;

                _defs[def.id] = def;

                var value = new AttributeValue();
                switch (def.type)
                {
                    case AttributeType.Number: value.number = def.defaultNumber; break;
                    case AttributeType.Bool: value.boolValue = def.defaultBool; break;
                    default: value.stringValue = def.defaultString; break;
                }

                _values[def.id] = value;
            }
        }

        public bool Has(string id) => !string.IsNullOrEmpty(id) && _values.ContainsKey(id);

        public AttributeValue Get(string id)
        {
            return _values.TryGetValue(id, out var value) ? value : default;
        }

        public float GetNumber(string id) => Get(id).number;
        public bool GetBool(string id) => Get(id).boolValue;
        public string GetString(string id) => Get(id).stringValue;

        public void Set(string id, AttributeValue value)
        {
            if (string.IsNullOrEmpty(id)) return;

            if (_defs.TryGetValue(id, out var def) && def.type == AttributeType.Number && def.clampNumber)
                value.number = UnityEngine.Mathf.Clamp(value.number, def.minNumber, def.maxNumber);

            _values[id] = value;
            Changed?.Invoke(id, value);
        }

        public void SetNumber(string id, float value) => Set(id, AttributeValue.AsNumber(value));
        public void SetBool(string id, bool value) => Set(id, AttributeValue.AsBool(value));
        public void SetString(string id, string value) => Set(id, AttributeValue.AsString(value));

        public void AddNumber(string id, float delta)
        {
            SetNumber(id, GetNumber(id) + delta);
        }

        public void FillSave(List<AttributeSaveEntry> target)
        {
            target.Clear();
            foreach (var pair in _values)
            {
                target.Add(new AttributeSaveEntry
                {
                    id = pair.Key,
                    number = pair.Value.number,
                    boolValue = pair.Value.boolValue,
                    stringValue = pair.Value.stringValue
                });
            }
        }

        public void Restore(List<AttributeSaveEntry> entries)
        {
            if (entries == null) return;

            for (int i = 0; i < entries.Count; ++i)
            {
                var entry = entries[i];
                if (entry == null || string.IsNullOrEmpty(entry.id)) continue;

                Set(entry.id, new AttributeValue
                {
                    number = entry.number,
                    boolValue = entry.boolValue,
                    stringValue = entry.stringValue
                });
            }
        }
    }
}
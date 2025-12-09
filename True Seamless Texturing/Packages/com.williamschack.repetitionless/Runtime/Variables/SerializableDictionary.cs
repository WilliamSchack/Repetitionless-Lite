using System.Collections.Generic;
using UnityEngine;

namespace Repetitionless.Variables
{
    // Not fleshed out but does what i need
    // Uses two lists for keys and values

    [System.Serializable]
    public class SerializableDictionary<TKey, TValue>
    {
        [SerializeField] private List<TKey> keys = new List<TKey>();
        [SerializeField] private List<TValue> values = new List<TValue>();

        public int Count { get { return keys.Count; }}

        public TValue Get(TKey key)
        {
            int index = keys.IndexOf(key);
            return values[index];
        }

        public void Set(TKey key, TValue value)
        {
            int index = keys.IndexOf(key);
            
            if (index < 0) {
                keys.Add(key);
                values.Add(value);
            } else {
                values[index] = value;
            }
        }

        public void Remove(TKey key)
        {
            int index = keys.IndexOf(key);
            if (index < 0)
                return;

            keys.RemoveAt(index);
            values.RemoveAt(index);
        }

        public bool ContainsKey(TKey key)
        {
            return keys.Contains(key);
        }
    }
}
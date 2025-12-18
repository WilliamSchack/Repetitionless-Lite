using System.Collections.Generic;
using UnityEngine;

namespace Repetitionless.Variables
{
    /// <summary>
    /// A simple serializable dictionary that uses two lists for keys and values
    /// </summary>
    /// <typeparam name="TKey">
    /// The key type
    /// </typeparam>
    /// <typeparam name="TValue">
    /// The value type
    /// </typeparam>
    [System.Serializable]
    public class SerializableDictionary<TKey, TValue>
    {
        [SerializeField] private List<TKey> keys = new List<TKey>();
        [SerializeField] private List<TValue> values = new List<TValue>();

        /// <summary>
        /// The amount of values stored
        /// </summary>
        public int Count { get { return keys.Count; }}

        /// <summary>
        /// Gets a value at the index of key
        /// </summary>
        /// <param name="key">
        /// The key used to index into values 
        /// </param>
        /// <returns>
        /// A value at the index of key
        /// </returns>
        public TValue Get(TKey key)
        {
            int index = keys.IndexOf(key);
            if (index < 0)
                return default(TValue);

            return values[index];
        }

        /// <summary>
        /// Sets a value at a key and creates a new entry if not found
        /// </summary>
        /// <param name="key">
        /// The key to overwrite or create
        /// </param>
        /// <param name="value">
        /// The value to set at key
        /// </param>
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

        /// <summary>
        /// Removes a value at the index of key
        /// </summary>
        /// <param name="key">
        /// The key used to index into values
        /// </param>
        public void Remove(TKey key)
        {
            int index = keys.IndexOf(key);
            if (index < 0)
                return;

            keys.RemoveAt(index);
            values.RemoveAt(index);
        }

        /// <summary>
        /// Gets if a value has been set at a key
        /// </summary>
        /// <param name="key">
        /// The key that will be checked
        /// </param>
        /// <returns>
        /// If the key has been set
        /// </returns>
        public bool ContainsKey(TKey key)
        {
            return keys.Contains(key);
        }
    }
}
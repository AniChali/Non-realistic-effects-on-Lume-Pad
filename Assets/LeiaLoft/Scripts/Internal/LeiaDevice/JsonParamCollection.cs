using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace LeiaLoft
{
    /// <summary>
    /// Class for storing an arbitrarily large collection of string-collection pairs.
    ///
    /// User can store any data in this object in memory, but only types
    ///     string
    ///     int
    ///     float
    ///     bool
    /// will be serialized.
    ///
    /// This facilitates storing
    ///     sparse user-modified parameters
    /// instead of
    ///     all fields of a class with fixed properties.
    ///
    /// This class
    ///     is a Dictionary of string-Array pairs (which would normally not be serializable using JsonUtility).
    ///     automatically populates its fields with easily serialized string-Array pairs when ToString() is called.
    /// </summary>
    [Serializable]
    public class JsonParamCollection : Dictionary<string, Array>, ISerializationCallbackReceiver
    {

#region private_classes
        /// <summary>
        /// A tCollection holds a named collection of data.
        /// Get/set accessors encapsulate the object's name  and data, while its serialized fields actually get written.
        /// </summary>
        /// <typeparam name="T">Type of data to hold</typeparam>
        private class tCollection<T>
        {
            [SerializeField] private string Name;
            public string name { get { return this.Name; } set { Name = value; } }
            [SerializeField] private T[] Collection;
            // Array is not directly serializable, so we have to use inflatedType: tCollection<T> :: T[]
            public T[] collection { get { return Collection; } set { Collection = value; } }
        };

        // serializable inflatedTypes which can each hold one string-Array pair
        [Serializable] private class strCollection : tCollection<string> { };
        [Serializable] private class intCollection : tCollection<int> { };
        [Serializable] private class boolCollection : tCollection<bool> { };
        [Serializable] private class floatCollection : tCollection<float> { };
#endregion

        // these List<tCollection>s are serializable. This data gets written out as name-collection pairs
        [SerializeField] private List<strCollection> stringParams = new List<strCollection>();
        [SerializeField] private List<intCollection> intParams = new List<intCollection>();
        [SerializeField] private List<boolCollection> boolParams = new List<boolCollection>();
        [SerializeField] private List<floatCollection> floatParams = new List<floatCollection>();

        /// <summary>
        /// Gets first element of a collection that had been stored in this structure.
        ///
        /// Useful if user wants a single element, not an array.
        /// </summary>
        /// <typeparam name="T">Type of elements of collection</typeparam>
        /// <param name="name">Name of collection or parameter</param>
        /// <returns>First element of type T</returns>
        public T GetSingle<T>(string name)
        {
            if (!ContainsKey(name) || this[name] == null || this[name].Length <= 0)
            {
                LogUtil.Log(LogLevel.Error, "User tried to GetSingle<{0}> with Key {1} but that key is not present", typeof(T), name);
                return default(T);
            }
            if (this[name] == null || this[name].Length <= 0)
            {
                LogUtil.Log(LogLevel.Error, "User tried to GetSingle<{0}> with Key {1} but matching Value is empty", typeof(T), name);
                return default(T);
            }

            TypeCode prev, next;
            if (IsArrayTypeMismatch<T>(name, out prev, out next))
            {
                LogUtil.Log(LogLevel.Error, "JsonParamCollection :: GetSingle<{0}> mismatched with previous value {1}", next, prev);
                return default(T);
            }

            return ((T[])this[name])[0];
        }

        /// <summary>
        /// Sets a single value in a collection in this structure. Unspecified indices are set to default(T).
        /// </summary>
        /// <typeparam name="T">Type of array to set</typeparam>
        /// <param name="name">Parameter name</param>
        /// <param name="value">Single parameter value</param>
        /// <param name="index">Which index of collection to set</param>
        /// <param name="maxCollectionLength">Collection length to generate</param>
        public void SetSingle<T>(string name, T value, int index, int maxCollectionLength) {
            if (typeof(T).IsArray)
            {
                LogUtil.Log(LogLevel.Error, "Got array {0}. Arrays cannot be assigned using SetSingle", typeof(T));
            }

            TypeCode prev, next;
            if (IsArrayTypeMismatch<T>(name, out prev, out next))
            {
                LogUtil.Log(LogLevel.Error, "JsonParamCollection :: SetSingle<{0}> mismatched with previous value {1}", next, prev);
            }
            else
            {
                if (!ContainsKey(name))
                {
                    this[name] = new T[maxCollectionLength];
                }
                if (index > this[name].Length)
                {
                    LogUtil.Log(LogLevel.Error, "Param {0} already has an array with length {1}", name, this[name].Length);
                }
                else
                {
                    // array exists and can receive single value
                    ((T[])this[name])[index] = value;
                }
            }
        }

        /// <summary>
        /// Sets a single parameter to a value.
        /// </summary>
        /// <typeparam name="T">Type to set</typeparam>
        /// <param name="name">Name of parameter</param>
        /// <param name="value">Value of parameter</param>

        public void SetSingle<T>(string name, T value)
        {
            if (typeof(T).IsArray)
            {
                LogUtil.Log(LogLevel.Error, "Got array {0}. Arrays cannot be assigned using SetSingle", typeof(T));
            }
            SetSingle(name, value, 0, 1);
        }

        /// <summary>
        /// Defines JsonParamCollection.ToString. Recruits JsonUtility, which automatically triggers a SerializationCallback.
        /// </summary>
        /// <returns>String data containing name-collection pairs. Ready to be written to a human-readable file</returns>
        public override string ToString()
        {
            // JsonUtility.ToJson implicitly triggers OnBeforeSerialize
            string serializedData = JsonUtility.ToJson(this, true);
            return (serializedData);
        }

        /// <summary>
        /// Checks if a mismatch exists between previously existing array's type, and newly specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="prev"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        private bool IsArrayTypeMismatch<T>(string name, out TypeCode prev, out TypeCode next)
        {
            if (ContainsKey(name))
            {
                prev = Type.GetTypeCode(this[name].GetType().GetElementType());
                next = Type.GetTypeCode(typeof(T));
                return prev != next;
            }
            else
            {
                // no key, can't have a mismatch
                next = prev = Type.GetTypeCode(typeof(T));
                return false;
            }
        }

#region serialization_callbacks

        /// <summary>
        /// Moves content from non-serializable Dictionary of string-Array pairs
        /// to Lists of serializable string-Array pairs.
        /// </summary>
        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            // clear lists in case they were previously populated by another call of ToString
            foreach (IList list in new IList[] { stringParams, intParams, boolParams, floatParams })
            {
                list.Clear();
            }

            // re-populate list content from lookup. Lists are serializable, so they get written by JsonUtility.ToJson
            foreach (KeyValuePair<string, Array> collection in this)
            {
                switch (Type.GetTypeCode(collection.Value.GetType().GetElementType()))
                {
                    default:
                        LogUtil.Log(LogLevel.Error, "Unsupported type {0}", collection.Value.GetType());
                        break;
                    case TypeCode.String:
                        stringParams.Add(new strCollection { name = collection.Key, collection = (string[])collection.Value });
                        break;
                    case TypeCode.Boolean:
                        boolParams.Add(new boolCollection { name = collection.Key, collection = (bool[])collection.Value });
                        break;
                    case TypeCode.Single:
                        floatParams.Add(new floatCollection { name = collection.Key, collection = (float[])collection.Value });
                        break;
                    case TypeCode.Int16:
                    case TypeCode.Int32:
                    case TypeCode.Int64:
                        intParams.Add(new intCollection { name = collection.Key, collection = (int[])collection.Value });
                        break;
                }
            }
        }

        /// <summary>
        /// Moves content from serialized Lists of string-Array pairs into Dictionary.
        ///
        /// Does NOT clear string-Array pairs in Dictionary if these pairs already exist. I.e. user can write serialized data
        /// from multiple files into the same JsonParamCollection, and have them combine additively. If same Key is entered twice,
        /// more recent Value is kept.
        /// </summary>
        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            foreach (strCollection namedStrData in stringParams)    { this[namedStrData.name] = namedStrData.collection; }
            foreach (intCollection namedIntData in intParams)       { this[namedIntData.name] = namedIntData.collection; }
            foreach (boolCollection namedBoolData in boolParams)    { this[namedBoolData.name] = namedBoolData.collection; }
            foreach (floatCollection namedFloatData in floatParams) { this[namedFloatData.name] = namedFloatData.collection;  }
        }

#endregion
    }
}

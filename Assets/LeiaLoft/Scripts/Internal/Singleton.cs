/****************************************************************
*
* Copyright 2019 © Leia Inc.  All rights reserved.
*
* NOTICE:  All information contained herein is, and remains
* the property of Leia Inc. and its suppliers, if any.  The
* intellectual and technical concepts contained herein are
* proprietary to Leia Inc. and its suppliers and may be covered
* by U.S. and Foreign Patents, patents in process, and are
* protected by trade secret or copyright law.  Dissemination of
* this information or reproduction of this materials strictly
* forbidden unless prior written permission is obtained from
* Leia Inc.
*
****************************************************************
*/
using UnityEngine;

namespace LeiaLoft
{
 
    [DisallowMultipleComponent]
    public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
 
        private static T _instance = null;

        public virtual string ObjectName
        {
            get {
                return typeof(T).Name;
            }
        }

        public static T Instance
        {
            get
            {
                if (_instance == null  )
                {
                    _instance = (T)FindObjectOfType<T>();

                    if (_instance == null)
                    {
                        GameObject _go = new GameObject();
                        _instance = _go.AddComponent<T>();
                        _instance.gameObject.name = _instance.ObjectName;
                    }
                    return _instance;
                }
                return _instance;
            }
        }


        public static void AssignInstance(T t) {
            _instance = t;
        }

        private void OnEnable()
        {
            T[] existing_instances = FindObjectsOfType<T>();
 
            if (_instance == null) {

                AssignInstance(existing_instances[0]);
            }

            if (existing_instances.Length > 1)
            {
                string existingInstanceGameobjectName = "";
                string existingInstanceTypeName = "";
                for (int i = 0; i < existing_instances.Length; i++)
                {
                    if (existing_instances[i] == _instance)
                    {
                        existingInstanceGameobjectName = _instance.gameObject.name;
                        existingInstanceTypeName = _instance.GetType().ToString();
                    }
                }

                for (int i = 0; i < existing_instances.Length; i++)
                {

                    if (existing_instances[i] != _instance)
                    {
                        if (Application.isPlaying)
                        {
                            Destroy(existing_instances[i]);
                        }
                        else
                        {
                            DestroyImmediate(existing_instances[i]);
                        }
                    }
                }

                this.Warning(string.Format("The Instance of singleton <{0}> already exist on scene ({1})", existingInstanceTypeName, existingInstanceGameobjectName));
            }
    
            _instance.gameObject.name = _instance.ObjectName;

            if (_instance == this)
            {
                OnEnableSingleton();
            }
        }

        protected virtual void OnEnableSingleton()
        {
            //inherited clas must override this in order to call OnEnable()
        }

        public static bool InstanceIsNull
        {
            get {
                return _instance == null;
            }
        }
 
    }
}
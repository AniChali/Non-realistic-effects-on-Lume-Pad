using UnityEngine;
using System;

namespace LeiaLoft
{
    public class ComponentInitializer<T> where T : Component
    {
        private readonly T component;

        public T Component
        {
            get
            {
                return component;
            }
        }
        
        public ComponentInitializer(GameObject gameObject) : this(gameObject, null, null)
        {
            // this ctor chains to one where user has provided a TComponent and Action callback
        }

        public ComponentInitializer(GameObject gameObject, Action<T> onComponentAdded) : this(gameObject, null, onComponentAdded)
        {
            // this ctor chains to one where user has provided a TComponent and Action callback
        }

        /// <summary>
        /// Tracks an Added Component or existing Component. Triggers a callback on initialization so user can set some params
        /// </summary>
        /// <param name="gameObject">A non-null gameObject to Add a Component to</param>
        /// <param name="existingReference">A Component to try to track. If null, the Component is added anyway. For Components which must not be duplicated on a gameObject, like MeshRenderer, it is important that you provide a reference to the existing Component on the gameObject</param>
        /// <param name="onComponentAdded">A callback to trigger on construction and addition of the Component.</param>
        public ComponentInitializer(GameObject gameObject, T existingReference, Action<T> onComponentAdded)
        {
            if (existingReference == null)
            {
                component = gameObject.AddComponent<T>();
            } else {
                component = existingReference;
            }

            // trigger callback. might be used to set properties on component
            if (onComponentAdded != null)
            {
                onComponentAdded(component);
            }
        }
    }
}

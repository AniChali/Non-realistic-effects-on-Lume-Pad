using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

namespace LeiaLoft
{
    public static class BehaviourUtils
    {
        /// <summary>
        /// Copies fields by reference from original to target. 
        /// </summary>
        /// <param name="target">A Behaviour to retrieve fields from using Reflection</param>
        /// <param name="original">A Behaviour to retrieve fields from using Reflection</param>
        /// <param name="rootCamera">A Camera reference to replace in the target</param>
        /// <param name="view">A LeiaView reference to insert into the target</param>
        public static void CopyFieldsFrom(this Behaviour target, Behaviour original, Camera rootCamera, LeiaView view)
        {
            if (target == null || original == null) { return; }

            Debug.AssertFormat(original.GetType() == target.GetType(), "Possible type mismatch - {0} vs {1}", original.GetType(), target.GetType());
            // it is conceivable that there might eventually be demand for copying a specific class into a more general glass, or general class to specific class, etc

            // could cache this data as well
            var fields = target.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            for (int j = 0; j < fields.Length; j++)
            {
                // if reference is tracking the rootCamera, set it to any Camera on the Behaviour instead
                if (fields[j].FieldType == typeof(Camera) && (fields[j].GetValue(original) as Camera) == rootCamera)
                {
                    fields[j].SetValue(target, target.GetComponent<Camera>());
                }

                // if reference is to a LeiaView and it is null, set it to the LeiaView reference
                else if (fields[j].FieldType == typeof(LeiaLoft.LeiaView))
                {
                    fields[j].SetValue(target, view);
                }

                // if reference is a CommandBuffer, set it to null to avoid some Unity PostProcessingStack issues
                else if (fields[j].FieldType != typeof(UnityEngine.Rendering.CommandBuffer))
                {
                    // else very basic case. we just want to set the reference
                    fields[j].SetValue(target, fields[j].GetValue(original));
                }
            }
        }

        /// <summary>
        /// Gets some Post-effect Behaviours on a GameObject. Post-effect behaviours are implementers of OnRenderImage/OnPostRender/OnPreRender or a Behaviour type which user has opted to manually track
        /// </summary>
        /// <param name="gameObject">a gameObject to search for Behaviours on</param>
        /// <param name="trackedTypes">Types of Behaviours to manually search for</param> 
        /// <returns>A collection of Behaviours which match the search constraints provided</returns>
        public static IEnumerable<Behaviour> GetPostBehavioursOn(GameObject gameObject, HashSet<System.Type> manuallyTrackedTypes)
        {
            if (gameObject == null)
            {
                return new List<Behaviour>();
            }

            var methodFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

            var effects = new List<Behaviour>();
            foreach (var item in gameObject.GetComponents<Behaviour>())
            {
                if (!item || item.GetType() == typeof(LeiaPostEffectsController))
                {
                    // always skip adding the LeiaPostEffectsController to LeiaViews
                    continue;
                }
                if (manuallyTrackedTypes.Contains(item.GetType()) ||
                    item.GetType().GetMethod("OnRenderImage", methodFlags) != null ||
                    item.GetType().GetMethod("OnPostRender", methodFlags) != null ||
                    item.GetType().GetMethod("OnPreRender", methodFlags) != null)
                {
                    effects.Add(item);
                }
            }

            return effects;
        }

    }
}

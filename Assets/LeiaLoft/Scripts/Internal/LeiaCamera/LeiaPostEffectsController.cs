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
using System.Collections.Generic;
using System.Reflection;
using System.Collections;
using System.Linq;

namespace LeiaLoft
{

    public sealed class LeiaPostEffectsController : MonoBehaviour
    {
#pragma warning disable 649

        private LeiaCamera _leiaCamera;
        // user inputs references to Behaviour into this collection
        [SerializeField] private readonly List<Behaviour> _manuallyAddedEffects = new List<Behaviour>();

        // this collection of Types is how we track what Behaviours were on the root LeiaCamera, and which types to search for on LeiaViews
        private readonly HashSet<System.Type> _trackedTypes = new HashSet<System.Type>();
        private readonly HashSet<Behaviour> _effects = new HashSet<Behaviour>();
        private int _lastViewCount = 0;
        private readonly string resetProjectionMatrixURL = "https://docs.leialoft.com/developer/unity-sdk/troubleshooting/issue-leialoft-unity-ppsv2-camera-projection-reset";

        [SerializeField, Tooltip("If selected, the LeiaCamera will be queried every frame for new Camera effects to propagate to LeiaViews")] private bool CheckEffectsEveryFrame;

#pragma warning restore 649

        // to propagate effects from root LeiaCamera to LeiaViews, see ForceUpdate
        public void Update()
        {
            if (CheckEffectsEveryFrame)
            {
                ForceUpdate();
            }
        }

        /// <summary>
        /// This check specifically deals with Post Processing Stack version 2 issue, where PostProcessLayer.cs 
        /// resets all camera projection matrices in OnPreCull(), breaking Leia interlacing. This is a known bug in Unity PPSv2.
        /// </summary>
        /// <returns></returns>
        IEnumerator CheckProjectionMatrixReset()
        {
            yield return new WaitForEndOfFrame();
            
            if (IsProjectionReset())
            {
                Debug.Break();
                Debug.LogError("Issue Recognized: PPSv2 Projection Matrix Reset with LeiaLoft: Opening Online Documentation..." + resetProjectionMatrixURL);
                if (resetProjectionMatrixURL.Length > 0)
                {
                    Application.OpenURL(resetProjectionMatrixURL);
                }
            }
        }
        bool IsProjectionReset()
        {
#if !UNITY_EDITOR
            return false;
#else
            if (_effects.Count < 1)
            {
                return false;
            }
            if (Mathf.Approximately(_leiaCamera.BaselineScaling, 0))
            {
                return false;
            }

            if (LeiaDisplay.Instance.ActualLightfieldMode == LeiaDisplay.LightfieldMode.Off)
            {
                return false;
            }

            for (int i = 0; i < _leiaCamera.GetViewCount(); i++)
            {
                //m02 only appears to be 0 when the projection matrix has been reset
                if (!Mathf.Approximately(_leiaCamera.GetView(i).Matrix.m02, 0))
                {
                    return false;
                }
            }

            return true;
#endif
        }

        /// <summary>
        /// Forces a re-scan of effects on LeiaCamera and each LeiaView. Propagates effects from LeiaCamera to LeiaViews.
        /// </summary>
        public void ForceUpdate()
        {
            if (_leiaCamera == null)
            {
                _leiaCamera = GetComponent<LeiaCamera>();
                if (_leiaCamera == null)
                {
                    Destroy(this);
                    return;
                }
            }

            if (LeiaDisplay.InstanceIsNull)
            {
                return;
            }

            if (IsEffectsChanged())
            {
                CopyEffectsToLeiaViews();
                StartCoroutine(CheckProjectionMatrixReset());
            }
        }

        /// <summary>
        /// Detects change in post-processing effects on root LeiaCamera. Updates _effects and _trackedTypes
        /// </summary>
        /// <returns>False if all _Effects which were previously detected on root LeiaCamera are still on LeiaCamera, true otherwise</returns>
        private bool IsEffectsChanged()
        {
            //detect if views count changed
            bool isEffectsChanged = _lastViewCount != _leiaCamera.GetViewCount();

            // detects null or disabled effects
            isEffectsChanged = isEffectsChanged || _effects.RemoveWhere((Behaviour b) =>
            {
                return b == null || !b.enabled;
            }) > 0;

            // detect changes in number of Behaviours that user wishes to track.
            // _manuallyAddedEffects is the serializable list that user can modify,
            // _trackedTypes is the hashset that we can easily query
            isEffectsChanged = isEffectsChanged || _trackedTypes.Count != _manuallyAddedEffects.Count;
            _trackedTypes.Clear();
            _trackedTypes.UnionWith(_manuallyAddedEffects.Select(x=> x.GetType()));

            // detect changes in number of known _effects
            int prevEffectsSize = _effects.Count;
            _effects.UnionWith(BehaviourUtils.GetPostBehavioursOn(gameObject, _trackedTypes));
            isEffectsChanged = isEffectsChanged || prevEffectsSize != _effects.Count;

            return isEffectsChanged;
        }

        [System.Obsolete("Deprecated in 0.6.20. Scheduled for removal in 0.6.22. RestoreEffects behavior is now redundant with ForceUpdate")]
        public void RestoreEffects()
        {
            _effects.Clear();

            if (_leiaCamera == null)
            {
                _leiaCamera = GetComponent<LeiaCamera>();
            }

            ForceUpdate();
        }

        private void CopyEffectsToLeiaViews()
        {
            for (int i = 0; i < _leiaCamera.GetViewCount(); i++)
            {
                CopyEffectsToView(_effects, _leiaCamera.GetView(i));
            }
        }

        private static void CopyEffectsToView(IEnumerable<Behaviour> effects, LeiaView view)
        {
            if (view == null || view.Object == null)
            {
                return;
            }

            foreach (var effect in effects)
            {
                view.AttachBehaviourToView(effect);
            }
        }
    }
}
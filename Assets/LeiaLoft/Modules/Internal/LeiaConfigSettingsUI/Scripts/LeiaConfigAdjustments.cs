/****************************************************************
*
* Copyright 2020 © Leia Inc.  All rights reserved.
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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LeiaLoft;
using System.Linq;

namespace LeiaLoft
{
    /// <summary>
    /// A component for controling UI and modifying an existing scene's DisplayConfig params at runtime.
    ///
    /// In OnDestroy (gameObject destruction, scene deload, component destruction, editor stop play, etc.)
    /// writes your DisplayConfig params to a local file.
    /// </summary>
    public class LeiaConfigAdjustments : MonoBehaviour
    {
        [SerializeField] private JsonParamCollection sparseUpdates = new JsonParamCollection();

#pragma warning disable 0649

        [SerializeField] private SliderInputAction[] ActXSliders;
        [SerializeField] private SliderInputAction[] ActYSliders;

        [SerializeField] private SliderInputAction gammaSlider;
        [SerializeField] private SliderInputAction betaSlider;
        [SerializeField] private SliderInputAction disparitySlider;
        [SerializeField] private SliderInputAction resScaleSlider;
        [SerializeField] private SliderInputAction offsetLandscapeSlider;

        [SerializeField]
        private GameObject interlacingPage;

        [SerializeField] private GameObject actLandPage;
        [SerializeField] private GameObject actPortPage;

        [SerializeField]
        private TextInputAction inputNumViewsX, inputNumViewsY, inputStepPerUVX, inputStepPerUVY, inputStepPerRGB, inputViewResX, inputViewResY;

        [SerializeField]
        private Text interlacingMatrixLabel;

        const string stateUpdateFilename = "DisplayConfigUpdateSlanted.json";
        private DisplayConfig config
        {
            get
            {
                return LeiaDisplay.Instance.GetDisplayConfig();
            }
        }

        private void OnApplicationFocus(bool focus)
        {
            if (!focus)
            {
                WriteUpdates(false);
            }
        }

        private void Start()
        {
            // attach a callback that changes which ACT UI is enabled
            LeiaDisplay.Instance.StateChanged += () =>
            {
                UpdateActiveActPanel();
            };

        }

        private void OnDestroy()
        {
            WriteUpdates(true);
        }

        /// <summary>
        /// Recruits StringAssetUtil to perform a file IO operation
        /// </summary>
        /// <param name="performEditorResourceReload">Whether to perform an AssetDatabase.ImportAsset call. This breaks float4x4 properties in shaders in editor</param>
        void WriteUpdates(bool performEditorResourceReload)
        {
            // If we call StringAssetUtil.WriteJsonObject... in editor, it calls AssetDatabase.ImportAsset
            // Importing a Resource like a json can cause other Shader resources with float4x4 properties to discard their previously set properties.
            // So only write json when application is shutting down
            if (sparseUpdates != null)
            {
                StringAssetUtil.WriteJsonObjectToDeviceAwareFilename(stateUpdateFilename, sparseUpdates, performEditorResourceReload);
            }
        }

        void DeconstructUI()
        {
            // json is written on Component destruction, not on UI deconstruction

            IEnumerable<Slider> sliders = new HashSet<Slider>();
            sliders = sliders.Union(ActXSliders.Select(x => x.slider));
            sliders = sliders.Union(ActYSliders.Select(x => x.slider));
            sliders = sliders.Union(new[] { gammaSlider.slider, disparitySlider.slider, betaSlider.slider, offsetLandscapeSlider.slider});

            // clear all callbacks. this 
            foreach (Slider slider in sliders)
            {
                slider.onValueChanged.RemoveAllListeners();
            }
        }

        void ConstructUI()
        {
            bool loadedStateUpdateFile = StringAssetUtil.TryGetJsonObjectFromDeviceAwareFilename<JsonParamCollection>(stateUpdateFilename, out sparseUpdates);
            if (!loadedStateUpdateFile)
            {
                sparseUpdates = new JsonParamCollection();
            }

            AttachPrimitiveSliderCallbacks();
            AttachActCoefficientsXYCallbacks();

            // hide interlacing matrix ui from users. this could disrupt general functionality
            /// <include_in_public> if (interlacingPage != null) { interlacingPage.SetActive(false); } </include_in_public>

            AttachInterlacingMatrixActions();
            AttachViewResolutionActions();
        }

        void AttachViewResolutionActions()
        {
            System.Action onBaseViewResUpdated = () =>
            {
                foreach (var input in new[] { inputViewResX, inputViewResY})
                {
                    // skip if a broken UI element
                    if (input == null || float.IsInfinity(input.value))
                    {
                        return;
                    }
                }

                // when user inputs a new view res X/Y, update the ViewRes on config then update state
                config.ViewResolution = new XyPair<int>(inputViewResX.valueAsInt, inputViewResY.valueAsInt);
                sparseUpdates["ViewResolution"] = new int[] { config.ViewResolution.x, config.ViewResolution.y };
                LeiaDisplay.Instance.UpdateLeiaState();
            };

            // attach callback to each of the user input methods for 
            foreach (var input in new[] {inputViewResX, inputViewResY})
            {
                input.SetActionOnChange(onBaseViewResUpdated);
            }
        }

        void AttachInterlacingMatrixActions()
        {
            // generate a callback to attach to UI elements
            System.Action recalculateInterlacingMatrix = () =>
            {
                // use these user-provided params: inputNumViewsX, inputNumViewsY, inputStepPerUVX, inputStepPerUVY, inputStepPerRGB
                // to calculate a new interlacing matrix
                foreach (var input in new[] { inputNumViewsX, inputNumViewsY, inputStepPerUVX, inputStepPerUVY, inputStepPerRGB })
                {
                    if (input == null || float.IsInfinity(input.value))
                    {
                        return;
                    }
                }

                // update NumViews according to user input
                config.NumViews = new XyPair<int>(inputNumViewsX.valueAsInt, inputNumViewsY.valueAsInt);
                sparseUpdates["NumViews"] = new[] { config.NumViews[0], config.NumViews[1] };

                // override every single possible matrix. if users want to rotate their device, they have to recalculate. This avoids ambiguity about which matrix to write in which orientation
                float[] imatFromParams = new LeiaSharedInterlaceCalculationsWrapper().GetInterlaceMatrix(config.UserPanelResolution[0], config.UserPanelResolution[1], config.NumViews[0],
                    inputStepPerUVX.valueAsInt, inputStepPerUVY.valueAsInt, inputStepPerRGB.valueAsInt);

                foreach (string propertyName in new[] {"InterlacingMatrix", "InterlacingMatrixLandscape", "InterlacingMatrixLandscape180", "InterlacingMatrixPortrait", "InterlacingMatrixPortrait180" })
                {
                    config.SetPropertyByReflection(propertyName, imatFromParams, DisplayConfigModifyPermission.Level.DeveloperTuned);
                    sparseUpdates[propertyName] = imatFromParams;
                }
                
                string logger = string.Format("User-generated interlacing matrix\n{0}", imatFromParams.ToMatrix4x4());
                if (interlacingMatrixLabel != null)
                {
                    interlacingMatrixLabel.text = logger;
                }
                LogUtil.Log(LogLevel.Warning, logger);

                // force all data to be written, just to eliminate variability. we will reload state anyway in next step
                WriteUpdates(true);

                // have to force state update to avoid an issue where shader keyword would stick to previous view count
                LeiaDisplay.Instance.ForceLeiaStateUpdate();
            };

            // attach the callback action to UI elements
            foreach (var input in new[] { inputNumViewsX, inputNumViewsY, inputStepPerUVX, inputStepPerUVY, inputStepPerRGB })
            {
                input.SetActionOnChange(recalculateInterlacingMatrix);
            }
        }

        void AttachPrimitiveSliderCallbacks()
        {
            foreach (SliderInputAction slider in new [] {gammaSlider, betaSlider, disparitySlider, resScaleSlider, offsetLandscapeSlider})
            {
                // trigger OnDisable, OnEnable so that our text formatting callback is attached
                slider.enabled = false;
                slider.enabled = true;
            }

            // after we set each value (which triggers text formatting) we also want to attach the sparseUpdates write callback. This allows us to trigger text updates for each slider
            // but avoid writing default data into sparseUpdates when we open UI

            gammaSlider.value = config.Gamma;
            gammaSlider.SetActionOnChange((val) =>
            {
                sparseUpdates.SetSingle("Gamma", val);
                config.Gamma = val;
                LeiaDisplay.Instance.UpdateLeiaState();
            });

            betaSlider.value = config.Beta;
            betaSlider.SetActionOnChange((val) =>
            {
                sparseUpdates.SetSingle("Beta", val);
                config.Beta = val;
                LeiaDisplay.Instance.UpdateLeiaState();
            });

            // this will eventually be disparity in landscape
            disparitySlider.value = config.SystemDisparityPixels;
            disparitySlider.SetActionOnChange((val) =>
            {
                sparseUpdates.SetSingle("SystemDisparityPixels", val);
                config.SystemDisparityPixels = val;
                LeiaDisplay.Instance.UpdateLeiaState();
            });

            resScaleSlider.value = config.ResolutionScale;
            resScaleSlider.SetActionOnChange((val) =>
            {
                sparseUpdates.SetSingle("ResolutionScale", val);
                config.ResolutionScale = val;
                LeiaDisplay.Instance.UpdateLeiaState();
            });

            // this slider is prone to receiving "0" as its default value, which doesn't trigger a UI update if slider value is already 0. so ensure slider's default value is -1
            offsetLandscapeSlider.value = (int)config.AlignmentOffset.x;
            // this may eventually require permitting user to set portrait orientation offset as well
            offsetLandscapeSlider.SetActionOnChange((val) =>
            {
                sparseUpdates["AlignmentOffset"] = config.AlignmentOffset.ToArray();
                config.AlignmentOffset.x = (int)val;
                LeiaDisplay.Instance.UpdateLeiaState();
            });
        }

        void AttachActCoefficientsXYCallbacks()
        {
            foreach (int xy in new [] { 0,1})
            {
                // iterate through ActXSliders and ActYSliders. use xy to track which array we are working with
                SliderInputAction[] actSliders = new[] { ActXSliders, ActYSliders }[xy];

                // iterate through sliders in our array
                for (int i = 0; i < actSliders.Length; ++i)
                {
                    // set ith slider active if ACTX[i] is defined
                    actSliders[i].gameObject.SetActive(i < config.ActCoefficients[xy].Count);

                    // capture loop vars so that our callbacks work as desired
                    int callxy = xy;
                    int index = i;
                    string propertyKey = (callxy == 0 ? "ActCoefficientsX" : "ActCoefficientsY");

                    // trigger OnDisable, OnEnable so that our text formatting callback is attached
                    actSliders[i].enabled = false;
                    actSliders[i].enabled = true;

                    // set value if index into collection exists. this also triggers text formatting
                    if (i < config.ActCoefficients[xy].Count)
                    {
                        // when slider value is set equal to its current value, it doesn't trigger onValueChanged, so text doesn't update
                        // try setting to min, then max, then desired value, to try to avoid this issue
                        // issue still occurs when min = max but this is less important
                        actSliders[i].value = actSliders[i].slider.minValue;
                        actSliders[i].value = actSliders[i].slider.maxValue;

                        // update value
                        actSliders[i].value = config.ActCoefficients[xy][i];
                    }

                    // afterward, attach callbacks that update sparseUpdates ActCoefficients[XY] as any ACT slider changes
                    actSliders[i].SetActionOnChange((val) =>
                    {
                        config.ActCoefficients[callxy][index] = val;

                        LeiaDisplay.Instance.UpdateLeiaState();

                        sparseUpdates[propertyKey] = actSliders
                            .Select(elem => elem.value)
                            .Take(config.ActCoefficients[callxy].Count)
                            .ToArray();
                    });
                }
            }
        }

        void UpdateActiveActPanel()
        {
            // set actLandPage page active if config says it is in Landscape
            // set actPortPage active if config says it is in Portrait
            // when user rotates display, it should trigger StateChanged, which should trigger new ConstructUI stack
            actLandPage.SetActive(config.UserOrientationIsLandscape);
            actPortPage.SetActive(!config.UserOrientationIsLandscape);
        }

        void OnEnable()
        {
            ConstructUI();
            UpdateActiveActPanel();
        }

        private void OnDisable()
        {
            DeconstructUI();
        }

        public void ResetValues()
        {
            sparseUpdates.Clear();

            // trigger a write of a blank SparseUpdates file
            StringAssetUtil.WriteJsonObjectToDeviceAwareFilename(stateUpdateFilename, sparseUpdates, true);

            this.DeconstructUI();

            // reload DisplayConfig from firmware up
            LeiaDisplay.Instance.LeiaDevice.GetDisplayConfig(true);
            LeiaDisplay.Instance.UpdateDevice();

            // reload UI, rebind callbacks
            this.ConstructUI();
            UpdateActiveActPanel();
        }

    }
}

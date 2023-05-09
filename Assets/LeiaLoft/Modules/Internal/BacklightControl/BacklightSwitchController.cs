using UnityEngine;
using System.Collections;

namespace LeiaLoft
{
    /// <summary>
    /// Leia backlight switch MonoBehaviour. Performs no action if Unity Editor version earlier than 2018.1.
    /// Preferentially uses tasks in 2019.1+.
    /// Also supports opening execution log through keypress on Windows. Backlight switching and need for
    /// error logs tend to go hand-in-hand.
    /// </summary>
    public class BacklightSwitchController : UnityEngine.MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField] private KeyCode Key2D = KeyCode.F2;
        [SerializeField] private KeyCode Key3D = KeyCode.F3;
#pragma warning restore 0649

        void Update()
        {
            if (Input.GetKeyDown(Key2D))
            {
                if (!LeiaDisplay.InstanceIsNull)
                {
                    LeiaDisplay.Instance.DesiredLightfieldMode = LeiaDisplay.LightfieldMode.Off;
                }
            }
            if (Input.GetKeyDown(Key3D))
            {
                if (!LeiaDisplay.InstanceIsNull)
                {
                    LeiaDisplay.Instance.DesiredLightfieldMode = LeiaDisplay.LightfieldMode.On;
                }
            }
        }

        // this value is set in Start(). it tracks the current 2D/3D light balance of the display
        private float _mLightBalance = 0.0f;

        // this value will continuously set the display light balance
        Coroutine crLightSetter;

        // this will be triggered when a transition was requested, then backlight was set to 2D/3D before transition could complete. It will help user to restore state
        System.Action onBacklightInterrupt;

        /// <summary>
        /// Allows users to request a backlight transition over time. This method triggers a coroutine which runs very frequently in order to avoid precision issues.
        ///
        /// This transition can be interrupted by setting backlight.
        /// </summary>
        /// <param name="startingBalance">The 2D/3D light ratio between 0 and 1 that we wish to start our transition at</param>
        /// <param name="targetBalance">The 2D/3D light ratio between 0 and 1 that we wish to end our transition at</param>
        /// <param name="msec">The time over which we wish to transition from current light ratio, to the target light ratio</param>
        /// <param name="onFrame">An optional event to trigger as the transition occurs. Argument will span from 0 (start of transition) to 1 (transition has ended)</param>
        /// <param name="onEnd">An optional event to trigger on end of transition</param>
        /// <param name="onTransitionInterrupted">An optional event to trigger when backlight is set to 2D/3D before transition would have been completed</param>
        public void RequestLightBalanceBy(float startingBalance, float targetBalance, int msec, System.Action<float> onFrame, System.Action onEnd, System.Action onTransitionInterrupted) 
        {
            if (crLightSetter != null)
            {
                // if the user provided an interrupt event, trigger it
                if (onBacklightInterrupt != null) { onBacklightInterrupt.Invoke(); }
                StopCoroutine(crLightSetter);
            }
            crLightSetter = StartCoroutine(LerpLight(startingBalance, targetBalance, msec, onFrame, onEnd));

            // track the user-provided interrupt
            onBacklightInterrupt = onTransitionInterrupted;
        }

        public float GetCurrentLightBalance()
        {
            return _mLightBalance;
        }

        IEnumerator LerpLight(float startingBalance, float endingBalance, int msec, System.Action<float> onFrame, System.Action onEnd)
        {
            float secondsTarget = msec * 1.0f / 1000;

            // waitFrame is generated once; might need to be generated each frame, in order to be more accurate
            WaitForEndOfFrame waitFrame = new WaitForEndOfFrame();

            // set up LeiaDevice for backlight transition
            AndroidLeiaDevice ald = null;
            ILeiaDevice ld = LeiaDisplay.Instance.LeiaDevice;
            if (ld is AndroidLeiaDevice) {
                ald = (AndroidLeiaDevice)ld;
            }

            for (float f = 0.0f; f < secondsTarget; f = f + Time.deltaTime)
            {
                // completionRatio will always span 0 (begin) to 1 (end)
                float completionRatio = f / secondsTarget;

                // update tracker for light, request new light intensities, yield a frame
                _mLightBalance = Mathf.Lerp(startingBalance, endingBalance, completionRatio);
                if (ald != null) { ald.SetDisplayLightBalance(_mLightBalance); }
                // if user specified an action to occur each frame, trigger it
                if (onFrame != null) { onFrame.Invoke(completionRatio); }

                // to get to msec precision, have to use WaitForEndOfFrame and Time.deltaTime
                yield return waitFrame;
            }

            // yield one frame with a completed intensity
            _mLightBalance = endingBalance;
            if (ald != null) { ald.SetDisplayLightBalance(_mLightBalance); }
            if (onFrame != null) { onFrame.Invoke(1.0f); }
            yield return waitFrame;

            // if user specified an action to occur on end of frame, trigger it
            if (onEnd != null) { onEnd.Invoke(); }

            // if we completely finish a transition, null out the user-provided backlight interrupt event
            onBacklightInterrupt = null;
        }

        private void Start()
        {
            // attach a callback for when backlight is re-set. This callback will always trigger the user-provided backlight interrupt event
            LeiaDisplay.Instance.BacklightStateChanged += (prev, curr) =>
            {
                if (crLightSetter != null)
                {
                    // if the user provided an interrupt event, trigger it
                    if (onBacklightInterrupt != null) { onBacklightInterrupt.Invoke(); }
                    StopCoroutine(crLightSetter);
                }
                // on display backlight change, update the light balance to latest info
                _mLightBalance = (curr == LeiaDisplay.LightfieldMode.On ? 1.0f : 0.0f);

                crLightSetter = null;
            };

            // on Start, update this script's tracking of _mLightBalance
            _mLightBalance = 0.0f;
            if (!LeiaDisplay.InstanceIsNull && LeiaDisplay.Instance.DesiredLightfieldMode == LeiaDisplay.LightfieldMode.On)
            {
                _mLightBalance = 1.0f;
            }
        }

        /// <summary>
        /// Application developer access to backlight API.
        /// </summary>
        /// <param name="mode">2D: backlight off, 3D: backlight on</param>
        public static void ApplicationRequestBacklight(string mode)
        {
            if (string.IsNullOrEmpty(mode))
            {
                Debug.LogWarningFormat("ApplicationRequestBacklight has empty param");
                return;
            }

            switch (mode.ToLower())
            {
                case "2d":
                    if (!LeiaDisplay.InstanceIsNull)
                    {
                        LeiaDisplay.Instance.DesiredLightfieldMode = LeiaDisplay.LightfieldMode.Off;
                    }
                    break;
                case "3d":
                    if (!LeiaDisplay.InstanceIsNull)
                    {
                        LeiaDisplay.Instance.DesiredLightfieldMode = LeiaDisplay.LightfieldMode.On;
                    }
                    break;
                default:
                    LogUtil.Log(LogLevel.Warning, "ApplicationRequestBacklight mode not recognized: {0}", mode);
                    break;
            }
        }

    }
}

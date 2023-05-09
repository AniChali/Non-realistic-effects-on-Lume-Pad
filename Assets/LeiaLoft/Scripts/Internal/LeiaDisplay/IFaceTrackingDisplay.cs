using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LeiaLoft
{
    public partial class LeiaDisplay : IFaceTrackingDisplay
    {
        // need to retrieve as param eventually
        private const int _viewPeelBinCount = 40;

        #region IFaceTrackingDisplay

        /// <summary>
        /// <para>
        /// Defines whether view peeling is enabled. If enabled, view order shifts as FOV shifts. This supports usage where
        /// a dev wants to track viewer position and avoid showing "view jumps" (7 -> 0, 3 -> 0) as the user moves horizontally
        /// through many views.
        /// </para>
        ///
        /// <para>
        /// Default false, in which case FOV shifts camera position but does not shift view order.
        /// </para>
        /// </summary>
        public bool ViewPeelEnabled
        {
            get
            {
                return Decorators.ViewPeelEnabled;
            }
            set
            {
                LeiaStateDecorators decos = Decorators;
                decos.ViewPeelEnabled = value;
                Decorators = decos;
            }
        }

        /// <summary>
        /// Gets a string which represents series of views that user sees.
        /// </summary>
        /// <returns>Sequence of views, separated by | characters. Null in 2D mode</returns>
        public string GetViewBinPattern()
        {
            return this._leiaState.GetViewBinPattern();
        }

        /// <summary>
        /// Gets the recommended view peel bin count.
        /// </summary>
        public int ViewPeelBinCount { get { return _viewPeelBinCount; } }

        /// <summary>
        /// <para>
        /// IFaceTrackingDisplay.ViewPeelEnabled = true must be set to achieve both view peeling and FOV shift.
        /// </para>
        /// 
        /// A shorthand for ViewPeelForPosition(userSpecifiedX, previousValue).
        ///
        /// This facilitates control using a UnityEngine.UI.Slider which dynamically controls the LeiaDisplay.ViewPeelForXPosition parameter.
        /// </summary>
        public float ViewPeelForXPosition
        {
            get
            {
                return ViewPeelForPosition.x;
            }
            set
            {
                ViewPeelForPosition = new Vector2(value, ViewPeelForPosition.y);
            }
        }

        /// <summary>
        /// <para>
        /// IFaceTrackingDisplay.ViewPeelEnabled = true must be set to achieve both view peeling and FOV shift.
        /// </para>
        /// 
        /// A shorthand for ViewPeelForPosition(previousValue, userSpecifiedY).
        ///
        /// This facilitates control using a UnityEngine.UI.Slider which dynamically controls the LeiaDisplay.ViewPeelForYPosition parameter.
        /// </summary>
        public float ViewPeelForYPosition
        {
            get
            {
                return ViewPeelForPosition.y;
            }
            set
            {
                ViewPeelForPosition = new Vector2(ViewPeelForPosition.x, value);
            }
        }

        /// <summary>
        /// <para>
        /// IFaceTrackingDisplay.ViewPeelEnabled = true must be set to achieve both view peeling and FOV shift.
        /// </para>
        /// 
        /// Specifies the LeiaDisplay view peel position to show views from. View peeling is intended to
        /// <para>    - shift camera frustum so that as the viewer moves in the world, their view of Unity content moves with them</para>
        /// <para>    - carousel through views so that viewer's center views of the display are always middle views of rendered content</para>
        ///
        /// <para>LeiaDisplay's AdaptFOV is recruited to perform view peeling.</para>
        /// </summary>
        public Vector2 ViewPeelForPosition
        {
            get
            {
                // supports retrieval of current view peel effect.
                // currently we just retrieve AdaptFOV
                return Decorators.AdaptFOV;
            }
            set
            {
                LeiaStateDecorators decos = this.Decorators;
                decos.AdaptFOV = value;
                this.Decorators = decos;
            }
        }

        /// <summary>
        /// Pair of integers inside of a Vector2 (which stores floats). Round DisplaySizeInMm.x and DisplaySizeInMm.y to nearest int
        /// </summary>
        public Vector2 DisplaySizeInMm
        {
            get
            {
                XyPair<int> size = this.GetDisplayConfig().DisplaySizeInMm;
                return new Vector2(size.x, size.y);
            }
        }

        #endregion IFaceTrackingDisplay

    }
}

interface IFaceTrackingDisplay
{
    bool ViewPeelEnabled { get; set; }
    string GetViewBinPattern();
    int ViewPeelBinCount { get; }
    float ViewPeelForXPosition { get; set; }
    float ViewPeelForYPosition { get; set; }
    Vector2 ViewPeelForPosition{ get; set; }
    Vector2 DisplaySizeInMm { get; }
}

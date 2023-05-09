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
namespace LeiaLoft
{
    [System.Obsolete("Deprecated in 0.6.18. Use Slanted. Scheduled for removal in 0.6.20.")]
    public class SquareLeiaStateBuilder : AbstractLeiaStateBuilder
    {
		public SquareLeiaStateBuilder(DisplayConfig displayConfig) : base(displayConfig)
        {
        }

        protected override AbstractLeiaStateTemplate CreateLeiaState()
        {
			var state = new SquareLeiaStateTemplate(_displayConfig);
            state.SetBacklightMode(BacklightModeConstants._3D);
            return state;
        }
    }
}

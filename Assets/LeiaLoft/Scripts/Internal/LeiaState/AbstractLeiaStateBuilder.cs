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
    public abstract class AbstractLeiaStateBuilder
    {
		public static class BacklightModeConstants
		{
			public static int _3D { get { return 3; } }
			public static int _2D { get { return 2; } }
		}

		protected DisplayConfig _displayConfig;

		public AbstractLeiaStateBuilder(DisplayConfig displayConfig)
        {
			_displayConfig = displayConfig;
        }

        protected abstract AbstractLeiaStateTemplate CreateLeiaState();

        public virtual AbstractLeiaStateTemplate MakeBasic()
        {
            var leiaState = CreateLeiaState();
			leiaState.SetViewCount(_displayConfig.UserNumViews.x, _displayConfig.UserNumViews.y);
            return leiaState;
        }

        public virtual AbstractLeiaStateTemplate MakeHPO()
        {
            var leiaState = CreateLeiaState();
			leiaState.SetViewCount(_displayConfig.UserNumViews.x, 1);

            // These calls to leiaState.SetViewCount will typically be rendered zero-impact.
            // calls to AbstractLeiaStateBuilder :: MakeHPO -> leiaState.SetViewCount will be followed by
            // LeiaDisplay -> UpdateLeiaState -> ILeiaState.UpdateState -> ILeiaState.SetViewCount.
            // only the last SetViewCount call will have impact.

            // This call to leiaState.SetViewCount is preseved only for continuity. Revisit after n x m hp/vp interlacing
            // 2021_04_05

            return leiaState;
        }

        public virtual AbstractLeiaStateTemplate MakeTVO()
        {
            var leiaState = CreateLeiaState();
            leiaState.SetViewCount(2, 1);
            return leiaState;
        }
    }
}
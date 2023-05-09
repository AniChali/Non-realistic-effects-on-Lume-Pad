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
using System.Collections.Generic;
using UnityEngine;

namespace LeiaLoft
{
    /// <summary>
    /// A LeiaState is abstraction for LeiaDisplay rendering logic that includes
    /// camera properties management, framebuffer creation and rendering.
    /// <see cref="LeiaStateFactory"/>
    /// </summary>
    public interface ILeiaState
    {
        void GetFrameBufferSize(out int width, out int height);
        void GetTileSize(out int width, out int height);
        void DrawImage(LeiaCamera camera, LeiaStateDecorators decorators);
		void UpdateState(LeiaStateDecorators decorators, ILeiaDevice device);
        void UpdateViews(LeiaCamera array);
        void Release();
        int GetBacklightMode();
        int GetViewsCount();
        string GetViewBinPattern();
    }
}

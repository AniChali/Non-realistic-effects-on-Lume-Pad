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
    /// <summary>
    /// Specifies physical orientation of a device related to a viewer
    /// </summary>
    public enum ParallaxOrientation
    {
        Portrait,
        Landscape,
        InvPortrait,
        InvLandscape
    }

    public static class ParallaxOrientationExtensions
    {
        public static bool IsInv(this ParallaxOrientation orientation)
        {
            return orientation == ParallaxOrientation.InvPortrait || orientation == ParallaxOrientation.InvLandscape;
        }

        public static bool IsLandscape(this ParallaxOrientation orientation)
        {
            return orientation == ParallaxOrientation.Landscape || orientation == ParallaxOrientation.InvLandscape;
        }
    }
}
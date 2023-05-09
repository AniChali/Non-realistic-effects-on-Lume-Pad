<title>Immersive Wallpaper</title>

Updated 2021_07_07

Overview

<dd>ImmersiveMode is a feature on Lume Pad devices with updated firmware (1.2.7+) where the device's backlight can be set to "immersive mode."</dd>

In this mode, most content will appear 2D but periodic background content will simultaneously appear 3D.

Demo and usage

A demo scene, LeiaLoft/Modules/ImmersiveWallpaper/Example/FullscreenImmersiveBackground.unity, is provided for your reference. Build this
to your Lume Pad with firmware 1.2.7+.

Generally to use ImmersiveMode features you will want to
- remove any LeiaCamera or LeiaDisplay objects from your scene. These interfere with immersive mode and simutaneous 2D / periodic 3D rendering
- use LeiaLoft/Modules/ImmersiveWallpaper/Scripts/SetLumePadBacklightTo.cs to set backlight to ImmersiveMode
- use LeiaLoft/Modules/ImmersiveWallpaper/Shaders/FullscreenPeriodicContent.shader to draw periodic backgrounds which will appear 3D

Platform constraints

ImmersiveMode is only supported on Unity apps built to Lume Pad devices with updated firmware (1.2.7+).
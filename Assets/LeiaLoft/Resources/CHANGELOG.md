<b>RELEASE NOTES</b>
=============

Product: Leia Unity Plugin

Version: 0.6.22
   Date: 25 June 2022

<b>OVERVIEW</b>
========

This package provides a LeiaCamera component that should be added
to existing Unity camera. LeiaDisplay component also appears when
LeiaCamera is added. The LeiaDisplay contains settings that define
render mode in general, while LeiaCamera controls specific view
parameters.

<b>KNOWN ISSUES</b>
==========

Many known issues with Unity are specific to a non-LTS version of Unity. We recommend only building from supported LTS versions of Unity.
    Please see the "UNITY VERSION SUPPORT" section for more information on supported LTS versions.

[Detected in Unity 2017.4.4]
2021-11-29
	LeiaRemote does not work with Unity 2017.4.4, and will throw an error "InvalidOperationException: File name has not been set"
	Recommended fix is to upgrade to a 2018 or greater Unity LTS version.

[Detected in: Unity 2017.4.40 - Unity 2018.4.36. Not detected in: Unity 2019.1.6]
2021-11-02
    LeiaDepthFocus may not function correctly in Unity Editor Versions before 2019 if the build target is set to Android. It may still work in Android builds though.
	
2021-11-01
	In orthographic mode, the LeiaView cameras frustrum gizmo are offseted due to a Unity bug displaying gizmos for custom projection matrices.
	This bug appears only in Scene View panel and does not reflect the actual projection matrix.
    
2021-07-02
    AndroidLeiaDevice SetDisplayLightBalance / SetBacklightTransition has a small chance of causing device to switch to 2D mode on app re-open.

2021-04-16
    Support for Parallax Auto Rotation (landscape-only apps can still show 3D in portrait orientation) has been dropped.

[Detected in: Unity 2017.04.40, not detected in: 2018.01.01f1]
2021-02-26
    Using ScreenSpaceCameraQuad.cs to render quad UI on a Screenspace Canvas for usage with LeiaRemote can create blank game view when both the game window
    and scene window are open in editor. To avoid this issue either disable the script and only run LeiaRemote in interlaced mode, or ensure the scene window is not open
    while in play mode.

[Detected in: Unity 2018.04.14 - Unity 2018.04.24. Not detected in: Unity 2017.04.34, Unity 2019.01.01f1]
2020-11-09
    Attempting to use the LeiaMediaRecorder with versions 2018.3 - 2018.4 may result in recorded pixels being vertically squashed. Workaround is to upgrade to
        Unity 2019.x.

[Detected in: Unity 2019.2.01, 2019.2.05. Not detected in Unity 2019.02.14, Unity 2019.04.06.]
2020-11-05
    Builds from Unity 2019.2 to Android may switch between LightfieldMode On and LightfieldMode Off multiple times as the Android status bar is dismissed.
    If the device switches between LightfieldMode On and LightfieldMode Off multiple times, the device's backlight may remain on after the app closes.

[Detected in: Unity 2018.03.14f1. Not detected in Unity 2018.04.06f1.]
2020-09-24
    Builds from Unity 2018.3 to Android which call Applicaiton.Quit will not engage the Leia device's backlight when the app is re-opened.

[Detected in: Unity 2019.03.*. Not detected in Unity 2018.02.02f1, Unity 2018.04.26f1.]
2020-09-17
    When building from Unity 2018.3.* to the Lumepad or Hydrogen, closing an app using Application.Quit then re-opening the app from the device's "Recents"
        tab may trigger an issue where artifacts like LeiaLoft firmware DisplaySDK aar 7.1 (md5 f15b1320...) fail to be reloaded. As a result the the
        device will be unable to show 3D lit content.
    Workaround: use Unity 2018.4.6+ https://unity3d.com/unity/whats-new/2018.4.6 see "Fixes"

[Detected in: Unity 2019.01.*.]
2020-08-11
    Builds to Android from Unity 2019.1.x with Vulkan graphics API and video content in the scene, may crash on scene load with an error like
    OMX-VDEC-1080P: Unsupported output color format for c2d (2141391876)
    OMX-VDEC-1080P: Setting color format failed
    Workaround is to set your Android graphics API to a target other than Vulkan, or update your Unity editor version.
    (Player settings -> Player -> Android settings -> Other settings -> (x) Auto graphics API -> move Vulkan to lower priority)

[Detected in: Unity 2019.01.06f1, Unity 2019.0201f1. Not detected in Unity 2018.04.*, Unity 2019.02.02f1]
2020-08-07
    Builds from Unity 2019.1 to Android which use the Android onscreen back button to trigger an app close will not engage the Leia
        device's backlight when the app is re-opened.
    This is due to a known bug in Unity which was resolved in Unity 2019.2.2.
    Workaround is to build with Unity 2018.4- or 2019.2.2+.

2020-07-30
    If user generates tuned content for an Android build / installed Android APK using the LeiaConfigAdjustmentsUI, then rebuilds or
        reinstalls their APK, their tuning file from previous tuning will affect current output. Users are recommended to tune
        content in the UnityEditor, rather than in an installed APK.

<b>WHAT'S NEW</b>
==========
0.6.22

2022-06-25
    Added convergency distance modification by handles transform.
    Leia media recorder unity version recommendation.

2022-06-22
    LeiaDisplay support for edit mode

2022-01-03
    Modified LeiaConfigSettingsUI prefab to only show ACT Landscape or ACT Portrait depending upon device orientation.

2021-12-27
    We can copy data again using more robust process. The other one broke with PPSV2 due to PPSV2 issue.
    Remove special reference storage dictionary which was allowing us to avoid special cases. Now have special cases for root Camera reference (gets LeiaView's reference), LeiaView (gets this LeiaView reference), and CommandBuffer (set to null to avoid tracking another Camera's CB)
    There are 4 Unity PPS CBs, and 3 LeiaView CBs (2 for setting view index, 1 for un-setting view index)

2021-12-23
    Resolved an issue where Leia Unity SDK and Unity's PostProcessingStack V3 together would generate many NullReferenceExceptions every frame.

2021-12-19
    Standardized serialization of LeiaMedia types (Texture, Video, VideoURL). Scenes using LeiaMediaViewer and LeiaMediaViewer variables will need to be re-serialized.

2021-12-01
    Resolved an issue where convergence plane would always be drawn for full screen, even when LeiaCamera's ViewportRect was smaller than full screen.

0.6.21

2021-11-08
    Resolved several deprecation issues with ScreenOrientation.LandscapeLeft and Texture2D.Resize.

2021-11-02
    Promoted Auto-focus to a Module.
    Consolidated common parameters and methods for LeiaDepthFocus, LeiaRaycastFocus, and LeiaTargetFocus into one base LeiaFocus script.
    Each script now has these common parameters:
    - Baseline Range
    - Depth Scale
    - Convergence Change Threshold
    - Baseline Change Threshold
    - Convergence Focus Speed
    - Baseline Focus Speed
    - Focus Offset
    Resolved an issue where LeiaTargetFocus would yield jittery convergence distance values at low framerates.
    Resolved an issue where updating LeiaTargetFocus samples at runtime had no effect.
    
2021-10-29
     LeiaCamera.CameraShift property now fully supported when LightfieldMode = off

2021-10-28
    Added a feature - Parallax 3D backgrounds. Users can render a scene with tuned 3D settings, then add in a separately tuned 3D background. This addresses an issue where
        content near the convergence plane and content in the background / skybox sometimes could not both be shown with the best depth that developers desired.

2021-10-22
    Resolved an issue where LeiaAboutWindow could cause UI errors when Unity was recompiling a project.
    
202-10-21
  Leia Remote: resolved an issue where Leia Remote switched to Quality Mode when entering play mode

2021-10-19
    Reduced memory allocations that caused garbage collection.
    Addded Leia Media and Immersive Wallpaper links to About window. Moved Leia Media and Immersive Wallpaper scenes from Modules to Examples parent folder.

2021-10-15
     Fixed a bug where LeiaTargetFocus allocate memory every frame.

2021-10-12
    Resolved an issue in LeiaMediaViewer where calls to SetLeiaMedia (texture, video, video URL) would receive args in order $rows, $cols. Arg order is now $cols, then $rows.
        Please update your code accordingly. This is a substantial code difference.
    Resolved an issue in LeiaMediaVideoPlayer where LeiaMediaViewer could not be added at runtime as a Component using AddComponent. Process now succeeds without crashing.
        However, you should continue to strongly prefer to drag in the prefab at LeiaLoft/Modules/LeiaMedia/Prefabs/LeiaMediaViewer.prefab, as this asset has a Renderer set up.
    Resolved an issue in LeiaMediaViewer where a destroyed LeiaMediaViewer might still receive callbacks from LeiaDisplay.StateChanged.

2021-10-07
    Resolved an issue where there was ambiguity about LeiaView anti-aliasing vs application QualitySettings.antiAliasing. Leia Unity SDK no longer suggests users  to
        set QualitySettings.antiAliasing to any particular value.

2021-10-05 - 2021-11-08
    Improved the LeiaMediaRecorder's UI. LeiaMediaRecorder can now record tiled n x m LeiaMedia content to a user-specified directory.

2021-10-05
    LeiaMediaViewer:  added MediaScaleMode option for controlling whether LeiaMediaViewer draws in game world or in pixel-perfect screen space.
        World XYZ - behave as any other object in the scene: respects transform and perspective distortion .
        OnscreenPercent - use screen coordinates with given scale and offset percentage. Pixel-perfect view guaranted for OnscreenPercent = [0,0,1,1]

2021-10-04
    Introduced a MinMaxPair data type to track some serialized min/max data. Users may have to insert updated values in some scenes which used AutoFocus or
        LeiaMediaRecorder assets.

2021-10-04
    Removed deprecated Unity Editor 4.6 and 4.7 support code.

2021-09-30
    Resolved an issue where Leia Unity SDK prompt might get data for another Leia SDK.

2021-09-30
    Resovled an issue where UnityEditor on OSX with build target Android might emit a warning like
        Tiled GPU perf. warning: RenderTexture color surface (wxh) was not cleared/discarded. See TiledGPUPerformanceWarning.ColorSurface label in Profiler for info

2021-09-28
    Resovled an issue where LeiaPostEffectsController would run code every frame, for each LeiaView, for every Behaviour on the root LeiaCamera, and for every Behaviour
        on each LeiaView. LeiaPostEffectsController now by default runs less often, and runs faster, and allows the user to manually propagae many Behaviours to child
        LeiaViews. LeiaPostEffectsController no longer detects when a Behaviour has been detached from the root LeiaCamera.

0.6.20

2021-08-25
    Removed many obsolete classes including
        - json classes which are superseded by JsonParamCollection
        - {Android, etc.}LeiaDeviceBehaviours; instead refer to {*}LeiaDevice
        - all ViewSharpening classes; instead ViewSharpening is performed using Graphics.Blit
        - several obsolete LeiaView, LeiaCameraUtil, and LeiaMediaViewer methods

2021-08-23
    Added support for URP post processing

2021-08-20
    Resolved an issue where shaders or other assets would sometimes not be found by Unity.

2021-08-12
    Added an API for BacklightSwitchController to use RequestLightBalanceBy to request a 2D/3D light balance over time.

0.6.19.1
    Resovled an issue where app would trigger conflicting SetBacklightMode and SetBacklightTransition calls especially on app re-open.

0.6.19

2021-07-06
    Added support for Unity apps to enter ImmersiveMode on the Lume Pad. When the Lume Pad's lights are in ImmersiveMode, some periodic imagery can appear 3D but 2D
        content can simultaneously appear 2D. This is especially valuable for displaying periodic imagery in 3D while you display text at full resolution.

2021-06-18
    Added the ability to calculate a convergence projection matrix in CameraCalculatedParams :: GetConvergedProjectionMatrixForPosition. This will help to manually converge
        Unity Cameras onto a convergence plane.

2021-06-17
    LeiaMedia : Automatically select best meda tile per view when media tiles do not match user's display. Fixed an ambiguous case where in 2D mode, LeiaMedia would show
        left most tile. Now in 2D, the left-leaning center tile is displayed.

2021-06-14
    Added AndroidLeiaDevice SetBacklightTransition and modified SetDisplayLightBalance to give users better control over 2D/3D light transitions. To transition backlight
        intensity, try using AndroidLeiaDevice :: SetDisplayLightBalance(1f...0f).

   Added more shader variants to LeiaLoft_Slanted_8V. This takes longer to compile, but there are more debug and performance options that are automatically selected on
2021-05-26
        devices where the more performant route can be taken.

2021-05-13
    Support setting a LeiaCamera's ViewportRect. This allows devs to crop interlaced LeiaCamera content to fill just a section of the screen. Devs must set the LeiaCamera
        "Fill technique" to a ViewportRectFill technique depending upon their Unity verison and graphics effects stack. Generally
        simple Forward rendering without camera effects renders square pixels with TruncatedRectOfFullRenderTexture, and
        complex rendering using AA, HDR, post-processing, or Deferred rendering on some versions of Unity renders square pixels with FullRectOfTruncatedRenderTexture.

2021-05-11
    Added support for an Override profile which uses no LitByLeia firmware (and will not engage device backlight or retrieve display-appropriate parameters).
    These override builds can be run on common build platforms without LitByLeia support.
    To use an Override profile:
        - create a file in <projectPath>/Assets/LeiaLoft/DisplayConfiguration_Override.json which contains json data for your Override profile
        - set Player Settings -> Other Settings -> Scripting Define Symbols to contain LEIALOFT_CONFIG_OVERRIDE
        - confirm that when you enter play mode in the editor, your Override profile is used to draw content
        - build to your device and confirm that when you play the build on your device, your Override profile is used to draw content

2021-05-07
    Hot-fixed an issue where AlignmentOffset was not applying a horizontal pixel translation. Changing AlignmentOffset.x now shifts views to the right (mod ViewCount).

0.6.18

2021-04-22
    Added several future profiles and renamed the "Lumepad" profile to "Lume Pad" for more consistent branding.

2021-04-16
    Support for Parallax Auto Rotation (where landscape-only apps could still show 3D in portrait orientation) has been temporarily dropped. Support will be re-added soon.

2021-04-09
    Streamlined AndroidLeiaDevice to use more consistent code paths to retrieve parameters on Android.

2021-04-08
    Transitioned interlacing process to distinguish between 4 views in horizontal orientation vs 4 views in vertical orientation.

2021-04-08
    In the case that an APK made with the LeiaLoft SDK is run on a non-Leia device, the APK will no longer immediately crash. Note that running an APK
        with the LeiaLoft SDK on a non-Leia device may incur a performance hit compared to an APK with no LeiaLoft SDK.

2021-03-19
    Transitioned interlacing process to use "Slanted" shader rather than "Square" shader. Content visuals should stay the same.

2021-03-18
    Added support for drawing global and per-pixel masked background textures when "interlacing" or compositing views onto our LitByLeia displays.
        Textures and pixel-masking properties can be set using LeiaDisplay.Instance.Decorators properties. These properties must be set using C# code, not UI.

2021-03-09
    Implemented LeiaCameraUtils.ScreenToWorldPoint. Made LeiaCameraUtils.ScreenPointToRay and LeiaCameraUtils.ScreenToWorldPoint into extension methods of LeiaCamera.
        Users can now call myLeiaCamera.ScreenPointToRay. Existing code which calls these static LeiaCameraUtils functions continues to work.

2021-03-04
    Added new callback to LeiaDisplay: BacklightStateChanged(LightfieldMode previousState, LightfieldMode currentState). Which provides
        users a way to subscribe to mode specific backlight changes instead of explicitly checking the values in LeiaDisplay.

2021-03-04
    Resolved an issue where previews of LeiaCamera content in portrait-mode orientation in editor would be stretched rather than displayed correctly.

0.6.17

2021-02-09
    Resolved an issue where setting multiple types of LeiaMedia video content (clip + URL) could introduce conflicts on the VideoPlayer.

2021-02-3
    Removed video files:
     - Source/Assets/LeiaLoft/Modules/LeiaMedia/Example/Videos/exampleVideo_2x2.mp4 and
     - Source/Assets/LeiaLoft/Modules/LeiaMedia/Example/Videos/recording_2x2.mp4
    From the SDK in favor of DandelionsSnipOgvVP81440_2x2.ogv

2021-01-27
    Fixed a bug with LeiaMediaViewer which was causing col and row counts to stick to their initial value rather than changing as user updated LeiaMedia.
        LeiaMedia col and row values now update as
            - user changes media in inspector (if new media has a name and is parsable), or
            - user sets values by script and supplies cols, rows as arguments, or
            - user specifies LeiaMediaViewer.ActiveLeiaMediaRows and LeiaMediaViewer.ActiveLeiaMediaCols

2021-01-26
    Resolved an issue where changing RenderTechnique (Stereo / Default) could update content without updating LeiaDisplay's inspector.

2021-01-25
    Fixed a bug with *LeiaStateTemplate where using Ctrl-L menu to reset DisplayConfig would cause refs to ViewCount variables to be dropped.

2021-01-25
    Fixed a bug with *LeiaStateTemplate where setting camera view count, would cause interlacing steps to be performed with default shader values.

2021-01-20
    Resolved an issue appearing in some versions of Unity where setting LeiaDisplay Editor Emulated Device to a landscape config might cause content to be stretched.

2021-01-12
    Resolved an issue where 4 LeiaViews sould spawn regardless of device configuration then be destroyed once firmware data was pulled

2021-1-5
    Streamlined About window UX, adding welome screen, consolidating release notes and log settings to one location. Added auto popup functionality.

2020-12-28
    When LeiaPostEffectsController copies a MonoBehaviour from a root Camera to a LeiaView Camera, it will now set any LeiaView refs, to the LeiaView of that LeiaView's Camera.
    Modified LeiaPostEffectsController to copy Leia effects to a LeiaView, not a GameObject.

2020-12-23
    Resolved an issue where the LeiaPostEffectsController would sometimes cause errors regarding calls to Application.isPlaying in constructor.

0.6.16

2020-12-10
    Added stereo render technique

2020-12-01
    Implemented DisplayConfig :: ToString. This will provide some debug data to users. Do not rely upon parsing this string; implement a class conversion operator instead.

PREVIOUSLY

0.6.15

2020-11-06
    Resolved an issue where closing the DisplayConfig tuner (Ctrl-L menu) would sometimes cause interlacing to switch to a 2D style.

2020-11-03
    Added API overloads for loading Leia Media with custom rows and columns

0.6.14

2020-11-05
    Added Resolution Scale to LeiaConfigSettingsUI.

2020-10-08
    Resolved an issue with Unity's Post-Processing Stack Version 2.3 that caused PostProcessingLayers to add redundant CommandBuffers to LeiaViews and
        unnecessarily remove CommandBuffers from the root Camera. This affected all Unity SDK versions up to 0.6.14.

    Resolved an issue where convergence plane Editor UI was always drawn for a 2560 x 1440 aspect ratio (1.7777).

2020-10-06
    Resolved an issue with calculating the LeiaDevice's portrait or landscape orientaiton and passing that info to DisplayConfig in Unity Editor versions 2017.4 - 2019.3
        would cause content to be stretched.

2020-10-06
    Resolved issue in Parallax Effect module where gyro would not disable when application is not in focus, draining battery unnecessarily.

2020-10-02
    Deprecated LeiaCamera.ClearCommandBuffers. Users should no longer expect the LeiaLoft Unity SDK to automatically clear, release, or remove any of their
        CommandBuffers. (Users will still have to reattach their CommandBuffers when LeiaViews are reconstructed.)

2020-09-29
    Added support for AndroidLeiaDevice :: SetDisplayLightBalance. Call with param in range 0 - 1. This modulates the intensity of the Android device's backlight.

2020-09-18
    Moved assets in LeiaLoft/Examples/LeiaLogo/Resources to .../LeiaLogo/LeiaLogoData. This excludes the assets from the AssetDatabase by default, which
        saves 0.4 MB in APKs which do not include the "LeiaLogo" scene in their build.

2020-09-18
    LeiaViews now take the same renderingPath as was root camera had. Previously, LeiaViews would always use project default renderingPath.

2020-09-15
    Changed labeling of LeiaDisplay :: RenderModes. RenderModes HPO and 2D are legacy terms; we now think in terms of whether the device's display is in
        LightfieldMode: On or LighfieldMode: Off.
    Users should change references to LeiaDisplay.Instance.DesiredLeiaStateID = {"HPO"/"2D"} to LeiaDisplay.Instance.DesiredLightfieldMode = LeiaDisplay.LightfieldMode.On/Off

2020-09-14
    Added a drop-down menu for selecting an emulated device in editor.

2020-09-08
    Resolved a Unity Editor 2018.3+ issue with LeiaLoft Unity SDK 0.6.13 where changing the LeiaDisplay's RenderMode would cause content to be stretched.

2020-09-04
    Resolved an issue where LeiaMediaRecorder would record views in row-swapped order (wanted 0123, previously got 2301).

2020-09-03
    Re-enabled alpha blending. This feature had been supported in SDK 0.5.0 (Jan 2019), disabled in SDK 0.6.13 (August 2020) and is now re-enabled.
        Users who previously had a LeiaDisplay :: Enable Alpha Blending setting in a scene made with LeiaLoft Unity SDK 0.5.0  will need to re-set the
        LeiaDisplay :: Enable Alpha Blending flag in each scene.

2020-09-03
    Resolved an issue where display would show views in order 0123 but LeiaMediaView would show content from 3012 in corresponding views.

2020-08-18
    Changed calling style for LeiaCameraUtils.ScreenPointToRay. Users should now supply a LeiaCamera and screen-space Vector3.

0.6.13

2020-07-30
    Added support for generating user-tuned parameters in the UnityEditor using the LeiaConfigAdjustmentsUI, which can be shipped in APKs.

2020-07-20
    Introduced an IFaceTrackingDisplay interface. LeiaDisplay fulfills this interface, allowing it to perform view peeling and field-of-view shifting.

2020-07-17
	Added stub API for eventual support for Android devices to load additional DisplayConfig info from within their virtual file system.

2020-07-01
    Resolved an issue where users could accidentally update params on the wrong DisplayConfig. Correct DisplayConfig will now be
        default DisplayConfig which is modified, users will be warned if their DisplayConfig is null, and users will be warned if they
        attempt to modify a DisplayConfig other than the default DisplayConfig.

2020-05-12
  Resolved an issue where retrieving LeiaDisplay.Instance during edit time would cause more LeiaDisplays to spawn.
        Now retrieving LeiaDisplay.Instance during edit mode returns null.

	Added EditorWindowUtil for reusable editor window functions

2020-05-01
    Adjusted color settings to accomodate ACT/view sharpening on linear color space.

2020-04-28
  Added release and debug gradle minification to Recommended Settings Window. Any setting beside None will cause backlight failure on Android.

2020-04-24
    Resolved an issue where selecting LeiaCamera might dirty scene in some versions of Unity (2019?).

2020-04-23
    Redesigned DisplayConfig saving with json to support Android.

2020-04-22
    Resolved an issue where GPU memory would continue to be reserved for nonexistent textures.

2020-04-13
    Added step to clone tags from root camera to LeiaRenderCamera. This may change behavior for users who use Camera.main.
    Enabled support for multi-LeiaCamera-editing. This support may be changed in the future.
    Added support for iterating through LeiaViews in a LeiaCamera. Note that LeiaViews may sometimes be null (e.g. on frame 0).

2020-04-13
    Resolved an issue where converting between orthographic camera and perspective camera would not change interlacing state.
    Added a method, LeiaDisplay.ForceLeiaStateUpdate(), for forcing a regeneration of interlacing state without changing backlight.
        This method resolves a specific issue with orthographic / perspective cameras and does not need to be used by external users.

2020-04-08
    Modified LeiaDisplay's StateChanged delegate to only trigger once per state change.
        Users who wrote code that reacted to changes in backlight state, or LeiaDisplay's
        StateChanged callback, should test if behavior changed for them.
    Reduced the number of calls sent to backlight service per DesiredLeiaState change.
        Previously 4+ backlight calls would occur each time the interlacing or backlight mode changed.

2020-04-07
    Added accessor variables for LeiaView's Camera, absolute ID, x position in camera grid, y position in camera grid.

2020-04-03
    Resolved an issue with state tracking when Android status bar would be pulled down.

2020-04-01
    Rolled back Display aar to 6.x.x. This resolves an issue where users who pulled down the Android status bar would see parallax content.

2020-03-31
    Updated Post Processing Stack Camera Reset url

2020-03-30
    Removed checks for ViewSharpening on LeiaDisplay; users will still be warned to remove ViewSharpening because class is deprecated.

2020-03-30
    Resolved an issue where orthographic content would be stretched in 2D mode.

2020-03-24
    Restored the ParallaxEffects module to LeiaLoft modules.

2020-03-24
    Resolved an issue where parallax auto rotation setting would not remain set.

2020-03-20
    Extended PPSv2 compatibility back from 2019.1+ to 2018.1+.

2020-03-19
    Resolved one issue where users would exit app but backlight would remain on.

2020-03-18
    Resolved a bug where users would encounter a bug when changing LeiaDisplay renderMode in edit mode.

2020-03-16
    Resolved an issue where tabbing out, then back in, would draw a black screen when trying to build to standalone targets.

2020-03-13
    Added flags for some build-breaking errors.

2020-03-13:
    Resolved a bug where backlight would not turn off, or would turn off slowly, after a scene switch.

2020-03-11
    Resolved an issue where only 4 LeiaViews would show as camera gizmos in editor scene view.

2020-02-10
    Deprecated LeiaDisplay :: AlphaBlending interlaced content.

2020-02-10
    Added PreBuildProcess macro for setting settings before build. Enabled wildcard searches in post-build macro.

2020-02-07
    Patched issues with 2D/HPO switching that appeared ~Jan 7, AKA SDK 0.6.7.
    Removed support for HDR on interlacing camera, as it was preventing blits to screen in HDRP.
    Individual LeiaViews still support HDR when root camera has HDR enabled.

2020-01-17
    Changed scripting API for setting LeiaMedia content on LeiaMediaViewer.
    When writing C# scripts, instead of assigning LeiaMedia public variables
    LeiaMediaVideoURL, leiaMediaVideoClip, leiaMediaTexture,
        set them from script using
    SetVideoURL, SetVideoClip, SetTexture

2020-01-20
    Changed LeiaView.AttachCommandBufferToView API. Users should now specify CameraEvent.

2019-12-01
    Added LeiaMediaViewer - a gameObject which can display prerendered n-view content including textures and movies.

0.5.0
Improved Leia Remote Module
Added Parallax Effect Module


<b>UNITY VERSION SUPPORT</b>
=====================
2017 LTS - 2020 LTS

<b>QUICK START GUIDE</b>
=================

1)    Import Leia Loft Unity package
2)    Add LeiaCamera script to Unity Camera

<title>	Leia Auto Focus Add-ons </title>

	Description:
  	<dd>Provides functionality for automating the LeiaCamera's convergence distance and baseline scaling.</dd>

	Quick start:
  1) Attach LeiaDepthFocus.cs to a GameObject with a LeiaCamera component.

  2) (Optional) Adjust depth scale to increase or decrease the amount of depth output.

  3) (Optional) Adjust min and max baseline to set minimum and maximum possible baseline values for the LeiaCamera.

  LeiaDepthFocus may not work in some projects with shaders that don't write to the depth buffer. 
  Two alternative auto focus methods are provided; LeiaRaycastFocus, and LeiaTargetFocus.
  To learn more about these alternatives, see the documentation here:
  https://docs.leialoft.com/developer/unity-sdk/modules/auto-focus
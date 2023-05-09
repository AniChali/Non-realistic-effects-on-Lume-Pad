<title>Parallax Background</title>

Overview:
	<dd>The ParallaxBackground.shader masks a texture into another texture with some automatic horizontal parallax.

	This is useful when you have an object near the convergence plane which you wish to keep in perfect focus, and the skybox is so far away that it is splitting into distinct objects which you would rather replace.

	This ParallaxBackground.shader feature will allow you to
		- render transarent or opaque objects or skyboxes in your scene with a weak Alpha channel
		- tune your LeiaCamera.BaselineScaling for objects that in the scene
		- entirely replace or interpolate out weak Alpha pixels in the scene, especially in the Skybox, with new background pixels with their own tunable _Baseline

	The ParallaxBackground Shader can add custom disparity or parallax to these replacement pixels through 3 _BackgroundModes.</dd>

_BackgroundModes in more depth:
	In the Leia ParallaxBackground Shader, there are options to suit various use cases for filling in background pixels:
		- In mode 0, we retrieve pixels from a "_BackgroundAlbedoTex" and apply uniform left/right translations to the pixels in order to show the texture to each LeiaView as if the Texture were off the convergence plane. In the example provided, the texture is shown at some arbitrary far distance.
		- In mode 1, we retrieve pixels from a "_BackgroundAlbedoTex" and call ParallaxBackground::RealtimeCalculatedDepth to shift the background pixels in real time. Users can extend this code to apply their own realtime depth translations to the background.
		- In mode 2, we retrieve pixels from the seamlessly tileable "_BackgroundAlbedoTex", tile the texture into the background according to _BackgroundAlbedoTilingAndOffset, automatically compute an ideal baseline for the horizontal tiling, and show the texture to each LeiaView without any discontinuities between views.

Quick start:
	Test the setup in the LeiaLoft Parallax3DBackground.unity Example scene in the Unity Editor and build on LitByLeia device. Vary the settings on MainCamera.FullscreenCameraEffect and see how they impact the onscreen output.
		OR
	In Unity Standard RenderPipeline,
		- Set up a new scene with a Camera, a LeiaCamera, some geometry in the scene (like a Unity Cube).
		- Set the Camera's Clear Flag to Solid color, and set the Background color to one with a low alpha
		- Attach a Fullscreen Camera Effect script to the Camera
			- Set the Fullscreen Camera Effect Shader to LeiaLoft/ParallaxBackground
			- Set the Fullscreen Camera Effect float param "_Mode0Baseline" to 0.4
			- Set the Fullscreen Camera Effect Texture param "_BackgroundAlbedoTex" to a seamless texture; ParallaxBackgroundAlbedo.png is provided to you as an option
			- Set the Fullscreen Camera Effect "Update shader every frame" option to "On"
		- Observe the scene on your LitByLeia device

Constraints:
	- The ParallaxBackground Shader is expected to be used with LeiaViews which have a CameraClearFlags.Color, a Camera.backgroundColor with a low alpha, a MonoBehaviour.OnRenderImage call which Blits the source texture, and scripting calls which have set the Shader's Material's _BackgroundAlbedoPixelShift and textures.
	- MonoBehaviour.OnRenderImage is not supported in Unity's Scriptable Render Pipeline, so the ParallaxBackground module will currently not work with Scriptable Render Pipeline.
	- If you choose to modify the ParallaxBackground assets, be aware that the ParallaxBackground Shader source or Fullscreen Camera Effect MonoBehaviour might change in the future.
using UnityEngine;
using UnityEngine.Video;

namespace LeiaLoft
{
	/// <summary>
	/// Implementing class will fulifll an obligation to pass information along to internal MaterialPropertyBlock (or shared Material properties).
	/// UI controls -> LeiaQuad -> MaterialPropertyBlock / Material properties -> shader
	/// </summary>
	public interface ILeiaMediaMaterialHandler
	{

		void ToggleRenderer();
		bool GetRendererActive();
		void SetRendererActive(bool status);

        void ToggleAspectRatioRegulation();
        void SetAspectRatioRegulation(bool status);
        bool GetAspectRatioRegulation();
        void ForceAspectRatio(Vector2 forced_aspect_ratio);

		void SetVideoClip(VideoClip vc);

		void SetTexture(Texture tex);
	}

}

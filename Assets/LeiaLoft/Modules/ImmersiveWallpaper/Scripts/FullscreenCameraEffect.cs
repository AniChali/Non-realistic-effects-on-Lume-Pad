using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LeiaLoft.Examples
{
	[System.Serializable]
	class NameDataPair<T>
	{
		public string Name
		{
			get
			{
				return name;
			}
		}
		public T Data
		{
			get
			{
				return data;
			}
		}

#pragma warning disable 649
		[SerializeField] private string name;
		[SerializeField] T data;
#pragma warning restore 649
	}

	[System.Serializable] class NameFloatPair : NameDataPair<float> { };
	[System.Serializable] class NameTexturePair : NameDataPair<Texture> { };
	[System.Serializable] class NameFloat2Pair : NameDataPair<Vector2> { };
	[System.Serializable] class NameFloat4Pair : NameDataPair<Vector4> { };
	[System.Serializable] class NameMatrix4x4Pair : NameDataPair<Matrix4x4> { };

	[System.Serializable]
	class ShaderParams
	{
		public List<NameTexturePair> textureParams
		{
			get
			{
				return _textureParams;
			}
		}
		public List<NameFloatPair> floatParams
		{
			get
			{
				return _floatParams;
			}
		}
		public List<NameFloat2Pair> float2Params
		{
			get
			{
				return _float2Params;
			}
		}
		public List<NameMatrix4x4Pair> matrix4x4Params
        {
			get
            {
				return _matrix4x4Params;
            }
        }

		public List<NameFloat4Pair> float4Params
        {
			get
            {
				return _float4Params;
            }
        }


#pragma warning disable 649
		[SerializeField] private List<NameFloat4Pair> _float4Params;
		[SerializeField] private List<NameFloat2Pair> _float2Params;
		[SerializeField] private List<NameFloatPair> _floatParams;
		[SerializeField] private List<NameTexturePair> _textureParams;
		[SerializeField] private List<NameMatrix4x4Pair> _matrix4x4Params;
#pragma warning restore 649
	}

	public class FullscreenCameraEffect : MonoBehaviour
	{
#pragma warning disable 649
		[SerializeField] ShaderParams shaderParams;
		[SerializeField] private Shader shader;
		[SerializeField] private bool updateShaderEveryFrame = false;
#pragma warning restore 649


		RenderTextureOperation mRTO;
		public RenderTextureOperation RTO {
			get {
				if (mRTO == null) {
					mRTO = new RenderTextureOperation(shader);
				}
				return mRTO;
			}
		}

 		private int tabFlag;

		private void OnDestroy()
		{
			if (mRTO != null) {
				mRTO.Release();
			}
		}

        private void Start()
        {
			ExportParamsToShader();
        }

        private void OnApplicationFocus(bool focus)
        {
			if (focus) { tabFlag = Mathf.Max(tabFlag, Time.frameCount); }
        }

        void ExportParamsToShader()
        {
			foreach (NameFloat2Pair pair in shaderParams.float2Params)
			{
				RTO.SetVector(pair.Name, pair.Data);
			}
			foreach (NameFloat4Pair pair in shaderParams.float4Params)
            {
				RTO.SetVector(pair.Name, pair.Data);
            }
			foreach (NameFloatPair pair in shaderParams.floatParams)
			{
				RTO.SetFloat(pair.Name, pair.Data);
			}
			foreach (NameTexturePair pair in shaderParams.textureParams)
			{
				RTO.SetTexture(pair.Name, pair.Data);
			}
			foreach (NameMatrix4x4Pair pair in shaderParams.matrix4x4Params)
            {
				RTO.SetMatrix(pair.Name, pair.Data);
            }
		}

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			// for 5 frames after tab back in, update shader properties. this resolves an issue where shader might have dropped some properties on AssetDB reload
			const int updateSpanAfterTab = 5;
			if (updateShaderEveryFrame || Mathf.Abs(Time.frameCount - tabFlag) < updateSpanAfterTab)
			{
				ExportParamsToShader();
			}

			RTO.Process(source, destination);
		}
	}
}

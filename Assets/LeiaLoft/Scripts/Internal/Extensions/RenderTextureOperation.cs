using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LeiaLoft
{

    /// <summary>
    /// has-a Material 
    /// </summary>
    public class RenderTextureOperation : IReleasable
    {

        private readonly Material mMaterial;

        public void Release() {
            if (Application.isPlaying)
            {
                Object.Destroy(mMaterial);
            }
            else
            {
                Object.DestroyImmediate(mMaterial);
            }
           
        }

        public RenderTextureOperation(Shader shader)
        {
            mMaterial = new Material(shader);
            mMaterial.hideFlags = HideFlags.HideAndDontSave;
        }

        public RenderTextureOperation(string shaderName)
        {
            mMaterial = new Material(Resources.Load<Shader>(shaderName));
            mMaterial.hideFlags = HideFlags.HideAndDontSave;
        }

        /// <summary>
        /// wrapper for Graphics.Blit 
        /// </summary>
        public void Process(RenderTexture source, RenderTexture destination)
        {
            Graphics.Blit(source, destination, mMaterial);
        }

        public void Process(Texture source, RenderTexture destination)
        {
            Graphics.Blit(source, destination, mMaterial);
        }

        public RenderTextureOperation SetTexture(string textureName, Texture2D value)
        {
            mMaterial.SetTexture(textureName, value);
            return this;
        }

        public RenderTextureOperation SetTexture(string textureName, Texture value)
        {
            mMaterial.SetTexture(textureName, value);
            return this;
        }

        public RenderTextureOperation SetFloat(string floatName, float value)
        {
            mMaterial.SetFloat(floatName, value);
            return this;
        }

        public RenderTextureOperation SetInt(string intName, int value)
        {
            mMaterial.SetInt(intName, value);
            return this;
        } 

        public RenderTextureOperation SetColor(string colorName, Color value)
        {
            mMaterial.SetColor(colorName, value);
            return this;
        }

        public RenderTextureOperation SetVector(string vectorName, Vector2 value)
        {
            mMaterial.SetVector(vectorName, value);
            return this;
        }

        public RenderTextureOperation SetVector(string vectorName, Vector3 value)
        {
            mMaterial.SetVector(vectorName, value);
            return this;
        }

        public RenderTextureOperation SetVector(string vectorName, Vector4 value)
        {
            mMaterial.SetVector(vectorName, value);
            return this;
        }

        public RenderTextureOperation SetVector(string vectorName, float x, float y)
        {
            mMaterial.SetVector(vectorName, new Vector4(x,y,0,0));
            return this;
        }

        public RenderTextureOperation SetVector(string vectorName, float x, float y, float z)
        {
            mMaterial.SetVector(vectorName, new Vector4(x, y, z, 0));
            return this;
        }

        public RenderTextureOperation SetVector(string vectorName, float x, float y, float z, float w)
        {
            mMaterial.SetVector(vectorName, new Vector4(x, y, z, w));
            return this;
        }

        public RenderTextureOperation SetMatrix(string matrixName, Matrix4x4 value)
        {
            mMaterial.SetMatrix(matrixName, value);
            return this;
        }

        public RenderTextureOperation EnableKeyword(string keyword )
        {
            mMaterial.EnableKeyword(keyword);
            return this;
        }

        public RenderTextureOperation DisableKeyword(string keyword)
        {
            mMaterial.DisableKeyword(keyword);
            return this;
        }

    }
}

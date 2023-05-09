using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LeiaLoft
{
    /// <summary>
    /// Class which acquires every frame of a camera as a RenderTexture, and saves the RT at any instant
    /// to another memory-chunk-as-render-texture.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class RenderTextureExtractor : MonoBehaviour
    {
        [SerializeField] private RenderTexture address;

        /// <summary>
        /// Sets location for RT to be saved to
        /// </summary>
        /// <param name="gpuAddress"></param>
        public void SetRenderTexture(RenderTexture gpuAddress)
        {
            address = gpuAddress;
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            // copy to natural target location; usually back buffer, eventually could be another RT
            Graphics.Blit(source, destination);
            if (address != null)
            {
                // also copy to specified address
                Graphics.Blit(source, address);
            }
        }
    }
}

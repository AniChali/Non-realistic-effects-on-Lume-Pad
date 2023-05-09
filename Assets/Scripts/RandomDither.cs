/*
File: RandomDither.cs
Author: Aneta Chalivopulosova (xchali00)
Description: script used to apply the random dithering effect
*/

using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class RandomDither : MonoBehaviour {
    public float intensity;
    private Material material;

    // create a private material used for the effect
    void Awake ()
    {
        material = new Material( Shader.Find("Hidden/RandomDither") );
    }
    
    // post-processing - apply effect
    void OnRenderImage (RenderTexture source, RenderTexture destination)
    {
        if (intensity == 0)
        {
            Graphics.Blit (source, destination);
            return;
        }

        material.SetFloat("_bwBlend", intensity);
        Graphics.Blit (source, destination, material);
    }
}
/*
File: Tresholding.cs
Author: Aneta Chalivopulosova (xchali00)
Description: script used to apply the tresholding effect
*/

using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class Tresholding : MonoBehaviour {
    public float intensity;
    private Material material;

    // create a private material used for the effect
    void Awake ()
    {
        material = new Material( Shader.Find("Hidden/Tresholding") );
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
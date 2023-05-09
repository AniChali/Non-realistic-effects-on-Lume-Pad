/*
File: OutlineFull.cs
Author: Aneta Chalivopulosova (xchali00)
Description: script used to apply the outline full effect
*/

using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class OutlineFull : MonoBehaviour {

    private Material material;

    // create a private material used for the effect
    void Awake ()
    {
        material = new Material( Shader.Find("Hidden/OutlineFull") );
    }
    
    // post-processing - apply effect
    void OnRenderImage (RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit (source, destination, material);
    }
}
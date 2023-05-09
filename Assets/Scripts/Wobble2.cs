/*
File: Wobble2.cs
Author: Aneta Chalivopulosova (xchali00)
Description: script used to apply the wobble 2 effect
*/

using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class Wobble2 : MonoBehaviour {

    private Material material;

    // create a private material used for the effect
    void Awake ()
    {
        material = new Material( Shader.Find("Hidden/Wobble2") );
    }
    
    // post-processing - apply effect
    void OnRenderImage (RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit (source, destination, material);
    }
}
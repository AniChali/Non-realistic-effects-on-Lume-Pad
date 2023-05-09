using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using LeiaLoft.Examples;

[CustomEditor(typeof(FullscreenCameraEffect))]
public class FullscreenCameraEffectEditor : Editor {

    const string userWritePathKey = "immersiveWeightWriteFolder";
    private string userWritePath
    {
        get
        {
            return PlayerPrefs.GetString(userWritePathKey);
        }
        set
        {
            PlayerPrefs.SetString(userWritePathKey, value);
        }
    }

    private FullscreenCameraEffect _effectTarget;
    private FullscreenCameraEffect FullscreenCameraEffectTarget
    {
        get
        {
            if (_effectTarget == null) { _effectTarget = (FullscreenCameraEffect)target; }
            return _effectTarget;
        }
    }

    private static string pathCat(string originalString, params string[] additionalStrings)
    {
        foreach (string additional in additionalStrings)
        {
            originalString = System.IO.Path.Combine(originalString, additional);
        }
        return originalString;
    }

    private void OnEnable()
    {
        // update once on init if necessary
        if (string.IsNullOrEmpty(userWritePath)) { userWritePath = Application.dataPath; }
    }

    private byte[] getMaterialOutputAsBytes(int width, int height)
    {
        RenderTexture prev = RenderTexture.active;

        // render a texture using the existing material
        RenderTexture temp = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32) { enableRandomWrite = true };
        FullscreenCameraEffectTarget.RTO.Process(Texture2D.blackTexture, temp);
 


        // stack on the new RT
        RenderTexture.active = temp;

        // read out data as byte[]
        Texture2D writeTexture = new Texture2D(width, height, TextureFormat.ARGB32, false);
        writeTexture.ReadPixels(new Rect(0, 0, RenderTexture.active.width, RenderTexture.active.height), 0, 0);
        byte[] bytes = writeTexture.EncodeToPNG();

        // free T2D memory
        GameObject.Destroy(writeTexture);

        // free RT memory
        temp.Release();
        GameObject.Destroy(temp);

        // pop off temporary RT
        RenderTexture.active = prev;

        return bytes;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button(string.Format("Texture write folder: {0}", userWritePath), new GUIStyle(EditorStyles.miniButton) { wordWrap =true }))
        {
            string userSelection = EditorUtility.OpenFolderPanel("Select a directory", userWritePath, "");
            if (!string.IsNullOrEmpty(userSelection))
            {
                userWritePath = userSelection;
            }
        }

        EditorGUI.BeginDisabledGroup(!Application.isPlaying);

        int width, height;
        LeiaLoft.GameViewUtils.GetGameViewWidthHeight(out width, out height);

        if (GUILayout.Button("Save material output as png"))
        {
            byte[] bytes = getMaterialOutputAsBytes(width, height);

            string dateTimeRelativelyUnique = System.DateTime.Now.ToString("MM_dd_yyyy_hh_mm_ss_fff");
            
            string path = pathCat(userWritePath, dateTimeRelativelyUnique + ".png");

            System.IO.Directory.CreateDirectory(userWritePath);
            System.IO.File.WriteAllBytes(path, bytes);
            Debug.LogFormat("Wrote immersive weight texture to\n{0}", path);
        }

        EditorGUI.EndDisabledGroup();
    }
}

/****************************************************************
*
* Copyright 2019 © Leia Inc.  All rights reserved.
*
* NOTICE:  All information contained herein is, and remains
* the property of Leia Inc. and its suppliers, if any.  The
* intellectual and technical concepts contained herein are
* proprietary to Leia Inc. and its suppliers and may be covered
* by U.S. and Foreign Patents, patents in process, and are
* protected by trade secret or copyright law.  Dissemination of
* this information or reproduction of this materials strictly
* forbidden unless prior written permission is obtained from
* Leia Inc.
*
****************************************************************
*/

using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using UnityEngine.Events;
using UnityEditor.SceneManagement;


namespace LeiaLoft.Editor
{

    public class LeiaWelcomeWindow 
    {
        private readonly string header_title = "Welcome, Creators!";
        private readonly string header_content = "Lightfield is the next generation medium that lets you experience 3D imagery with a natural sensation of depth and feel for textures, materials and lights — with no eye-wear required. It makes you feel content like never before, adding emotional connection in a digital world, making memories become more present, connections more human, and life, much richer.";

        class WelcomeUIGroup
        {
            public WelcomeUIGroup(string title, List<WelcomeUIElement> elements)
            {
                Title = title;
                Elements = elements;
            }
            public string Title { set; get; }
            public List<WelcomeUIElement> Elements { set; get; }
            public bool IsExpanded { set; get; }
            public void Display(bool HeaderHorizonatalLine)
            {
                if (HeaderHorizonatalLine)
                {
                    EditorWindowUtils.HorizontalLine();
                }
                EditorWindowUtils.BeginHorizontal();
                IsExpanded = EditorGUILayout.Foldout(IsExpanded, Title, true);
                EditorWindowUtils.EndHorizontal();

                EditorWindowUtils.HorizontalLine();
                if (IsExpanded)
                {
                    for (int i = 0; i < Elements.Count; i++)
                    {
                        Elements[i].Display();
                    }
                }
            }

        }
        class WelcomeUIElement
        {
            public WelcomeUIElement(string title, string tooltip, string decsription, string buttonLabel, UnityAction buttonAction, bool horizontalLine)
            {
                Title = title;
                Tooltip = tooltip;
                Description = decsription;
                ButtonLabel = buttonLabel;
                ButtonAction = buttonAction;
                HorizontalLine = horizontalLine;
            }
            public string Title { set; get; }
            public string Tooltip { set; get; }
            public string Description { set; get; }
            public string ButtonLabel { set; get; }
            UnityAction ButtonAction { set; get; }
            public bool HorizontalLine { set; get; }

            public void Display()
            {
                if (HorizontalLine) { EditorWindowUtils.HorizontalLine(); }
                EditorWindowUtils.BeginHorizontal();
                EditorWindowUtils.Label(Title, Tooltip, true);
                EditorWindowUtils.FlexibleSpace();
                if(!string.IsNullOrEmpty(ButtonLabel))EditorWindowUtils.Button(ButtonAction, ButtonLabel);
                EditorWindowUtils.EndHorizontal();
                EditorWindowUtils.Space(5);
                GUILayout.Label(Description, EditorStyles.wordWrappedLabel);
                EditorWindowUtils.Space(5);
            }

        }
        
        WelcomeUIGroup helpfulLinks;
        WelcomeUIGroup sampleScenes;
        WelcomeUIGroup quickStart;
        GUIStyle headlineStyle;
        const string examplesPath = "Assets/LeiaLoft/Examples/";
        const string modulesPath = "Assets/LeiaLoft/Modules/";

        public void DrawGUI()
        {
            if (helpfulLinks == null || sampleScenes == null || quickStart == null)
            {
                InitUI();
            }
            Header();
            sampleScenes.Display(true);
            helpfulLinks.Display(sampleScenes.IsExpanded);
            quickStart.Display(quickStart.IsExpanded);
        }
        void Header()
        {
            string headline = header_title;
            string body = header_content;

            EditorWindowUtils.Space(20);
            EditorGUILayout.LabelField(headline, headlineStyle, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false), GUILayout.MinHeight(20f));
            EditorWindowUtils.Space(20);
            GUILayout.Label(body, EditorStyles.wordWrappedLabel);
            EditorWindowUtils.Space(20);
        }
        void InitUI()
        {
            headlineStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 24, clipping = TextClipping.Overflow };
            if (helpfulLinks == null)
            {
                helpfulLinks = new WelcomeUIGroup("Helpful Links", new List<WelcomeUIElement>());
            }
            if (sampleScenes == null)
            {
                sampleScenes = new WelcomeUIGroup("Sample Scenes", new List<WelcomeUIElement>());
            }
            if (quickStart == null)
            {
                quickStart = new WelcomeUIGroup("Quick Start", new List<WelcomeUIElement>());
            }
            helpfulLinks.Elements.Clear();
            sampleScenes.Elements.Clear();
            quickStart.Elements.Clear();
            populateHelpfulLinks();
            populateSampleScenes();
            populateQuickStart();
        }

        private void populateQuickStart()
        {
            foreach (string pathToQuickStart in Directory.GetFiles(modulesPath, "README.md", SearchOption.AllDirectories))
            {
                string data = File.ReadAllText(pathToQuickStart);
                string quickStartContent = data.ToString();
                string htmlQuickStartTitle = null;
                string htmlQuickStartDescription = null;
                Regex reUnityQuickStart = new Regex("<title[^>]*?>(.*?)</title>", RegexOptions.Compiled);
                Match match = reUnityQuickStart.Match(quickStartContent);
                if (match.Success)
                {
                    htmlQuickStartTitle = match.Groups[1].Value;
                }
                reUnityQuickStart = new Regex("<dd[^>]*?>(.*?)</dd>", RegexOptions.Compiled);
                match = reUnityQuickStart.Match(quickStartContent);
                if (match.Success)
                {
                    htmlQuickStartDescription = match.Groups[1].Value;
                }

                string body = Regex.Replace(quickStartContent, "<.*?>", "");
                
                quickStart.Elements.Add(new WelcomeUIElement(
                    htmlQuickStartTitle,htmlQuickStartDescription,
                    body,"",()=> { },quickStart.Elements.Count>0
                ));
                
            }

        }

        private void populateHelpfulLinks()
        {
            helpfulLinks.Elements.Add(new WelcomeUIElement(
                            "Leia Inc",
                            "Visit us at leiainc.com",
                            "Our vision is to change the way we connect, create, communicate and educate – making memories become more present, connections more human, and life, much richer.",
                            "Leia Inc Website",
                            () => { Application.OpenURL("https://www.leiainc.com"); }, false));
            helpfulLinks.Elements.Add(new WelcomeUIElement(
                 "Developer Forum",
                 "Visit our developer portal",
                 "The LeiaLoft Forum directly connects you with Lightfield enthusiasts, fellow creators, and the teams at Leia.",
                 "Developer Portal",
                 () => { Application.OpenURL("https://forums.leialoft.com/"); }, true));
            helpfulLinks.Elements.Add(new WelcomeUIElement(
                 "Developer Docs",
                 "Visit our developer docs",
                 "Here you will find key information including product documentation and content creation guidelines to help you create stunning Lightfield content.",
                 "Developer Docs",
                 () => { Application.OpenURL("https://docs.leialoft.com/developer/unity-sdk/unity-sdk-guide"); }, true));
        }

        private void populateSampleScenes()
        {
            sampleScenes.Elements.Add(new WelcomeUIElement(
                             "Leia Logo",
                             "Leia Logo Sample Scene",
                             "Leia Logo overviews several essential components for developing lightfield content for LitByLeia™ devices, including the LeiaCamera and LeiaDisplay components.",
                             "Open Leia Logo Scene",
                             () => { EditorSceneManager.OpenScene(string.Format("{0}{1}", examplesPath, "LeiaLogo/LeiaLogo.unity")); }, false));
            sampleScenes.Elements.Add(new WelcomeUIElement(
                  "Alpha Blending Cameras",
                  "Alpha Blending Cameras Sample Scene",
                  "It is common practice to have two separate cameras: one to render the 3d scene, and another to render the UI on top of it. AlphaBlendingCameras demonstrates how to properly composite multiple cameras using the Leia Unity SDK.",
                  "Open AlphaBlendingCameras Scene",
                  () => { EditorSceneManager.OpenScene(string.Format("{0}{1}", modulesPath, "AlphaBlending/Examples/AlphaBlendingCameras.unity")); }, true));
            sampleScenes.Elements.Add(new WelcomeUIElement(
                 "Leia Media Video Player",
                 "Leia Media Video Player Sample Scene",
                 "Leia Media Video Player offers a template for playing Leia Media content on LitByLeia™ devices.",
                 "Open Leia Media Video Player Scene",
                 () => { EditorSceneManager.OpenScene(string.Format("{0}{1}", modulesPath, "LeiaMedia/Examples/LeiaMediaVideoPlayer.unity")); }, true));
            sampleScenes.Elements.Add(new WelcomeUIElement(
                 "Leia Media Recorder",
                 "Leia Media Recorder Sample Scene",
                 "Share lightfield images and videos of your content with Leia Media Recorder! Leia Media Recorder uses your Leia Camera to generate Leia Media that can be enjoyed on LitByLeia™ devices.",
                 "Open Leia Media Recorder Scene",
                 () => { EditorSceneManager.OpenScene(string.Format("{0}{1}", modulesPath, "LeiaMedia/Examples/LeiaMediaRecorder.unity")); }, true));
            sampleScenes.Elements.Add(new WelcomeUIElement(
                 "Immersive Wallpaper",
                 "Immersive Wallpaper Sample Scene",
                 "With Immersive Wallpaper, most content will appear 2D, but periodic background content will simultaneously appear 3D. We can use this mode to display 3D wallpapers in a Unity app, while content is shown in 2D in the foreground.",
                 "Open Immersive Wallpaper Scene",
                 () => { EditorSceneManager.OpenScene(string.Format("{0}{1}", modulesPath, "ImmersiveWallpaper/Examples/FullscreenImmersiveWallpaper.unity")); }, true));
            sampleScenes.Elements.Add(new WelcomeUIElement(
                 "Parallax 3D Background",
                 "Parallax 3D Background Sample Scene",
                 "Tune the LeiaCamera's focus for a 3D object using FOV and BaselineScaling, then add background pixels with a separate, programmable baseline scale",
                 "Open Parallax 3D Backgrounds scene",
                 () => { EditorSceneManager.OpenScene(string.Format("{0}{1}", modulesPath, "ParallaxBackground/Examples/Parallax3DBackground.unity")); }, true));
        }
    }
}

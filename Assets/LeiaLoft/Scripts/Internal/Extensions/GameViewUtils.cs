#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace LeiaLoft
{
    public static class GameViewUtils
    {
        private const string LeiaLoftUnitySDKLabel = "LeiaLoft Unity SDK";
        private static object gameViewSizesInstance;
        private static MethodInfo getGroup;
        private static Type sizesType;

        static GameViewUtils()
        {
            sizesType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameViewSizes");
            var singleType = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
            var instanceProp = singleType.GetProperty("instance");
            getGroup = sizesType.GetMethod("GetGroup");
            gameViewSizesInstance = instanceProp.GetValue(null, null);
        }

        static object GetGroup(GameViewSizeGroupType type)
        {
            return getGroup.Invoke(gameViewSizesInstance, new object[] { (int)type });
        }

        // Leia Inc for the forseeable future should only support FixedResolution
        private enum GameViewSizeType
        {
            AspectRatio, FixedResolution
        }
        const GameViewSizeType fixedRes = GameViewSizeType.FixedResolution;

        /// <summary>
        /// UnityEditor utility for setting game view size. Automatically selects a supported build target and fixed resolution.
        /// </summary>
        /// <param name="width">Width to set game view resolution to</param>
        /// <param name="height">Height to set game view resolution to</param>
        public static void SetGameViewSize(int width, int height)
        {
            // setting game view res from no-gfx-mode or command line could cause a crash
            if (UnityEditorInternal.InternalEditorUtility.inBatchMode)
            {
                Debug.LogWarningFormat("Tried to set game view res to {0} x {1} but Application.isBatchMode was {2}. Do not set from command line",
                    width, height, UnityEditorInternal.InternalEditorUtility.inBatchMode);
                return;
            }
            BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            if (buildTargetGroup != BuildTargetGroup.Android && buildTargetGroup != BuildTargetGroup.Standalone)
            {
                return;
            }

            GameViewSizeGroupType gameViewSizeGroupType = GameViewSizeGroupToBuildTargetGroup(buildTargetGroup);

            int index = -1;
            // populates the var $index with the index of the existing matching resolution, or next index if resolution does not exist yet
            bool sizeExists = FindSize(gameViewSizeGroupType, string.Format("LeiaLoft Unity SDK {0} x {1}", width, height), out index);

            if (!sizeExists)
            {
                // inserts the resolution into the array at end of array
                AddCustomSize(fixedRes, gameViewSizeGroupType, width, height, LeiaLoftUnitySDKLabel);
            }

            if (index > -1)
            {
                SetSize(index);
            }
        }

        /// <summary>
        /// Gets actual game view width and height.
        /// </summary>
        /// <param name="width">Width of UnityEditor's game view window</param>
        /// <param name="height">Height of UnityEditor's game view window</param>
        /// <returns></returns>
        public static bool GetGameViewWidthHeight(out int width, out int height)
        {
            try
            {
                Vector2 wh = Handles.GetMainGameViewSize();
                width = (int)wh.x;
                height = (int)wh.y;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogErrorFormat("Tried to GetGameViewWidthHeight. Is batch mode? {0}. Is editor focused? {2}. Error {2}",
                    UnityEditorInternal.InternalEditorUtility.inBatchMode, Application.isFocused, e);
                width = height = 1;
                return false;
            }
        }

        /// <summary>
        /// Gets game view aspect ratio
        /// </summary>
        /// <returns>Width / height. </returns>
        public static float GetGameViewAspectRatio()
        {
            try
            {
                Vector2 wh = UnityEditor.Handles.GetMainGameViewSize();
                return wh.x * 1.0f / Mathf.Max(1.0f, wh.y);
            }
            catch (Exception e)
            {
                Debug.LogErrorFormat("Tried to GetGameViewAspectRatio. Is batch mode? {0}. Is editor focused? {1}. Error {2}",
                    UnityEditorInternal.InternalEditorUtility.inBatchMode, Application.isFocused, e);
                return 0.0f;
            }
        }

        /// <summary>
        /// Used internally to set game view resolution to a known existing index.
        /// </summary>
        /// <param name="index">Index to set game view resolution to. Ensure that this index exists.</param>
        private static void SetSize(int index)
        {
            Type gameViewEditorWindowType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameView");
            var selectedSizeIndexProp = gameViewEditorWindowType.GetProperty("selectedSizeIndex",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var gvWnd = EditorWindow.GetWindow(gameViewEditorWindowType);
            selectedSizeIndexProp.SetValue(gvWnd, index, null);

            // Spawn an object, then immediately destroy it.
            // This forces Unity to repaint scene, but does not generate a diff in the Unity scene serialization which would require scene to be re-saved
            // Repainting the scene causes Unity to recalculate UI positions for resized GameViewWindow : EditorWindow
            GameObject go = new GameObject();
            GameObject.DestroyImmediate(go);
            // works regardless of whether gvWnd.autoRepaintOnSceneChange is false or true
        }

        /// <summary>
        /// Adds a game view size.
        /// </summary>
        /// <param name="viewSizeType">Aspect ratio or fixed resolution</param>
        /// <param name="sizeGroupType">Build target</param>
        /// <param name="width">Width of game view resolution</param>
        /// <param name="height">Height of game view resolution</param>
        /// <param name="text">Label of game view resolution</param>
        private static void AddCustomSize(GameViewSizeType viewSizeType, GameViewSizeGroupType sizeGroupType, int width, int height, string text)
        {
            var group = GetGroup(sizeGroupType);
            var addCustomSize = getGroup.ReturnType.GetMethod("AddCustomSize");
            Type gameViewSizeType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameViewSize");

            object[] gameViewSizeConstructorArgs = new object[] { (int) viewSizeType, width, height, text };

            // select a constructor which has 4 elements which are enums/ints/strings
            ConstructorInfo gameViewSizeConstructor = gameViewSizeType.GetConstructors()
                .FirstOrDefault(x =>
             {
                 // lambda function defines a filter/predicate of ConstructorInfo objects.
                 // The first constructor, if any exists, which satisfies the predicate (true) will be returned
                 if (x.GetParameters().Length != gameViewSizeConstructorArgs.Length)
                 {
                     return false;
                 }

                 // iterate through constructor types + constructor args. If any mismatch, reject
                 for (int i = 0; i < gameViewSizeConstructorArgs.Length; i++)
                 {
                     Type constructorParamType = x.GetParameters()[i].ParameterType;
                     Type constructorArgType = gameViewSizeConstructorArgs[i].GetType();

                     bool isMatch = constructorParamType == constructorArgType || constructorParamType.IsEnum && constructorArgType == typeof(int);
                     if (!isMatch) return false;
                 }

                 // constructor with these params must be able to receive these args
                 return true;
             });

            if (gameViewSizeConstructor != null)
            {
                var newSize = gameViewSizeConstructor.Invoke(gameViewSizeConstructorArgs);
                addCustomSize.Invoke(group, new object[] { newSize });
            }

            sizesType.GetMethod("SaveToHDD").Invoke(gameViewSizesInstance, null);
        }

        /// <summary>
        /// Retrieves index of a resolution in GetDisplayTexts collection, if it exists in the collection.
        /// </summary>
        /// <param name="sizeGroupType">Group to search: Standalone/Android</param>
        /// <param name="text">String to search GetDisplayTexts for. Only [0-9] chars in label and GetDisplayTexts are actually considered in search</param>
        /// <param name="index">Index of match if a match was found, or first out-of-bounds index if no match was found</param>
        /// <returns>True if resolution in collection, false if resolution is not in collection</returns>
        private static bool FindSize(GameViewSizeGroupType sizeGroupType, string text, out int index)
        {
            index = -1;

            text = System.Text.RegularExpressions.Regex.Replace(text, @"[\D]", "");
            var group = GetGroup(sizeGroupType);
            var getDisplayTexts = group.GetType().GetMethod("GetDisplayTexts");
            var displayTexts = getDisplayTexts.Invoke(group, null) as string[];
            for (int i = 0; i < displayTexts.Length; i++)
            {
                if (string.IsNullOrEmpty(displayTexts[i]) || !displayTexts[i].StartsWith(LeiaLoftUnitySDKLabel))
                {
                    // skip text patterns which do not start with "LeiaLoft Unity SDK".
                    // LeiaLoft Unity SDK patterns will continue to follow a fixed res width x height description
                    // whereas some Unity versions may contain labels like 2560 x 1440 portrait
                    continue;
                }
                // compare the digits of the known resolution names, to the digits of the ideal resolution
                // if digits are a one-for-one match using string ==, then we have a match
                string display = System.Text.RegularExpressions.Regex.Replace(displayTexts[i], @"[\D]", "");
                if (display == text)
                {
                    index = i;
                    return true;
                }
            }

            // otherwise set to first index outside of array bounds, return false to warn user that size is not actually in array
            // inside of SetGameViewSize we will add the as-of-yet unknown size at index [first_index_outside_of_array_bounds]
            index = displayTexts.Length;
            return false;
        }

        /// <summary>
        /// Maps from UnityEditor.BuildTargetGroup to UnityEditor.GameViewSizeGroupType.
        /// </summary>
        /// <param name="buildTargetGroup">The BuildTargetGroup to map from</param>
        /// <returns>A matching GameViewSizeGroupType that we mapped to, if Android or Standalone; breaks otherwise.</returns>
        static GameViewSizeGroupType GameViewSizeGroupToBuildTargetGroup(BuildTargetGroup buildTargetGroup)
        {
            switch (buildTargetGroup)
            {
                case BuildTargetGroup.Android:
                    return GameViewSizeGroupType.Android;
                case BuildTargetGroup.Standalone:
                    return GameViewSizeGroupType.Standalone;
                default:
                    LogUtil.Log(LogLevel.Error, "Cannot map from buildTargetGroup {0} to GameViewSizeGroup", buildTargetGroup);
                    return (GameViewSizeGroupType) (int)buildTargetGroup;
            }
        }
    }
}

#endif

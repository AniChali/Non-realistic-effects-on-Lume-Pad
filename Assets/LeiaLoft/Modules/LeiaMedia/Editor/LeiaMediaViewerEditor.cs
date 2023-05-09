using UnityEngine;
using UnityEditor;

namespace LeiaLoft.Editor
{
     [CustomEditor(typeof(LeiaLoft.LeiaMediaViewer))]
     [CanEditMultipleObjects]
    public class LeiaMediaViewerEditor : UnityEditor.Editor
    {

        SerializedProperty activeMediaType;
        SerializedProperty mediaScaleMode;
        SerializedProperty onscreenPercent;
        GUIContent c_activeMediaType;
        MediaInfo[] mediaInfos;
        private readonly int mediaTypesCount = System.Enum.GetNames(typeof(LeiaMediaViewer.LeiaMediaType)).Length;


        class MediaInfo  {
            public GUIContent c_name;
            public SerializedProperty source;
            public SerializedProperty parsedLayout;
            public SerializedProperty useCustomLayout;
            public SerializedProperty customLayout;
            public GUIContent c_LeiaMedia_Column_count;
            public GUIContent c_LeiaMedia_Row_count;


            public MediaInfo(string name, SerializedObject serializedObject, string root, string sourceName) {
                c_name = new GUIContent(string.Format("LeiaMedia {0}", name));
                SerializedProperty rootProperty = serializedObject.FindProperty(root);
                source = rootProperty.FindPropertyRelative(sourceName);
                parsedLayout = rootProperty.FindPropertyRelative("parsedLayout");
                useCustomLayout = rootProperty.FindPropertyRelative("useCustomLayout");
                customLayout = rootProperty.FindPropertyRelative("customLayout");
                c_LeiaMedia_Column_count = new GUIContent(string.Format("LeiaMedia {0} Column count", name));
                c_LeiaMedia_Row_count = new GUIContent(string.Format("LeiaMedia {0} Row count", name));

            }
        }


        void OnEnable()
        {
            activeMediaType = serializedObject.FindProperty("m_activeMediaType");
            onscreenPercent = serializedObject.FindProperty("m_onscreenPercent");
            c_activeMediaType = new GUIContent("Select active LeiaMedia", "Users can select which LeiaMedia is being displayed");
            mediaInfos = new MediaInfo[mediaTypesCount];
            mediaInfos[0] = new MediaInfo("Texture", serializedObject, "mitexture", "texture");
            mediaInfos[1] = new MediaInfo("Video", serializedObject, "miclip", "clip");
            mediaInfos[2] = new MediaInfo("VideoURL", serializedObject, "miurl", "url");
            mediaScaleMode = serializedObject.FindProperty("m_mediaScaleMode");
            onscreenPercent = serializedObject.FindProperty("m_onscreenPercent");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(activeMediaType, c_activeMediaType);
            EditorGUILayout.LabelField("Set media here. Do NOT use VideoPlayer.");
         
            MediaInfo mi = mediaInfos[activeMediaType.intValue];
            EditorGUILayout.PropertyField(mi.source, mi.c_name);
            if (!mi.useCustomLayout.boolValue) {
                Vector2Int parsedLayout = mi.parsedLayout.vector2IntValue;
                if (parsedLayout.x < 0 && parsedLayout.y < 0)
                {
                    EditorGUILayout.HelpBox("Media filename should have format [name...]_[cols]x[rows].[fmt] or use Custom Layout", MessageType.Info);
                }
                else
                {
                    GUI.enabled = false;
                    EditorGUILayout.IntField(mi.c_LeiaMedia_Column_count, parsedLayout.x);
                    EditorGUILayout.IntField(mi.c_LeiaMedia_Row_count, parsedLayout.y);
                    GUI.enabled = true;
                } 
            }

            if (mi.useCustomLayout.boolValue)
            {
                Vector2Int columnsRows = mi.customLayout.vector2IntValue;
                columnsRows.x = EditorGUILayout.IntField(mi.c_LeiaMedia_Column_count, columnsRows.x);
                columnsRows.y = EditorGUILayout.IntField(mi.c_LeiaMedia_Row_count, columnsRows.y);
                mi.customLayout.vector2IntValue = columnsRows;
            }

            EditorGUILayout.PropertyField(mi.useCustomLayout);


            if (GUILayout.Button("Move to Convergence Plane")  )
            {
                (target as LeiaMediaViewer).ProjectOntoZDP();
            }

            EditorGUILayout.PropertyField(mediaScaleMode);

            if (mediaScaleMode.boolValue)
            {
                EditorGUILayout.PropertyField(onscreenPercent);
            }

            serializedObject.ApplyModifiedProperties();
        }

       
    }
}

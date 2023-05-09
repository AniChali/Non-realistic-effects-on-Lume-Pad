namespace LeiaLoft
{
    using UnityEditor;
    using UnityEditor.UI;

    [CustomEditor(typeof(FloatIntervalSlider))]
    public class FloatIntervalSliderEditor : SliderEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_snapInterval"));
            serializedObject.ApplyModifiedProperties();
        } 
    }
}

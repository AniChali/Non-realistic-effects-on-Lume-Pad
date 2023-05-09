using UnityEditor;
using UnityEngine;

namespace LeiaLoft.Editor
{
    // MinMaxPairDrawer
    [CustomPropertyDrawer(typeof(MinMaxPair))]
    public class MinMaxPairDrawer : PropertyDrawer
    {
        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // have to have this call prior to BeginProperty or we experience sequencing issues
            // have to clear the tooltip or else it spreads to other property drawers
            label.tooltip = "";
            foreach (var tta in (TooltipAttribute[])fieldInfo.GetCustomAttributes(typeof(TooltipAttribute), true))
            {
                // have to clone the tooltip in from field attribute since there is an issue in early Unity versionswhere GUIContent property might not have tooltip automatically set
                label.tooltip = tta.tooltip;
            }

            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            // Draw label
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Don't make child fields be indented
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            const int hMax = 4;
            // Calculate rects
            Rect[] rects = new Rect[hMax];

            for (int hCounter = 0; hCounter < hMax; ++hCounter) {
                rects[hCounter] = new Rect(position.x + hCounter * position.width / hMax, position.y, position.width / hMax, position.height);
            }

            SerializedProperty myMin = property.FindPropertyRelative("_min");
            SerializedProperty minLabel = property.FindPropertyRelative("_minLabel");
            SerializedProperty myMax = property.FindPropertyRelative("_max");
            SerializedProperty maxLabel = property.FindPropertyRelative("_maxLabel");

            // cache float values
            float priorMin = myMin.floatValue;
            float priorMax = myMax.floatValue;

            // Draw fields - passs GUIContent.none to each so they are drawn without labels
            EditorGUI.LabelField(rects[0], minLabel.stringValue);
            float v1 = EditorGUI.DelayedFloatField(rects[1], priorMin);
            EditorGUI.LabelField(rects[2], maxLabel.stringValue);
            float v2 = EditorGUI.DelayedFloatField(rects[3], priorMax);

            // if values weren't preserved, then they were updated. write updated data into properties
            if (!(Mathf.Approximately(priorMin, v1) && Mathf.Approximately(priorMax, v2)))
            {
                // write into serialized data of the min/max object. Hope this does not introduce bugs.
                // the MinMaxPair has current {min,max} as well as a lifetime {min,max}. See implementation in MinMaxPair.min setter property
                // later we might have to attach a callback to MinMaxPair so that when MinMaxPair data is updated using serialized properties
                myMin.floatValue = Mathf.Max(Mathf.Min(v1, v2), property.FindPropertyRelative("_lifetimeMin").floatValue);
                myMax.floatValue = Mathf.Min(Mathf.Max(v1, v2), property.FindPropertyRelative("_lifetimeMax").floatValue);
            }

            // Set indent back to what it was
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
    }
}

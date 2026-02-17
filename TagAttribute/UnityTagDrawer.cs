using UnityEditor;
using UnityEngine;


/// <summary>
/// Used to have a tag dropdown (instead of a plain string field)
/// Override how Unity draws string properties if "[UnityTag]" is used before it
/// Need to be in a "Editor" folder
/// Need "UnityTagAttribute.cs" to be in a non-Editor folder
/// </summary>
[CustomPropertyDrawer(typeof(UnityTagAttribute))]
public class UnityTagDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        if (property.propertyType != SerializedPropertyType.String) {
            EditorGUI.LabelField(position, label.text, "Use [UnityTag] only on string fields!");
            return;
        }

        EditorGUI.BeginProperty(position, label, property);

        // The magic line
        property.stringValue = EditorGUI.TagField(position, label, property.stringValue);

        EditorGUI.EndProperty();
    }
}

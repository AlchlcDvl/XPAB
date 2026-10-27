namespace XPAB;

[CustomPropertyDrawer(typeof(Optional<>))]
internal class OptionalPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var hasValueProp = property.FindPropertyRelative("HasValue");
        var valueProp = property.FindPropertyRelative("Value");

        EditorGUI.BeginProperty(position, label, property);

        var singleLineHeight = EditorGUIUtility.singleLineHeight;
        var toggleRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, singleLineHeight);

        hasValueProp.boolValue = EditorGUI.ToggleLeft(toggleRect, label, hasValueProp.boolValue);

        var valueRect = new Rect(position.x, position.y, position.width, position.height);
        var previousGUIState = GUI.enabled;

        GUI.enabled = hasValueProp.boolValue;
        EditorGUI.PropertyField(valueRect, valueProp, new GUIContent(" "), true);

        GUI.enabled = previousGUIState;

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var valueProp = property.FindPropertyRelative("Value");
        return EditorGUI.GetPropertyHeight(valueProp, true);
    }
}

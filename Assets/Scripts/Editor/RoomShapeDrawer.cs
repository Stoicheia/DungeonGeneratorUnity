using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(RoomShape))]
public class RoomShapeDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return 25 + property.FindPropertyRelative(nameof(RoomGenerationParameters.Size)).intValue * 20;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty shape = property.FindPropertyRelative(nameof(RoomGenerationParameters.SquashedShape));
        SerializedProperty size = property.FindPropertyRelative(nameof(RoomGenerationParameters.Size));

        EditorGUI.IntSlider(new Rect(position.x, position.y, position.width, 20), size, 1, 5,
            new GUIContent(ObjectNames.NicifyVariableName(property.name)));
        shape.arraySize = size.intValue * size.intValue;
        for (int i = 0; i < size.intValue; i++)
        {
            for (int j = 0; j < size.intValue; j++)
            {
                shape.GetArrayElementAtIndex(i * size.intValue + j).boolValue = EditorGUI.Toggle(
                    new Rect(position.x + 10 + i * 20
                        , 20 + position.y + j * 20, 20, 20)
                    , shape.GetArrayElementAtIndex(i * size.intValue + j).boolValue
                );
            }
        }
    }
}

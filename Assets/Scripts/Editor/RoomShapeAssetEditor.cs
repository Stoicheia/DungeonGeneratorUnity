using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(RoomShape))]
public class RoomShapeAssetEditor : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return 50 + property.FindPropertyRelative(nameof(RoomShape.Size)).intValue * 20;
    }
    
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty shape = property.FindPropertyRelative(nameof(RoomShape.SquashedShape));
        SerializedProperty size = property.FindPropertyRelative(nameof(RoomShape.Size));

        EditorGUI.IntSlider(new Rect(position.x, position.y, position.width, 20), size, 1, 5, 
            new GUIContent("Room Shape "));
        shape.arraySize = size.intValue * size.intValue;
        for (int i = 0; i < size.intValue; i++)
        {
            for (int j = 0; j < size.intValue; j++)
            {
                shape.GetArrayElementAtIndex(i * size.intValue + j).boolValue = EditorGUI.Toggle(new Rect(20 + position.x + i * 20
                        ,  30 + position.y + j * 20, 20, 20)
                   , shape.GetArrayElementAtIndex(i * size.intValue + j).boolValue
                );
            }
        }

        if (GUI.Button(new Rect(position.xMax - 100, position.y + 25, 100, 20), "Fill All"))
        {
            for (int i = 0; i < size.intValue; i++)
            {
                for (int j = 0; j < size.intValue; j++)
                {
                    shape.GetArrayElementAtIndex(i * size.intValue + j).boolValue = true;
                }
            }
        }
        
        if (GUI.Button(new Rect(position.xMax - 100, position.y + 50, 100, 20), "Empty All"))
        {
            for (int i = 0; i < size.intValue; i++)
            {
                for (int j = 0; j < size.intValue; j++)
                {
                    shape.GetArrayElementAtIndex(i * size.intValue + j).boolValue = false;
                }
            }
        }

        EditorGUI.EndProperty();
    }
}

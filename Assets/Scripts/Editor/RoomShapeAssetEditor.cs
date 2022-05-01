using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(RoomShape))]
public class RoomShapeAssetEditor : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return 160 + property.FindPropertyRelative(nameof(RoomShape.Size)).intValue * 20;
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
                shape.GetArrayElementAtIndex(i * size.intValue + j).boolValue = EditorGUI.Toggle(
                    new Rect(position.x + 10 + i * 20
                        ,  30 + position.y + j * 20, 20, 20)
                   , shape.GetArrayElementAtIndex(i * size.intValue + j).boolValue
                );
            }
        }
        
        GUIStyle shapeLabel = new GUIStyle();
        shapeLabel.fontSize = 17;
        shapeLabel.fontStyle = FontStyle.Bold;
        shapeLabel.normal.textColor = Color.white;
        EditorGUI.LabelField(
            new Rect(position.x + size.intValue * 20 + 27, position.y + size.intValue * 10 + 19 , 200, 50),
            "Edit Shape", shapeLabel);
        
        property.FindPropertyRelative(nameof(RoomShape.Weight)).floatValue = EditorGUI.FloatField(
            new Rect(position.x, position.y + GetPropertyHeight(property, label) - 120, position.width, 20),
            "Weight",
            property.FindPropertyRelative(nameof(RoomShape.Weight)).floatValue
        );
        property.FindPropertyRelative(nameof(RoomShape.MinCount)).intValue = EditorGUI.IntField(
            new Rect(position.x, position.y + GetPropertyHeight(property, label) - 100, position.width, 20),
            "Min Count",
            property.FindPropertyRelative(nameof(RoomShape.MinCount)).intValue
        );
        property.FindPropertyRelative(nameof(RoomShape.MaxCount)).intValue = EditorGUI.IntField(
            new Rect(position.x, position.y + GetPropertyHeight(property, label) - 80, position.width, 20),
            "Max Count",
            property.FindPropertyRelative(nameof(RoomShape.MaxCount)).intValue
        );
        
        property.FindPropertyRelative(nameof(RoomShape.OverrideGenerationRules)).boolValue = EditorGUI.ToggleLeft(
            new Rect(position.x, position.y + GetPropertyHeight(property, label) - 50, 140, 20),
            "Override Rules",
            property.FindPropertyRelative(nameof(RoomShape.OverrideGenerationRules)).boolValue
        );

        if (property.FindPropertyRelative(nameof(RoomShape.OverrideGenerationRules)).boolValue)
        {
            EditorGUI.PropertyField(
                new Rect(position.x + 180, position.y + GetPropertyHeight(property, label) - 50, 200, 20)
                , property.FindPropertyRelative(nameof(RoomShape.NeighboursRuleset)), GUIContent.none
                );
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

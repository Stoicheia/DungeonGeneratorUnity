using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(RoomGenerationParameters))]
public class RoomShapeAssetEditor : PropertyDrawer
{
    private const string INSTRUCTION =
        "If weight = 0, then only counts are considered.\nIf max count <= 0, then only weight is considered.\nNon-contiguous shapes are ignored.";
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return 170 + property.FindPropertyRelative(nameof(RoomGenerationParameters.Size)).intValue * 20;
    } 
    
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty shape = property.FindPropertyRelative(nameof(RoomGenerationParameters.SquashedShape));
        SerializedProperty size = property.FindPropertyRelative(nameof(RoomGenerationParameters.Size));

        EditorGUI.IntSlider(new Rect(position.x, position.y, position.width, 20), size, 1, 5, 
            new GUIContent("Size "));
        shape.arraySize = size.intValue * size.intValue;
        for (int i = 0; i < size.intValue; i++)
        {
            for (int j = 0; j < size.intValue; j++)
            {
                shape.GetArrayElementAtIndex(i * size.intValue + j).boolValue = EditorGUI.Toggle(
                    new Rect(position.x + 10 + i * 20
                        ,  45 + position.y + j * 20, 20, 20)
                   , shape.GetArrayElementAtIndex(i * size.intValue + j).boolValue
                );
            }
        }
        
        GUIStyle shapeLabel = new GUIStyle();
        shapeLabel.fontSize = 17;
        shapeLabel.fontStyle = FontStyle.Bold;
        shapeLabel.normal.textColor = Color.white;
        EditorGUI.LabelField(
            new Rect(position.x + size.intValue * 20 + 27, position.y + size.intValue * 10 + 34 , 200, 50),
            "Edit Shape", shapeLabel);
        
        GUIStyle instructionLabel = new GUIStyle();
        instructionLabel.fontSize = 7;
        instructionLabel.fontStyle = FontStyle.Italic;
        instructionLabel.normal.textColor = Color.white;
        EditorGUI.LabelField(
            new Rect(position.x + 5, position.y + 17 , 100, 50),
            INSTRUCTION, instructionLabel);
        
        property.FindPropertyRelative(nameof(RoomGenerationParameters.Weight)).floatValue = EditorGUI.FloatField(
            new Rect(position.x, position.y + GetPropertyHeight(property, label) - 120, position.width, 20),
            "Weight",
            property.FindPropertyRelative(nameof(RoomGenerationParameters.Weight)).floatValue
        );
        property.FindPropertyRelative(nameof(RoomGenerationParameters.MinCount)).intValue = EditorGUI.IntField(
            new Rect(position.x, position.y + GetPropertyHeight(property, label) - 100, position.width, 20),
            "Min Count",
            property.FindPropertyRelative(nameof(RoomGenerationParameters.MinCount)).intValue
        );
        property.FindPropertyRelative(nameof(RoomGenerationParameters.MaxCount)).intValue = EditorGUI.IntField(
            new Rect(position.x, position.y + GetPropertyHeight(property, label) - 80, position.width, 20),
            "Max Count",
            property.FindPropertyRelative(nameof(RoomGenerationParameters.MaxCount)).intValue
        );
        property.FindPropertyRelative(nameof(RoomGenerationParameters.Connections)).intValue = EditorGUI.IntSlider(
            new Rect(position.x, position.y + GetPropertyHeight(property, label) - 60, position.width, 20),
            "Connections",
            property.FindPropertyRelative(nameof(RoomGenerationParameters.Connections)).intValue, 1, 4 * size.intValue
        );
        
        property.FindPropertyRelative(nameof(RoomGenerationParameters.OverrideGenerationRules)).boolValue = EditorGUI.ToggleLeft(
            new Rect(position.x, position.y + GetPropertyHeight(property, label) - 35, 140, 20),
            "Override Rules",
            property.FindPropertyRelative(nameof(RoomGenerationParameters.OverrideGenerationRules)).boolValue
        );

        if (property.FindPropertyRelative(nameof(RoomGenerationParameters.OverrideGenerationRules)).boolValue)
        {
            EditorGUI.PropertyField(
                new Rect(position.x + 180, position.y + GetPropertyHeight(property, label) - 35, 200, 20)
                , property.FindPropertyRelative(nameof(RoomGenerationParameters.NeighboursRuleset)), GUIContent.none
                );
        }

        if (GUI.Button(new Rect(position.xMax - 75, position.y + 25, 75, 20), "Fill All"))
        {
            for (int i = 0; i < size.intValue; i++)
            {
                for (int j = 0; j < size.intValue; j++)
                {
                    shape.GetArrayElementAtIndex(i * size.intValue + j).boolValue = true;
                }
            }
        }
        
        if (GUI.Button(new Rect(position.xMax - 75, position.y + 45, 75, 20), "Empty All"))
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

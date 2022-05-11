using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DungeonDrawer))]
public class DungeonDrawerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DungeonDrawer obj = target as DungeonDrawer;
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_generator"));
        GUILayout.Space(20);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_tilesContainer"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_doorsContainer"));
        GUILayout.Space(20);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_tilePrefab"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_verticalDoorPrefab"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_horizontalDoorPrefab"));
        GUILayout.Space(20);
        
        if (GUILayout.Button("Init"))
        {
            obj.InitialiseGrid();
        }
        if (GUILayout.Button("Draw"))
        {
            obj.Draw(obj.Info);
        }
        if (GUILayout.Button("Generate and Draw"))
        {
            obj.Generate();
            obj.Draw(obj.Info);
        }
        if (GUILayout.Button("Clear"))
        {
            obj.Clear();
        }
        

        serializedObject.ApplyModifiedProperties();
    }
}

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
        if (GUILayout.Button("Draw"))
        {
            obj.Draw(obj.Info);
        }
        if (GUILayout.Button("Generate"))
        {
            obj.Generate();
        }
    }
}

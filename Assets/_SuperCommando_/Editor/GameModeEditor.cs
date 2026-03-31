using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameMode))]
public class GameModeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if(GUILayout.Button("RESET ALL"))
        {
            PlayerPrefs.DeleteAll();
            Debug.Log("DELETED ALL DATA");
        }

        if (GUILayout.Button("UNLOCK ALL"))
        {
            PlayerPrefs.SetInt(GlobalValue.WorldReached, int.MaxValue);
            for (int i = 1; i < 100; i++)
            {
                PlayerPrefs.SetInt(i.ToString(), 10000);        //world i, unlock 10000 levels
            }

            GlobalValue.LevelHighest = 9999;

            Debug.Log("UNLOCKED ALL");
        }

        if (GUILayout.Button("UNLOCK 15 LEVELS"))
        {
            GlobalValue.LevelHighest = 15;
            Debug.Log("UNLOCKED 15 LEVELS");
        }
    }
}

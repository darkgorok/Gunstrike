using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GameMode))]
public class GameModeEditor : Editor
{
    private readonly IKeyValueStore keyValueStore = new UnityPrefsKeyValueStore();

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("RESET ALL"))
        {
            keyValueStore.DeleteAll();
            Debug.Log("DELETED ALL DATA");
        }

        if (GUILayout.Button("UNLOCK ALL"))
        {
            keyValueStore.SetInt("WorldReached", int.MaxValue);
            for (int i = 1; i < 100; i++)
            {
                keyValueStore.SetInt(i.ToString(), 10000);
            }

            keyValueStore.SetInt("LevelHighest", 9999);
            Debug.Log("UNLOCKED ALL");
        }

        if (GUILayout.Button("UNLOCK 15 LEVELS"))
        {
            keyValueStore.SetInt("LevelHighest", 15);
            Debug.Log("UNLOCKED 15 LEVELS");
        }
    }
}

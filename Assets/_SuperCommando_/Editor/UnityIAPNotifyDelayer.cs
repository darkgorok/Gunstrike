using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class UnityIAPNotifyDelayer
{
    private const string CheckTimeKey = "PackageUpdaterLastChecked68207";

    static UnityIAPNotifyDelayer()
    {
        string setTime = "1/1/2020 0:0:0 AM";
        string curtime = EditorPrefs.GetString(CheckTimeKey);
        if (curtime != setTime)
        {
            EditorPrefs.SetString(CheckTimeKey, setTime);
            Debug.Log("Unity IAP Check Time is Changed [ " + curtime + " >> " + setTime + " ]");
        }
    }
}

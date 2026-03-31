using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class UnityIAPNotifyDelayer
{
    static UnityIAPNotifyDelayer()
    {
        string setTime = "1/1/2020 0:0:0 AM";
        string curtime = PlayerPrefs.GetString("PackageUpdaterLastChecked68207");
        if (curtime != setTime)
        {
            PlayerPrefs.SetString("PackageUpdaterLastChecked68207", setTime);
            Debug.Log("Unity IAP Check Time is Changed [ " + curtime + " >> " + setTime + " ]");
        }
        //else
        //Debug.Log("Unity IAP Check Time is [ " + setTime + " ]");
    }
}
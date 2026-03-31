using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BtnDartUI : MonoBehaviour
{
    //public Text dartTxt;
    //public float timeMakeNewDart = 3f;
    //float coolDownCounter = 0;
    //public Image image;
    //bool isMaking = false;
    //void Start()
    //{
    //    GetComponent<Button>().onClick.AddListener(OnBtnPress);
    //}

    //void OnBtnPress()
    //{
    //    ControllerInput.Instance.RangeAttack(false);
    //}

    //private void Update()
    //{
    //    dartTxt.text = GlobalValue.Bullets + "";
    //    dartTxt.color = GlobalValue.Bullets == GlobalValue.getDartLimited() ? Color.red : Color.white;

    //    if (GlobalValue.Bullets > 0)
    //    {
    //        image.fillAmount = 1;
    //        coolDownCounter = timeMakeNewDart;
    //        return;
    //    }
    //    else
    //    {
    //        coolDownCounter -= Time.deltaTime;
    //        image.fillAmount = Mathf.Clamp01((timeMakeNewDart - coolDownCounter) / timeMakeNewDart);
    //        if (coolDownCounter <= 0)
    //        {
    //            image.fillAmount = 1;
    //            GlobalValue.Bullets++;
    //        }
    //    }
    //}
}

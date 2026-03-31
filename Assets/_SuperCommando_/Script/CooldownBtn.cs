using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CooldownBtn : MonoBehaviour
{
  //  public float coolDown = 3f;
  //  [ReadOnly] public float reduceTime = 0;
  //[ReadOnly] public float coolDownCounter = 0;
  //  public Image image;
  //  public Button btn;
  //  bool allowWork = true;
  //  bool canUse = true;
  //  float holdCounter = 0;
  //  public CanvasGroup canvasGroup;
    
  //  void Start()
  //  {
  //      if (btn == null)
  //          btn = GetComponent<Button>();
  //      if (image == null)
  //          image = GetComponent<Image>();
  //      if (canvasGroup == null)
  //          canvasGroup = GetComponent<CanvasGroup>();

  //      //btn.onClick.AddListener(OnBtnPress);

  //      reduceTime = GlobalValue.UpgradeItemPower(UPGRADE_ITEM_TYPE.doggeRecharge.ToString());
  //      coolDown -= reduceTime;

  //  }

  // public void OnBtnPress()
  //  {
  //      if (!canUse)
  //          return;

  //      if (!allowWork)
  //          return;
        
  //      allowWork = false;
  //      coolDownCounter = coolDown;
  //      ControllerInput.Instance.Dogge();
  //  }

  //  // Update is called once per frame
  //  void Update()
  //  {
  //      if (!allowWork)
  //      {
  //          coolDownCounter -= Time.deltaTime;

  //          if (coolDownCounter <= 0)
  //              allowWork = true;
  //      }
  //      else
  //      {
  //          //but.gameObject.SetActive(GlobalValue.spear > 0 && GameManager.Instance.Player.allowThrownSpear);
  //          holdCounter -= Time.deltaTime;
  //      }

  //      if (allowWork && Input.GetKeyDown(DefaultValue.Instance == null ? DefaultValueKeyboard.Instance.keyDogge : DefaultValue.Instance.keyDogge))
  //      {
  //          //ControllerInput.Instance.Dogge();
  //          OnBtnPress();
  //      }

  //      image.fillAmount = Mathf.Clamp01((coolDown - coolDownCounter) / coolDown);
  //      //but.interactable = coolDownCounter <= 0;
  //      //canvasGroup.blocksRaycasts = coolDownCounter <= 0;
  //      canvasGroup.interactable = coolDownCounter <= 0;

  //      canUse = canvasGroup.interactable && canvasGroup.blocksRaycasts;
  //  }

  //  private void OnDisable()
  //  {
  //      if(GameManager.Instance.State == GameManager.GameState.Waiting || GameManager.Instance.State == GameManager.GameState.Dead)
  //          coolDownCounter = 0;
  //  }
}

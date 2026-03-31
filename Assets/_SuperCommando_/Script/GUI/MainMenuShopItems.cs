using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MainMenuShopItems : MonoBehaviour {
	//public int livePrice;
	//public int bulletPrice;
 //   public int powerBulletPrice;
 //   public int godPrice;

 //   public AudioClip boughtSound;
	//[Range(0,1)]
	//public float boughtSoundVolume = 0.5f;

	//public Text livePriceTxt;
	//public Text bulletPriceTxt;
 //   public Text powerBulletPriceTxt;
 //   public Text godPriceTxt;

 //   public Text livesTxt;
	//public Text bulletTxt;
 //   public Text powerBulletTxt;
 //   public Text godItemTxt;

 //   public GameObject normalBtn;

 //   private void Awake()
 //   {
 //       if (DefaultValue.Instance)
 //       {
 //           livePrice = DefaultValue.Instance.livePrice;
 //           bulletPrice = DefaultValue.Instance.bulletPrice;
 //           powerBulletPrice = DefaultValue.Instance.powerBulletPrice;
 //           godPrice = DefaultValue.Instance.godPrice;
 //           normalBtn.SetActive(!DefaultValue.Instance.defaultBulletMax);
 //       }
 //   }
 //   // Use this for initialization
 //   void Start () {
	//	livePriceTxt.text = livePrice.ToString ();
 //       bulletPriceTxt.text = bulletPrice.ToString ();
 //       powerBulletPriceTxt.text = powerBulletPrice.ToString();
 //       godPriceTxt.text = godPrice.ToString();
 //   }

	//void Update(){
	//	//livesTxt.text = "Remain: " + PlayerPrefs.GetInt (GlobalValue.Lives, DefaultValue.Instance != null ? DefaultValue.Instance.defaultLives : 10);
 //       if (DefaultValue.Instance && DefaultValue.Instance.defaultBulletMax)
 //           bulletTxt.text = "MAX";
 //       else
 //           bulletTxt.text = "Remain: " + GlobalValue.Bullets;
 //       powerBulletTxt.text = "Remain: " + GlobalValue.powerBullet;
 //       godItemTxt.text = "Remain: " + GlobalValue.storeGod;
 //   }
	
	//public void BuyLive(){
	//	var coins = GlobalValue.SavedCoins;
	//	if (coins >= livePrice) {
	//		coins -= livePrice;
	//		PlayerPrefs.SetInt (GlobalValue.Coins, coins);
	//		//var lives = PlayerPrefs.GetInt (GlobalValue.Lives, DefaultValue.Instance != null ? DefaultValue.Instance.defaultLives : 10);
	//		//lives++;
	//		//PlayerPrefs.SetInt (GlobalValue.Lives, lives);

	//		SoundManager.PlaySfx (boughtSound, boughtSoundVolume);
	//	} else
	//		NotEnoughCoins.Instance.ShowUp ();
	//}

	//public void BuyBullet(){
	//	var coins = GlobalValue.SavedCoins;
	//	if (coins >= bulletPrice) {
	//		coins -= bulletPrice;
	//		PlayerPrefs.SetInt (GlobalValue.Coins, coins);
 //           if (DefaultValue.Instance && DefaultValue.Instance.defaultBulletMax)
 //               Debug.Log("No Limit Bullet");
 //           else
 //               GlobalValue.Bullets++;

	//		SoundManager.PlaySfx (boughtSound, boughtSoundVolume);
	//	} else
	//		NotEnoughCoins.Instance.ShowUp ();
	//}

 //   public void BuyPowerBullet()
 //   {
 //       var coins = GlobalValue.SavedCoins;
 //       if (coins >= powerBulletPrice)
 //       {
 //           coins -= powerBulletPrice;
 //           PlayerPrefs.SetInt(GlobalValue.Coins, coins);
 //           GlobalValue.powerBullet++;
 //           //var bullets = PlayerPrefs.GetInt(GlobalValue.powerBullet, 0);
 //           //bullets++;
 //           //PlayerPrefs.SetInt(GlobalValue.powerb, bullets);

 //           SoundManager.PlaySfx(boughtSound, boughtSoundVolume);
 //       }
 //       else
 //           NotEnoughCoins.Instance.ShowUp();
 //   }

 //   public void BuyGodItem()
 //   {
 //       var coins = GlobalValue.SavedCoins;
 //       if (coins >= godPrice)
 //       {
 //           coins -= godPrice;
 //           PlayerPrefs.SetInt(GlobalValue.Coins, coins);
 //           GlobalValue.storeGod++;
 //           //var bullets = PlayerPrefs.GetInt(GlobalValue.powerBullet, 0);
 //           //bullets++;
 //           //PlayerPrefs.SetInt(GlobalValue.powerb, bullets);

 //           SoundManager.PlaySfx(boughtSound, boughtSoundVolume);
 //       }
 //       else
 //           NotEnoughCoins.Instance.ShowUp();
 //   }
}

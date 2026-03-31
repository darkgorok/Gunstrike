using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MainMenu_ChracterChoose : MonoBehaviour {

//	[Tooltip("The unique character ID")]
//	public int characterID;
//	public int price;
//	public GameObject CharacterPrefab;

//	public bool unlockDefault = false;

////	public GameObject Locked;
//	public GameObject UnlockButton;

//	public Text pricetxt;
//	public Text state;

//    public AudioClip pickSound;

//	bool isUnlock;
//	SoundManager soundManager;
//    Animator anim;

//    private void Awake()
//    {
//        if (DefaultValue.Instance)
//        {
//            switch (characterID)
//            {
//                case 2:
//                    price = DefaultValue.Instance.char2Price;
//                    break;
//                case 3:
//                    price = DefaultValue.Instance.char3Price;
//                    break;
//            }
           
//        }
//    }

//    // Use this for initialization
//    void Start () {
//		soundManager = FindObjectOfType<SoundManager> ();
//        anim = GetComponent<Animator>();

//        if (unlockDefault)
//			isUnlock = true;
//		else
//			isUnlock = PlayerPrefs.GetInt (GlobalValue.Character + characterID, 0) == 1 ? true : false;
		
////		Locked.SetActive (!isUnlock);
//		UnlockButton.SetActive (!isUnlock);

//		pricetxt.text = price.ToString ();
//	}

//	void Update(){
		
//		if (!isUnlock)
//			return;
		
//		if (PlayerPrefs.GetInt (GlobalValue.ChoosenCharacterID, 1) == characterID)
//			state.text = "Picked";
//		else
//			state.text = "Choose";
//	}
	
//	public void Unlock(){
//		SoundManager.PlaySfx (soundManager.soundClick);
//        var coins = GlobalValue.SavedCoins;
//		if (coins >= price) {
//			coins -= price;
//			PlayerPrefs.SetInt (GlobalValue.Coins, coins);
//			//Unlock
//			PlayerPrefs.SetInt (GlobalValue.Character + characterID, 1);
//			isUnlock = true;
////			Locked.SetActive (false);
//			UnlockButton.SetActive (false);
//		} else
//			NotEnoughCoins.Instance.ShowUp ();
//	}

//	public void Pick(){
//		SoundManager.PlaySfx (soundManager.soundClick);
        
//		if (!isUnlock) {
//			Unlock ();
//			return;
//		}
//        anim.SetTrigger("Pick");
//        SoundManager.PlaySfx(pickSound);
//        PlayerPrefs.SetInt (GlobalValue.ChoosenCharacterID, characterID);
//		PlayerPrefs.SetInt (GlobalValue.ChoosenCharacterInstanceID, CharacterPrefab.GetInstanceID ());
//		CharacterHolder.Instance.CharacterPicked = CharacterPrefab;
//	}
}

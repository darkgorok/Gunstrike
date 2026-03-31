using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Menu_GUI : MonoBehaviour {
    public static Menu_GUI Instance;
	public Text bulletText;
    public Text grenadeTxt;
    public Text liveTxt;
    private void Awake()
    {
        Instance = this;
    }

    bool firstPlay = true;

    private void Start()
    {
        firstPlay = false;
    }

    private void OnEnable()
    {
        if (firstPlay)
            return;
    }

    void Update()
    {
        bulletText.text = GameManager.Instance.Player.gunTypeID.unlimitedBullet ? "-/-" : (GameManager.Instance.Player.gunTypeID.bullet + "");
        grenadeTxt.text = GameManager.Instance.Player.grenadeRemaining + "";
        liveTxt.text = GlobalValue.SaveLives + "";
    }
}

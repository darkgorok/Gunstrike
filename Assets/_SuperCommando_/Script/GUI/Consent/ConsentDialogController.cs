using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class ConsentDialogController : MonoBehaviour
{
    private const string Title = "USER CONSENT";
    private const string BodyCopy =
        "We collect limited data (such as device info and gameplay activity) to operate, improve, and analyze the app, and to provide relevant content or ads.\n\n" +
        "By tapping \"Accept\", you agree to this data use. Third-party services (e.g., analytics) may also process data. You can withdraw consent anytime in settings or by uninstalling the app.\n\n" +
        "See our Privacy Policy for details.";
    private const string Footer = "Consent is required to continue.";

    [Header("ROOT")]
    [SerializeField] private GameObject dialogRoot;
    [SerializeField] private bool persistAcrossScenes = true;

    [Header("TEXT")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private TMP_Text footerText;

    [Header("BUTTONS")]
    [SerializeField] private Button acceptButton;

    private Action onAccept;

    public bool IsVisible => dialogRoot != null && dialogRoot.activeSelf;

    public static ConsentDialogController FindSceneInstance()
    {
        var dialogs = UnityEngine.Object.FindObjectsByType<ConsentDialogController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < dialogs.Length; i++)
        {
            if (dialogs[i] != null && dialogs[i].gameObject.scene.IsValid())
                return dialogs[i];
        }

        return null;
    }

    private void Awake()
    {
        if (dialogRoot == null)
            dialogRoot = gameObject;

        if (persistAcrossScenes)
            DontDestroyOnLoad(gameObject);

        WireButtons();
        Hide();
    }

    public void Show(Action onAccept)
    {
        this.onAccept = onAccept;

        if (titleText != null)
            titleText.text = Title;

        if (bodyText != null)
            bodyText.text = BodyCopy;

        if (footerText != null)
            footerText.text = Footer;

        if (acceptButton != null)
            acceptButton.gameObject.SetActive(true);

        dialogRoot.SetActive(true);
        SelectPrimaryButton();
    }

    public void Hide()
    {
        if (dialogRoot != null)
            dialogRoot.SetActive(false);
    }

    private void WireButtons()
    {
        if (acceptButton != null)
        {
            acceptButton.onClick.RemoveListener(HandleAccept);
            acceptButton.onClick.AddListener(HandleAccept);
        }
    }

    private void SelectPrimaryButton()
    {
        if (EventSystem.current == null)
            return;

        if (acceptButton != null && acceptButton.gameObject.activeInHierarchy)
            EventSystem.current.SetSelectedGameObject(acceptButton.gameObject);
    }

    private void HandleAccept()
    {
        onAccept?.Invoke();
    }
}

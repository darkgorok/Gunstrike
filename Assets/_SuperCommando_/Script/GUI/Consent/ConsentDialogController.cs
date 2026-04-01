using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class ConsentDialogController : MonoBehaviour
{
    private string title;
    private string body;
    private string footer;

    [SerializeField] private ConsentDialogConfig config;

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

        Init();
        Hide();
    }

    void OnEnable()
    {
        acceptButton.onClick.AddListener(HandleAccept);
    }

    void OnDisable()
    {
        acceptButton.onClick.RemoveListener(HandleAccept);
    }

    private void Init()
    {
        title = config.Title;
        body = config.Body;
        footer = config.Footer;
    }

    public void Show(Action onAccept)
    {
        this.onAccept = onAccept;

        if (titleText != null)
            titleText.text = title;

        if (bodyText != null)
            bodyText.text = body;

        if (footerText != null)
            footerText.text = footer;

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

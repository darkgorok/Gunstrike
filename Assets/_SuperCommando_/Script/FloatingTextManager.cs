using UnityEngine;
using VContainer;

public class FloatingTextManager : MonoBehaviour
{
    [Header("Floating Text")]
    public GameObject FloatingText;

    private ICameraRigService cameraRigService;
    private IMenuFlowService menuFlowService;

    [Inject]
    public void Construct(ICameraRigService cameraRigService, IMenuFlowService menuFlowService)
    {
        this.cameraRigService = cameraRigService;
        this.menuFlowService = menuFlowService;
    }

    void Awake()
    {
        ProjectScope.Inject(this);
    }

    public void ShowText(FloatingTextParameter para, Vector2 ownerPosition)
    {
        if (FloatingText == null)
        {
            Debug.LogError("Need place FloatingText to GameManage object");
            return;
        }

        GameObject floatingText = SpawnSystemHelper.GetNextObject(FloatingText, false);
        Vector3 position = cameraRigService.WorldToScreenPoint(para.localTextOffset + ownerPosition);

        floatingText.transform.SetParent(menuFlowService.UiRoot, false);
        floatingText.transform.position = position;

        var floatingTextView = floatingText.GetComponent<FloatingText>();
        floatingTextView.SetText(para.message, para.textColor, para.localTextOffset + ownerPosition);
        floatingText.SetActive(true);
    }
}

[System.Serializable]
public class FloatingTextParameter
{
    [Header("Display Text Message")]
    public string message = "MESSAGE";
    public Vector2 localTextOffset = new Vector2(0, 1);
    public Color textColor = Color.yellow;
}

using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class FloatingText : MonoBehaviour
{
    public Text floatingText;

    Vector2 currentPos;
    private ICameraRigService cameraRigService;

    [Inject]
    public void Construct(ICameraRigService cameraRigService)
    {
        this.cameraRigService = cameraRigService;
    }

    private void Awake()
    {
        ProjectScope.Inject(this);
    }

    public void SetText(string text, Color color)
    {
        floatingText.color = color;
        floatingText.text = text;
    }

    public void SetText(string text, Color color, Vector2 worldPos)
    {
        floatingText.color = color;
        floatingText.text = text;
        currentPos = worldPos;
    }

    void Update()
    {
        Vector3 position = cameraRigService.WorldToScreenPoint(currentPos);
        floatingText.transform.position = position;
    }
}

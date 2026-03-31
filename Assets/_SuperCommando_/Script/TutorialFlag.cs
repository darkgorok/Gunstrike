using UnityEngine;
using VContainer;

public class TutorialFlag : MonoBehaviour
{
    public Sprite tutorialSprite;

    private ITutorialOverlayService tutorialOverlayService;

    [Inject]
    public void Construct(ITutorialOverlayService tutorialOverlayService)
    {
        this.tutorialOverlayService = tutorialOverlayService;
    }

    private void Awake()
    {
        ProjectScope.Inject(this);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.GetComponent<Player>() == null)
            return;

        tutorialOverlayService.Open(tutorialSprite);
        GetComponent<BoxCollider2D>().enabled = false;
    }
}

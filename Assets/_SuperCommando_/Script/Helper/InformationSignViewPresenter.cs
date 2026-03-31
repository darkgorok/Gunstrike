using System.Collections;
using UnityEngine;

public sealed class InformationSignViewPresenter : IInformationSignView
{
    private readonly MonoBehaviour runner;
    private readonly SpriteRenderer signSpriteRenderer;
    private readonly GameObject infoContainer;
    private readonly GameObject mobileTutorial;
    private readonly GameObject desktopTutorial;
    private readonly float fadeDuration;

    private readonly Color visibleColor;
    private readonly Color hiddenColor;

    private Coroutine fadeRoutine;

    public InformationSignViewPresenter(MonoBehaviour runner, InformationSignViewConfig config)
    {
        this.runner = runner;
        signSpriteRenderer = config.SignSpriteRenderer;
        infoContainer = config.InfoContainer;
        mobileTutorial = config.MobileTutorial;
        desktopTutorial = config.DesktopTutorial;
        fadeDuration = config.FadeDuration;

        visibleColor = signSpriteRenderer != null ? signSpriteRenderer.color : Color.white;
        hiddenColor = new Color(visibleColor.r, visibleColor.g, visibleColor.b, 0f);
    }

    public void ApplyPlatformTutorialState()
    {
#if UNITY_ANDROID || UNITY_IOS
        SetOptionalObjectActive(mobileTutorial, true);
        SetOptionalObjectActive(desktopTutorial, false);
#else
        SetOptionalObjectActive(mobileTutorial, false);
        SetOptionalObjectActive(desktopTutorial, true);
#endif
    }

    public void Show()
    {
        SetInfoVisible(true);
        FadeTo(visibleColor);
    }

    public void Hide()
    {
        SetInfoVisible(false);
        FadeTo(hiddenColor);
    }

    public void HideImmediate()
    {
        SetInfoVisible(false);

        if (signSpriteRenderer == null)
            return;

        StopFade();
        signSpriteRenderer.color = hiddenColor;
    }

    private void SetInfoVisible(bool visible)
    {
        if (infoContainer != null)
            infoContainer.SetActive(visible);
    }

    private void FadeTo(Color targetColor)
    {
        if (signSpriteRenderer == null)
            return;

        StopFade();
        fadeRoutine = runner.StartCoroutine(FadeRoutine(targetColor));
    }

    private IEnumerator FadeRoutine(Color targetColor)
    {
        yield return MMFade.FadeSpriteRenderer(signSpriteRenderer, fadeDuration, targetColor);
        fadeRoutine = null;
    }

    private void StopFade()
    {
        if (fadeRoutine == null)
            return;

        runner.StopCoroutine(fadeRoutine);
        fadeRoutine = null;
    }

    private static void SetOptionalObjectActive(GameObject target, bool active)
    {
        if (target != null)
            target.SetActive(active);
    }
}

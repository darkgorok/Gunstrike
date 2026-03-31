using UnityEngine;
using UnityEngine.UI;

public static class LoadingScreenViewResolver
{
    public static ILoadingScreenView Resolve(GameObject loadingObject, Slider progressSlider = null, Text progressText = null)
    {
        if (loadingObject != null)
        {
            var loadingView = loadingObject.GetComponent<LoadingScreenView>();
            if (loadingView != null)
                return loadingView;

            loadingView = loadingObject.GetComponentInChildren<LoadingScreenView>(true);
            if (loadingView != null)
                return loadingView;
        }

        if (loadingObject == null && progressSlider == null && progressText == null)
            return null;

        return new FallbackLoadingScreenView(loadingObject, progressSlider, progressText);
    }
}

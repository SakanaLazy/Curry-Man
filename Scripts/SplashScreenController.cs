using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SplashScreenController : MonoBehaviour
{
    [Header("References")]
    public CanvasGroup logoCanvas; // assign your logo’s CanvasGroup here

    [Header("Timing Settings")]
    public float fadeInDuration = 1.5f;
    public float holdDuration = 1.0f;
    public float fadeOutDuration = 1.5f;
    public float waitBeforeLoad = 0.3f;

    [Header("Next Scene")]
    public string nextSceneName = "CurryCity"; // name of the next scene

    private void Start()
    {
        StartCoroutine(PlaySplash());
    }

    private IEnumerator PlaySplash()
    {
        // Fade In
        float t = 0f;
        while (t < fadeInDuration)
        {
            t += Time.deltaTime;
            logoCanvas.alpha = Mathf.Lerp(0f, 1f, t / fadeInDuration);
            yield return null;
        }

        // Hold
        yield return new WaitForSeconds(holdDuration);

        // Fade Out
        t = 0f;
        while (t < fadeOutDuration)
        {
            t += Time.deltaTime;
            logoCanvas.alpha = Mathf.Lerp(1f, 0f, t / fadeOutDuration);
            yield return null;
        }

        // Wait before scene transition
        yield return new WaitForSeconds(waitBeforeLoad);

        SceneManager.LoadScene(nextSceneName);
    }
}

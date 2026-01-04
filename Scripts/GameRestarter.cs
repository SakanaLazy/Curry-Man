using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Diagnostics;

public class GameRestartManager : MonoBehaviour
{
    public static GameRestartManager instance;

    [Header("Restart Settings")]
    public float restartDelay = 2.0f;

    [Header("Fade Settings")]
    public CanvasGroup fadeCanvasGroup;
    public float fadeDuration = 1.0f;

    private int totalEnemies;
    private int defeatedEnemies;
    private bool restarting = false;

    void Awake()
    {
        instance = this;
        CountEnemies();

        if (fadeCanvasGroup != null)
            fadeCanvasGroup.alpha = 0;
    }

    void CountEnemies()
    {
        // ✅ Correct Unity 6.2 syntax
        var enemies = Object.FindObjectsByType<HealthSystem>(FindObjectsSortMode.None);

        totalEnemies = 0;
        foreach (var e in enemies)
        {
            if (!e.isPlayer)
                totalEnemies++;
        }

        UnityEngine.Debug.Log($"[RestartManager] Found {totalEnemies} enemies in scene.");
    }

    public void OnCharacterDied(HealthSystem hs)
    {
        if (restarting) return;

        if (hs.isPlayer)
        {
            UnityEngine.Debug.Log("[RestartManager] Player died → restarting whole game.");
            StartCoroutine(RestartEntireGame());
            return;
        }

        if (!hs.isPlayer)
        {
            defeatedEnemies++;
            UnityEngine.Debug.Log($"[RestartManager] Enemy defeated {defeatedEnemies}/{totalEnemies}.");

            if (defeatedEnemies >= totalEnemies)
            {
                UnityEngine.Debug.Log("[RestartManager] All enemies defeated → restarting whole game.");
                StartCoroutine(RestartEntireGame());
            }
        }
    }

    private IEnumerator RestartEntireGame()
    {
        restarting = true;

        yield return new WaitForSeconds(restartDelay);

        // Fade to black if available
        if (fadeCanvasGroup != null)
        {
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                fadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
                yield return null;
            }
        }

        yield return new WaitForSeconds(0.2f);

        string exePath = Process.GetCurrentProcess().MainModule.FileName;
        Process.Start(exePath);
        Application.Quit();
    }
}

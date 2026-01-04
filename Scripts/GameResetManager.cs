using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class GameResetManager : MonoBehaviour
{
    public static GameResetManager instance;

    [Header("Scene Names")]
    public string splashSceneName = "SplashScreen";
    public string gameSceneName = "CurryCity";

    [Header("Delays")]
    public float delayBeforeReset = 3f;

    private List<HealthSystem> enemies = new List<HealthSystem>();
    private bool resetting = false;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        // Collect all NPCs (non-player HealthSystems)

        enemies.AddRange(FindObjectsByType<HealthSystem>(FindObjectsSortMode.None));

        // Filter only enemies
        enemies.RemoveAll(h => h.isPlayer);
    }

    public void OnCharacterDied(HealthSystem hs)
    {
        if (resetting) return;

        // If the player dies
        if (hs.isPlayer)
        {
            Debug.Log("[GameResetManager] Player KO — restarting game soon...");
            StartCoroutine(ResetGame());
        }
        else
        {
            // Remove NPC from the list
            enemies.Remove(hs);
            Debug.Log($"[GameResetManager] NPC defeated. Remaining enemies: {enemies.Count}");

            // If all NPCs are gone
            if (enemies.Count == 0)
            {
                Debug.Log("[GameResetManager] All enemies defeated — restarting game...");
                StartCoroutine(ResetGame());
            }
        }
    }

    private IEnumerator ResetGame()
    {
        resetting = true;
        yield return new WaitForSeconds(delayBeforeReset);

        // Reload splash scene
        SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);

    }
}

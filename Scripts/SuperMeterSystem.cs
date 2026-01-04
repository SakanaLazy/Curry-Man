using UnityEngine;

public class SuperMeterSystem : MonoBehaviour
{
    [Header("Settings")]
    public int hitsToFill = 30;

    [Header("References")]
    public SuperMeterUI ui;
    public SuperController superControl;

    private int currentHits = 0;

    void Start()
    {
        // 🔒 Lock supers immediately on startup
        if (superControl)
            superControl.enabled = false;

        UpdateUI();
    }

    void OnEnable()
    {
        HitDetector.OnHitLanded += AddHit;
    }

    void OnDisable()
    {
        HitDetector.OnHitLanded -= AddHit;
    }

    private void AddHit(int totalHits)
    {
        currentHits = Mathf.Min(currentHits + 1, hitsToFill);
        UpdateUI();

        if (currentHits >= hitsToFill)
        {
            UnlockSuper();
        }
    }

    private void UnlockSuper()
    {
        if (superControl)
        {
            superControl.enabled = true;
            Debug.Log("Super Ready!");
        }
    }

    private void UpdateUI()
    {
        if (ui)
            ui.SetFill((float)currentHits / hitsToFill);
    }

    public void ResetMeter()
    {
        currentHits = 0;
        UpdateUI();

        if (superControl)
            superControl.enabled = false;
    }
}

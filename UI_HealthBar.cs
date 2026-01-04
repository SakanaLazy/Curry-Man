using UnityEngine;
using UnityEngine.UI;

public class UI_HealthBar : MonoBehaviour
{
    [Header("References")]
    public Image fillImage;              // The colored fill bar
    public HealthSystem target;          // The player or NPC with HealthSystem

    [Header("Animation Settings")]
    [Tooltip("How quickly the bar catches up to new HP")]
    public float smoothSpeed = 5f;

    [Tooltip("Colors from green → yellow → red")]
    public Gradient healthColor;

    private float currentFill = 1f;      // What we’re currently showing
    private float targetFill = 1f;       // Where we want to go

    void Start()
    {
        // Hook into the health system’s event
        if (target != null)
        {
            target.OnHealthChanged += OnHealthChanged;
            OnHealthChanged(target.maxHealth, target.maxHealth); // start full
        }

        if (fillImage != null)
            fillImage.color = healthColor.Evaluate(1f);
    }

    void OnDestroy()
    {
        if (target != null)
            target.OnHealthChanged -= OnHealthChanged;
    }

    private void OnHealthChanged(float current, float max)
    {
        if (max <= 0) return;
        targetFill = current / max;
    }

    void Update()
    {
        // Smoothly move toward the target fill
        currentFill = Mathf.Lerp(currentFill, targetFill, Time.deltaTime * smoothSpeed);

        if (fillImage != null)
        {
            fillImage.fillAmount = currentFill;
            fillImage.color = healthColor.Evaluate(currentFill);
        }
    }
}

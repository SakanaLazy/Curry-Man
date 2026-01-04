using UnityEngine;

public class PlayerKOTrigger : MonoBehaviour
{
    [Header("References")]
    public Animator animator;

    [Header("Settings")]
    [Tooltip("Name of the trigger parameter for KO animation")]
    public string koTriggerName = "KnockOut";

    [Tooltip("Key to trigger the KO manually (for testing)")]
    public KeyCode triggerKey = KeyCode.K;

    void Start()
    {
        // Auto-assign Animator if not set
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Press the trigger key to play KO animation manually
        if (Input.GetKeyDown(triggerKey) && animator != null)
        {
            Debug.Log($"[KO Trigger] Fired {koTriggerName} on {name}");
            animator.SetTrigger(koTriggerName);
            animator.Play("KnockOut", 0, 0f);
        }
    }
}

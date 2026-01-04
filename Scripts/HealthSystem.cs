using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class HealthSystem : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("References")]
    public Animator animator; // single global Animator reference

    [Header("Status Flags")]
    public bool isPlayer = false;
    public bool enableHitReactions = true;
    [HideInInspector] public bool isDead = false;

    [Header("Animation Triggers")]
    public string deathTrigger = "KnockOut";
    public string hitTrigger = "HitReact";

    // === UI EVENT (unchanged from v3.2) ===
    public delegate void HealthChanged(float current, float max);
    public event HealthChanged OnHealthChanged;

    private bool koSequenceStarted = false;

    void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        int before = currentHealth;
        currentHealth = Mathf.Max(0, before - damage);
        Debug.Log($"{name} HP: {before} → {currentHealth}");

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        // Only NPCs do quick hit reacts now
        if (currentHealth > 0)
        {
            if (enableHitReactions && !isPlayer)
                StartCoroutine(PlayHitReaction());
        }
        else if (!koSequenceStarted)
        {
            StartCoroutine(DelayedKO());
        }
    }

    private IEnumerator PlayHitReaction()
    {
        if (animator == null) yield break;

        animator.ResetTrigger(hitTrigger);
        animator.SetTrigger(hitTrigger);
        Debug.Log($"{name} plays {hitTrigger}");

        // NPC reactions faster
        animator.speed = 2.5f;
        yield return new WaitForSeconds(0.25f);
        animator.speed = 1f;
    }

    private IEnumerator DelayedKO()
    {
        koSequenceStarted = true;
        isDead = true;

        yield return new WaitForSeconds(0.3f);
        StartCoroutine(Die());
        AudioManager.instance.PlayKO();
    }

    private IEnumerator Die()
    {
        Debug.Log($"{name} entering KO sequence");

        if (animator != null && !string.IsNullOrEmpty(deathTrigger))
        {
            animator.ResetTrigger(deathTrigger);
            animator.SetTrigger(deathTrigger);
        }

        yield return new WaitForSeconds(0.2f);

        DisableIfExists<PlayerController>();
        DisableIfExists<CombatController>();
        DisableIfExists<NPCChase>();
        DisableIfExists<NPCCombat>();

        if (TryGetComponent(out NavMeshAgent agent))
        {
            if (agent.enabled && agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.ResetPath();
                agent.enabled = false;
            }
        }

        if (TryGetComponent(out Rigidbody rb))
        {
#if UNITY_6000_OR_NEWER
            rb.linearVelocity = Vector3.zero;
#else
            rb.linearVelocity = Vector3.zero;
#endif
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        yield return new WaitForSeconds(2.5f);
        Debug.Log($"{name} KO complete — HealthSystem still readable by UI.");

        // Notify Restart Manager
        if (GameRestartManager.instance != null)
            GameRestartManager.instance.OnCharacterDied(this);


    }

    private void DisableIfExists<T>() where T : Behaviour
    {
        if (TryGetComponent(out T comp))
            comp.enabled = false;
    }
}

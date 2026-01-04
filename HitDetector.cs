using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class HitDetector : MonoBehaviour
{
    public static int successfulHits = 0;           // counts total hits landed
    public static System.Action<int> OnHitLanded;   // event to notify UI
    [Header("Hit Settings")]
    public int Damage = 10;
    public bool isPlayerAttack = true;   // true = Player hits Enemy, false = Enemy hits Player

    [Header("Activation (Animator-State Driven)")]
    [Tooltip("Names of Animator states during which this hitbox should be ACTIVE (e.g., Punch1, Punch2, Kick1, Super1).")]
    public string[] attackStateNames = new string[] { "Punch1", "Punch2", "Punch3", "Kick1", "Kick2", "Kick3" };

    [Tooltip("Animator layer index to watch (usually 0).")]
    public int animatorLayerIndex = 0;

    [Tooltip("Normalized time window within the attack state when the hitbox is active. Leave 0..1 for whole clip.")]
    [Range(0f, 1f)] public float activeStartNormalized = 0f;
    [Range(0f, 1f)] public float activeEndNormalized = 1f;

    private Transform ownerRoot;
    private Collider hitboxCollider;
    private Animator ownerAnimator;
    private int[] attackStateHashes;

    void Awake()
    {
        ownerRoot = transform.root;
        hitboxCollider = GetComponent<Collider>();
        if (hitboxCollider == null)
            Debug.LogError($"{name}: No Collider found. Add a Sphere/Box/Capsule collider.");

        // Ensure trigger collider
        hitboxCollider.isTrigger = true;
        hitboxCollider.enabled = false; // start OFF

        // Find animator on the owner
        ownerAnimator = ownerRoot.GetComponentInChildren<Animator>();
        if (ownerAnimator == null)
            Debug.LogError($"{name}: No Animator found on {ownerRoot.name} or its children.");

        RebuildHashes();

        // Ensure Player↔Enemy collisions are not ignored (defensive, survives Unity 6.2 UI changes)
        int playerLayer = LayerMask.NameToLayer("Player");
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (playerLayer >= 0 && enemyLayer >= 0)
        {
            Physics.IgnoreLayerCollision(playerLayer, enemyLayer, false);
        }
    }

    void OnValidate()
    {
        // Keep it a trigger in editor too
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
        RebuildHashes();
    }

    private void RebuildHashes()
    {
        attackStateHashes = (attackStateNames == null)
            ? new int[0]
            : attackStateNames.Select(n => Animator.StringToHash(n)).ToArray();
    }

    void Update()
    {
        if (ownerAnimator == null || hitboxCollider == null) return;

        bool shouldEnable = IsInAttackWindow();

        // Toggle only when needed (avoids spam)
        if (hitboxCollider.enabled != shouldEnable)
            hitboxCollider.enabled = shouldEnable;
    }

    private bool IsInAttackWindow()
    {
        // Current state
        var state = ownerAnimator.GetCurrentAnimatorStateInfo(animatorLayerIndex);
        if (MatchesAttackStateAndTime(state)) return true;

        // If transitioning, also consider the next state
        if (ownerAnimator.IsInTransition(animatorLayerIndex))
        {
            var next = ownerAnimator.GetNextAnimatorStateInfo(animatorLayerIndex);
            if (MatchesAttackStateAndTime(next)) return true;
        }

        return false;
    }

    private bool MatchesAttackStateAndTime(AnimatorStateInfo info)
    {
        if (attackStateHashes == null || attackStateHashes.Length == 0) return false;

        // Match by shortNameHash
        bool stateMatches = attackStateHashes.Contains(info.shortNameHash);
        if (!stateMatches) return false;

        // Normalized time cycles; clamp to [0,1] for window checks
        float t = info.normalizedTime % 1f;
        if (activeStartNormalized <= activeEndNormalized)
            return t >= activeStartNormalized && t <= activeEndNormalized;

        // If someone inverts the window by mistake, just treat as always-on during the state
        return true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!hitboxCollider.enabled) return;
        if (other.isTrigger) return; // ignore trigger-trigger touches

        // Determine true root of the thing we hit
        Transform targetRoot = other.attachedRigidbody
            ? other.attachedRigidbody.transform.root
            : other.transform.root;

        // Ignore self
        if (targetRoot == ownerRoot) return;

        // Tag filtering
        bool validTarget =
            (isPlayerAttack && targetRoot.CompareTag("Enemy")) ||
            (!isPlayerAttack && targetRoot.CompareTag("Player"));

        if (!validTarget) return;

        // Find HealthSystem
        HealthSystem health = targetRoot.GetComponent<HealthSystem>()
            ?? targetRoot.GetComponentInChildren<HealthSystem>()
            ?? other.GetComponentInParent<HealthSystem>();

        if (health == null || health.isDead) return;

        Debug.Log($"{name} hit {targetRoot.name} — {Damage} dmg");
        health.TakeDamage(Damage);
        AudioManager.instance.PlayHit();


        if (isPlayerAttack)
        {
            successfulHits++;
            OnHitLanded?.Invoke(successfulHits);
        }
    }

    private void OnDisable()
    {
        if (hitboxCollider) hitboxCollider.enabled = false;
    }
}

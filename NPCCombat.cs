using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class NPCCombat : MonoBehaviour
{
    [Header("References")]
    public Animator anim;                 // assign in Inspector (or auto-find)
    public Transform player;              // assign or auto-find by tag "Player"

    [Header("Attack Settings")]
    public float attackRange = 2.0f;      // distance to start attacking
    public float minAttackDelay = 0.8f;   // random wait min between attacks
    public float maxAttackDelay = 2.0f;   // random wait max between attacks
    public bool useRandomDelay = true;
    public float fixedDelay = 1.2f;       // used if useRandomDelay == false

    [Header("Animation Parameters (names must match Animator)")]
    public string punchTriggerName = "Punch";   // Trigger
    public string punchTypeName = "PunchType";  // Int (1 or 2)

    [Header("Safety / Tuning")]
    public float triggerFailSafeTime = 1.2f;    // if animation doesn't start in this time, bail
    public float faceSpeed = 10f;               // how fast to rotate toward player

    // internal
    private bool isAttacking = false;
    private Coroutine attackRoutine;

    void Awake()
    {
        if (anim == null) anim = GetComponentInChildren<Animator>();
        if (player == null)
        {
            var pgo = GameObject.FindGameObjectWithTag("Player");
            if (pgo != null) player = pgo.transform;
        }
    }

    void Update()
    {
        if (player == null) return;

        float d = Vector3.Distance(transform.position, player.position);
        bool inRange = d <= attackRange;

        // start attacking if in range and not already
        if (inRange && !isAttacking)
        {
            attackRoutine = StartCoroutine(AttackLoop());
        }
        // stop attacking if player left
        else if (!inRange && isAttacking)
        {
            if (attackRoutine != null) StopCoroutine(attackRoutine);
            isAttacking = false;
        }
    }

    IEnumerator AttackLoop()
    {
        isAttacking = true;

        while (true)
        {
            // If player moved out of range, stop
            if (player == null || Vector3.Distance(transform.position, player.position) > attackRange)
                break;

            // Choose delay
            float delay = useRandomDelay ? Random.Range(minAttackDelay, maxAttackDelay) : fixedDelay;
            yield return new WaitForSeconds(delay);

            // Face the player smoothly (one frame to orient)
            yield return StartCoroutine(FacePlayerForAttack());

            // Choose punch type 1 or 2 at random
            int chosen = Random.Range(1, 3); // returns 1 or 2
            anim.SetInteger(punchTypeName, chosen);

            // Frame-sync before firing trigger to avoid animator-desync
            yield return new WaitForEndOfFrame();
            ResetPunchTriggersSafely();
            anim.SetTrigger(punchTriggerName);
            yield return null; // let animator update

            // Wait until the animator reports the corresponding state, with timeout
            string expectedStateName = "Punch" + chosen; // matches state names in your animator
            float timer = 0f;
            bool started = false;
            while (timer < triggerFailSafeTime)
            {
                var state = anim.GetCurrentAnimatorStateInfo(0);
                if (state.IsName(expectedStateName))
                {
                    started = true;
                    break;
                }
                timer += Time.deltaTime;
                yield return null;
            }

            if (!started)
            {
                Debug.LogWarning($"[NPCCombat] {name}: punch animation '{expectedStateName}' didn't start. Recovering.");
                // graceful recovery - clear triggers and continue loop
                ResetPunchTriggersSafely();
                yield return null;
                continue;
            }

            // Wait until animation reaches near the end so NPC doesn't instantly chain weirdly
            AnimatorStateInfo running = anim.GetCurrentAnimatorStateInfo(0);
            while (running.normalizedTime < 0.95f)
            {
                yield return null;
                running = anim.GetCurrentAnimatorStateInfo(0);
            }

            // After the attack finishes, continue loop (will wait the delay again)
        }

        isAttacking = false;
    }

    IEnumerator FacePlayerForAttack()
    {
        if (player == null) yield break;

        // compute flat direction toward player
        Vector3 dir = (player.position - transform.position);
        dir.y = 0;
        if (dir.sqrMagnitude < 0.001f) yield break;

        Quaternion target = Quaternion.LookRotation(dir);
        // rotate quickly over a couple frames (smooth)
        float t = 0f;
        while (Quaternion.Angle(transform.rotation, target) > 1f && t < 0.25f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * faceSpeed);
            t += Time.deltaTime;
            yield return null;
        }
        transform.rotation = target;
        yield return null;
    }

    // Clear both punch triggers (safety). If you only have one trigger, this is harmless.
    void ResetPunchTriggersSafely()
    {
        // reset any triggers named Punch (if you have multiple, adapt)
        anim.ResetTrigger(punchTriggerName);
    }

    // for debug: draw attack range in Scene view
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}

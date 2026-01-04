using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuperController : MonoBehaviour
{
    [Header("References")]
    public Animator animator;
    public HitDetector[] hitDetectors;
    public CombatController combatController;
    public PlayerController playerController;

    [Header("Super Triggers")]
    public string super1Trigger = "Super1";
    public string super2Trigger = "Super2";

    [Header("Animator States")]
    public string super1State = "Super1";
    public string super1MirrorState = "Super1_Mirror";
    public string super2State = "Super2";
    public string super2MirrorState = "Super2_Mirror";

    [Header("Mirror Parameter")]
    public bool useMirrorParam = true;
    public string mirrorParam = "Mirror";

    [Header("Timing & Damage")]
    public float lockFallbackSeconds = 2.0f;
    public float damageMultiplier = 2.0f;
    public float overlapRadius = 1.0f;      // distance check for manual damage

    private bool isSuper;
    public GameObject superBurstPrefab; //ParticleFX for Super
    public Transform fxSpawnPoint;

    void Awake()
    {
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!combatController) combatController = GetComponent<CombatController>();
        if (!playerController) playerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        if (isSuper) return;

        if (Input.GetKeyDown(KeyCode.Q))
            StartCoroutine(DoSuper(1));
        else if (Input.GetKeyDown(KeyCode.E))
            StartCoroutine(DoSuper(2));
    }

    public IEnumerator DoSuper(int index)
    {
        isSuper = true;
        if (combatController) combatController.enabled = false;
        if (playerController) playerController.enabled = false;

        bool mirror = Random.value > 0.5f;
        if (useMirrorParam && animator) animator.SetBool(mirrorParam, mirror);

        string trig = (index == 1) ? super1Trigger : super2Trigger;
        string stateA = (index == 1) ? super1State : super2State;
        string stateB = (index == 1) ? super1MirrorState : super2MirrorState;
        int hashA = Animator.StringToHash(stateA);
        int hashB = Animator.StringToHash(stateB);

        // Store original damage
        int[] original = new int[hitDetectors.Length];
        for (int i = 0; i < hitDetectors.Length; i++)
        {
            if (!hitDetectors[i]) continue;
            original[i] = hitDetectors[i].Damage;
            hitDetectors[i].Damage = Mathf.RoundToInt(hitDetectors[i].Damage * damageMultiplier);
        }

        animator.ResetTrigger(trig);
        animator.SetTrigger(trig);

        // 🔥 Spawn FX immediately after animation trigger
        SpawnSuperFX();

        float startTime = Time.time;
        bool started = false;
        float timeout = lockFallbackSeconds + 1f;

        // === Super active loop ===
        while (animator && Time.time - startTime < timeout)
        {
            var info = animator.GetCurrentAnimatorStateInfo(0);
            var next = animator.IsInTransition(0) ? animator.GetNextAnimatorStateInfo(0) : info;

            if (!started && (info.shortNameHash == hashA || info.shortNameHash == hashB ||
                             next.shortNameHash == hashA || next.shortNameHash == hashB))
                started = true;

            bool inSuper = started && (info.shortNameHash == hashA || info.shortNameHash == hashB ||
                                       next.shortNameHash == hashA || next.shortNameHash == hashB);

            if (!inSuper && started) break;

            // === Manual overlap check while super plays ===
            foreach (var hd in hitDetectors)
            {
                if (!hd) continue;
                Collider col = hd.GetComponent<Collider>();
                if (!col) continue;

                Vector3 pos = col.bounds.center;
                Collider[] hits = Physics.OverlapSphere(pos, overlapRadius);

                foreach (var other in hits)
                {
                    if (other.isTrigger) continue;
                    Transform targetRoot = other.attachedRigidbody
                        ? other.attachedRigidbody.transform.root
                        : other.transform.root;

                    if (targetRoot == transform.root) continue; // ignore self
                    bool valid =
                        (hd.isPlayerAttack && targetRoot.CompareTag("Enemy")) ||
                        (!hd.isPlayerAttack && targetRoot.CompareTag("Player"));

                    if (!valid) continue;

                    var health = targetRoot.GetComponent<HealthSystem>()
                                 ?? targetRoot.GetComponentInChildren<HealthSystem>();
                    if (health != null && !health.isDead)
                    {
                        Debug.Log($"[SUPER] {name} hits {targetRoot.name} for {hd.Damage}");
                        health.TakeDamage(hd.Damage);
                        AudioManager.instance.PlaySuper();
                    }
                }
            }

            yield return null;
        }

        // Restore damage
        for (int i = 0; i < hitDetectors.Length; i++)
        {
            if (!hitDetectors[i]) continue;
            hitDetectors[i].Damage = original[i];
        }

        if (useMirrorParam && animator) animator.SetBool(mirrorParam, false);
        if (playerController) playerController.enabled = true;
        if (combatController) combatController.enabled = true;
        isSuper = false;
        var meter = Object.FindAnyObjectByType<SuperMeterSystem>();
        if (meter) meter.ResetMeter();
    }

    private void SpawnSuperFX()
    {
        if (superBurstPrefab == null)
            return;

        Vector3 spawnPos = fxSpawnPoint ? fxSpawnPoint.position : transform.position + Vector3.up * 1.2f;
        Quaternion spawnRot = Quaternion.identity;
        GameObject fx = Instantiate(superBurstPrefab, spawnPos, spawnRot);

        // Ensure particles play even if Play on Awake is disabled
        var ps = fx.GetComponent<ParticleSystem>();
        if (ps != null)
            ps.Play();

        Destroy(fx, 2f);
    }
}

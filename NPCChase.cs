using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(NPCCombat))]
public class NPCChase : MonoBehaviour
{
    public Transform player;
    public float chaseRange = 10f;   // start moving when closer than this
    public float stopRange = 2.2f;   // distance to stop and punch
    public float updateRate = 0.2f;  // how often to refresh path

    private NavMeshAgent agent;
    private NPCCombat combat;
    private Animator anim;
    private HealthSystem health;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        combat = GetComponent<NPCCombat>();
        anim = GetComponentInChildren<Animator>();
        health = GetComponent<HealthSystem>();

        if (player == null)
        {
            GameObject pgo = GameObject.FindGameObjectWithTag("Player");
            if (pgo) player = pgo.transform;
        }

        InvokeRepeating(nameof(UpdateChase), 0, updateRate);
    }

    void UpdateChase()
    {
        // stop logic if agent is dead or invalid
        if (health != null && health.isDead)
        {
            if (agent != null && agent.enabled && agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.ResetPath();
                agent.enabled = false;
            }
            anim.SetBool("isWalking", false);
            if (combat.enabled) combat.enabled = false;
            return;
        }

        if (agent == null || !agent.enabled || !agent.isOnNavMesh || player == null)
            return;

        float dist = Vector3.Distance(transform.position, player.position);

        // CHASE
        if (dist <= chaseRange && dist > stopRange)
        {
            try
            {
                agent.isStopped = false;
                agent.SetDestination(player.position);
            }
            catch { return; }

            anim.SetBool("isWalking", true);
            if (combat.enabled) combat.enabled = false;
        }
        // STOP & PUNCH
        else if (dist <= stopRange)
        {
            if (agent.enabled && agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.ResetPath();
            }

            anim.SetBool("isWalking", false);

            // face player
            Vector3 dir = player.position - transform.position;
            dir.y = 0;
            if (dir.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(dir),
                    Time.deltaTime * 10f
                );

            if (!combat.enabled) combat.enabled = true;
        }
        // TOO FAR AWAY
        else
        {
            if (agent.enabled && agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.ResetPath();
            }
            anim.SetBool("isWalking", false);
            if (combat.enabled) combat.enabled = false;
        }
    }
}

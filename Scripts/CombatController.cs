using System.Collections;
using UnityEngine;

public class CombatController : MonoBehaviour
{
    [Header("References")]
    public Animator animator;
    public PlayerController playerController;

    [Header("Combo Settings")]
    public float comboResetTime = 0.9f;
    public float comboInputWindow = 0.45f;
    private int comboStep = 0;
    private bool canQueueNext = false;
    private bool isAttacking = false;
    private Coroutine comboCoroutine;

    [Header("Animation Triggers")]
    public string[] punchTriggers = { "Punch1", "Punch2", "Punch3" };
    public string[] kickTriggers = { "Kick1", "Kick2", "Kick3" };

    [Header("Input Buffer Settings")]
    public float bufferDuration = 0.4f;
    private bool bufferedPunch = false;
    private bool bufferedKick = false;
    private float punchBufferTimer = 0f;
    private float kickBufferTimer = 0f;

    [Header("Anti-Spam Click Limit")]
    public int maxClicksPerSequence = 3;  // Limit to triple input
    public float clickWindow = 0.5f;      // Time window to count clicks
    private int clickCount = 0;
    private float clickTimer = 0f;

    void Awake()
    {
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!playerController) playerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        HandleBufferedInput();

        // Handle click window timing
        if (clickTimer > 0)
        {
            clickTimer -= Time.deltaTime;
            if (clickTimer <= 0)
                clickCount = 0; // Reset count after the window ends
        }
    }

    private void HandleBufferedInput()
    {
        // Count clicks within window
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            clickCount++;
            clickTimer = clickWindow;

            if (clickCount > maxClicksPerSequence)
                return; // Ignore clicks beyond triple
        }

        // Capture input into buffer
        if (Input.GetMouseButtonDown(0))
        {
            bufferedPunch = true;
            punchBufferTimer = bufferDuration;
        }

        if (Input.GetMouseButtonDown(1))
        {
            bufferedKick = true;
            kickBufferTimer = bufferDuration;
        }

        // Process buffered inputs
        if (!isAttacking)
        {
            if (bufferedPunch)
            {
                bufferedPunch = false;
                StartCombo(punchTriggers);
            }
            else if (bufferedKick)
            {
                bufferedKick = false;
                StartCombo(kickTriggers);
            }
        }
        else if (canQueueNext)
        {
            if (bufferedPunch)
            {
                bufferedPunch = false;
                QueueNextAttack(punchTriggers);
            }
            else if (bufferedKick)
            {
                bufferedKick = false;
                QueueNextAttack(kickTriggers);
            }
        }

        // Countdown buffer timers
        if (punchBufferTimer > 0)
        {
            punchBufferTimer -= Time.deltaTime;
            if (punchBufferTimer <= 0) bufferedPunch = false;
        }

        if (kickBufferTimer > 0)
        {
            kickBufferTimer -= Time.deltaTime;
            if (kickBufferTimer <= 0) bufferedKick = false;
        }
    }

    private void StartCombo(string[] triggers)
    {
        comboStep = 0;
        if (comboCoroutine != null)
            StopCoroutine(comboCoroutine);
        comboCoroutine = StartCoroutine(DoCombo(triggers));
    }

    private void QueueNextAttack(string[] triggers)
    {
        comboStep++;
        if (comboCoroutine != null)
            StopCoroutine(comboCoroutine);
        comboCoroutine = StartCoroutine(DoCombo(triggers));
    }

    private IEnumerator DoCombo(string[] triggers)
    {
        isAttacking = true;
        canQueueNext = false;
        if (playerController)
            playerController.enabled = false;

        comboStep = Mathf.Clamp(comboStep, 0, triggers.Length - 1);
        string trigger = triggers[comboStep];
        animator.ResetTrigger(trigger);
        animator.SetTrigger(trigger);

        float clipLength = GetAnimationLength(trigger);
        float elapsed = 0f;

        while (elapsed < clipLength)
        {
            elapsed += Time.deltaTime;

            if (!canQueueNext && elapsed > clipLength - comboInputWindow)
                canQueueNext = true;

            if ((bufferedPunch || bufferedKick) && elapsed > clipLength - comboInputWindow)
                canQueueNext = true;

            yield return null;
        }

        if (!canQueueNext)
            comboStep = 0;

        canQueueNext = false;
        isAttacking = false;

        if (playerController)
            playerController.enabled = true;

        yield return new WaitForSeconds(comboResetTime);
        comboStep = 0;

        // Reset click count after combo
        clickCount = 0;
        clickTimer = 0f;
    }

    private float GetAnimationLength(string trigger)
    {
        if (!animator || animator.runtimeAnimatorController == null)
            return 0.6f;

        foreach (var clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name.Contains(trigger))
                return clip.length;
        }

        return 0.6f;
    }
}

using System.Collections;
using UnityEngine;

/// <summary>
/// PawnCombat (rewrite)
/// - 3-hit light combo with strict timing windows
/// - Drives Animator (Attack trigger + ComboStep int + isAttacking bool)
/// - Toggles sword hitbox (Trigger collider) during the active window
/// </summary>
public class PawnCombat : MonoBehaviour
{
    [Header("Refs")]
    [Tooltip("Trigger collider on the sword blade (IsTrigger = ON).")]
    [SerializeField] private Collider swordHitbox;
    [Tooltip("Animator on the pawn root.")]
    [SerializeField] private Animator animator;

    [Header("Light Combo (3 hits)")]
    [Tooltip("Number of hits in light combo (3 = Left, Right, Overhead).")]
    [SerializeField] private int lightComboHits = 3;

    [Tooltip("Max time allowed to buffer the next click (seconds).")]
    [SerializeField] private float comboBuffer = 0.20f;

    [Tooltip("Cooldown after the whole combo ends (seconds).")]
    [SerializeField] private float postComboCooldown = 0.20f;

    [System.Serializable]
    public struct StepTiming
    {
        [Tooltip("Delay before hitbox enables.")]
        public float windup;
        [Tooltip("Time window while hitbox is ON.")]
        public float active;
        [Tooltip("Recovery after hitbox turns OFF (can buffer next input here).")]
        public float recovery;
    }

    // Default timings: snappy & readable at ~60fps
    [SerializeField] private StepTiming step1 = new StepTiming { windup = 0.08f, active = 0.18f, recovery = 0.22f };
    [SerializeField] private StepTiming step2 = new StepTiming { windup = 0.10f, active = 0.20f, recovery = 0.24f };
    [SerializeField] private StepTiming step3 = new StepTiming { windup = 0.12f, active = 0.22f, recovery = 0.26f };

    [Header("Debug")]
    [SerializeField] private bool logInput = true;
    [SerializeField] private bool logActions = true;
    [SerializeField] private bool drawGizmos = true;
    
    [Header("Shield")]
    [Tooltip("Set true while blocking (e.g., RMB held). Read by Damageable.cs.")]
    public bool isBlocking = false;

    [Tooltip("Incoming damage is multiplied by this while blocking.")]
    [Range(0f, 1f)] public float blockDamageMultiplier = 0.35f;

    // Animator parameter hashes (match names EXACTLY in Animator)
    static readonly int HashAttack = Animator.StringToHash("Attack");
    static readonly int HashComboStep   = Animator.StringToHash("ComboStep");
    static readonly int HashIsAttacking = Animator.StringToHash("isAttacking"); // lowercase i

    // State
    private bool canAttack = true;        // gate to start a combo
    private bool isSwinging = false;      // true while any step is running
    private bool inputBuffered = false;   // set when we click during buffer window
    private int comboIndex = 0;           // 0..(lightComboHits-1) for steps; 0 means Step 1
    private Coroutine comboRoutine;

    void Awake()
    {
        if (!animator) animator = GetComponent<Animator>();

        if (!swordHitbox)
            Debug.LogWarning("[PawnCombat] No swordHitbox assigned.");

        if (swordHitbox && !swordHitbox.isTrigger)
        {
            Debug.LogWarning("[PawnCombat] Sword hitbox should be a Trigger.");
            swordHitbox.isTrigger = true;
        }

        if (swordHitbox) swordHitbox.enabled = false; // safety off at start
    }

    void Update()
    {
        // Left Mouse = light attack
        if (Input.GetMouseButtonDown(0))
        {
            if (logInput) Debug.Log("[PawnCombat] LMB => sword input");

            if (!isSwinging && canAttack)
            {
                // Start the combo
                if (comboRoutine != null) StopCoroutine(comboRoutine);
                comboRoutine = StartCoroutine(RunLightCombo());
            }
            else if (isSwinging)
            {
                // Buffer the next hit if still swinging
                inputBuffered = true;
                if (logInput) Debug.Log("[PawnCombat] Combo input buffered");
            }
        }
    }

    IEnumerator RunLightCombo()
    {
        canAttack = false;
        isSwinging = true;
        comboIndex = 0;

        // STEP 1
        yield return StartCoroutine(DoStep(comboIndex, GetStepTiming(0)));

        // Chain to STEP 2 if buffered within window
        if (inputBuffered && comboIndex + 1 < lightComboHits)
        {
            inputBuffered = false;
            comboIndex++;
            yield return StartCoroutine(DoStep(comboIndex, GetStepTiming(1)));
        }

        // Chain to STEP 3 if buffered within window
        if (inputBuffered && comboIndex + 1 < lightComboHits)
        {
            inputBuffered = false;
            comboIndex++;
            yield return StartCoroutine(DoStep(comboIndex, GetStepTiming(2)));
        }

        // Combo finished
        if (logActions) Debug.Log("[PawnCombat] Combo finished → ready");
        isSwinging = false;
        animator.SetInteger(HashComboStep, 0);
        animator.SetBool(HashIsAttacking, false);

        // Small cooldown before we can start again
        yield return new WaitForSeconds(postComboCooldown);
        canAttack = true;
        comboIndex = 0;
    }

    IEnumerator DoStep(int stepIndex, StepTiming t)
    {
        // Tell Animator which swing to play (1..3)
        PlayStepAnimation(stepIndex);

        if (logActions)
            Debug.Log($"[PawnCombat] Step {stepIndex + 1} START (windup={t.windup:0.00}, active={t.active:0.00}, recovery={t.recovery:0.00})");

        // WINDUP
        yield return new WaitForSeconds(t.windup);

        // ACTIVE (enable hitbox)
        if (swordHitbox) swordHitbox.enabled = true;
        yield return new WaitForSeconds(t.active);

        // END ACTIVE (disable hitbox)
        if (swordHitbox) swordHitbox.enabled = false;

        // RECOVERY (buffer window)
        float timer = 0f;
        bool chained = false;
        while (timer < t.recovery)
        {
            // If player has clicked during the combo, we accept it as a buffered input.
            if (inputBuffered)
            {
                chained = true;
                inputBuffered = false;
                if (logActions) Debug.Log("[PawnCombat] Chain → Step " + (stepIndex + 2));
                break; // exit recovery early; RunLightCombo will start next step
            }
            timer += Time.deltaTime;
            yield return null;
        }

        if (logActions)
            Debug.Log($"[PawnCombat] Step {stepIndex + 1} END ({(chained ? "chain" : "no chain")})");
    }

    StepTiming GetStepTiming(int i)
    {
        switch (i)
        {
            case 0: return step1;
            case 1: return step2;
            case 2: return step3;
            default: return step3;
        }
    }

    void PlayStepAnimation(int stepIndexZeroBased)
    {
        if (!animator) return;

        int comboStep = stepIndexZeroBased + 1; // 1..3 expected by Animator transitions
        animator.ResetTrigger(HashAttack);
        animator.SetInteger(HashComboStep, comboStep);
        animator.SetBool(HashIsAttacking, true);
        animator.SetTrigger(HashAttack);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!drawGizmos) return;
        // Simple gizmo showing a forward arc area (purely illustrative)
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position + Vector3.up * 0.8f, transform.forward * 0.75f);
    }
#endif
}

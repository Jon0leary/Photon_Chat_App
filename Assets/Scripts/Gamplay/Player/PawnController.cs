using Photon.Pun;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class PawnController : MonoBehaviourPun
{
    [Header("Move")]
    [SerializeField] float moveSpeed = 6f;      // target ground speed
    [SerializeField] float accel = 12f;         // how quickly we reach target speed
    [SerializeField] float turnLerp = 10f;      // how snappy rotation feels

    [SerializeField, Tooltip("Global speed scale (1 = normal).")]
    float speedMultiplier = 1f;

    Rigidbody rb;
    Transform cam; // main camera

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        // Small quality-of-life defaults
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    IEnumerator Start()
    {
        // Only drive camera & input for our own pawn
        if (!photonView.IsMine) yield break;

        // Wait a frame so Cinemachine is ready, then bind
        yield return null;
        FindObjectOfType<LocalCameraBinder>()?.Bind(transform);

        cam = Camera.main ? Camera.main.transform : null;
    }

    public void SetSpeedMultiplier(float value)
    {
        speedMultiplier = Mathf.Max(0f, value);
    }

    void FixedUpdate()
    {
        if (!photonView.IsMine) return;
        if (ChatManager.IsTyping) { rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0); return; }

        // --- Camera-relative input ---
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 input = new Vector3(h, 0f, v);
        if (input.sqrMagnitude > 1f) input.Normalize();

        Vector3 fwd = Vector3.forward;
        Vector3 right = Vector3.right;
        if (cam)
        {
            // flatten camera basis onto XZ
            Vector3 cf = cam.forward;  cf.y = 0f; cf.Normalize();
            Vector3 cr = cam.right;    cr.y = 0f; cr.Normalize();
            fwd = cf; right = cr;
        }

        Vector3 desiredVel = (fwd * input.z + right * input.x) * moveSpeed;
        // Smoothly accelerate towards target speed (keeps y from physics)
        Vector3 cur = rb.linearVelocity;
        Vector3 horiz = new Vector3(cur.x, 0f, cur.z);
        horiz = Vector3.MoveTowards(horiz, new Vector3(desiredVel.x, 0f, desiredVel.z), accel * Time.fixedDeltaTime);
        rb.linearVelocity = new Vector3(horiz.x, cur.y, horiz.z);   // <- correct property

        // Smooth face movement direction
        Vector3 look = new Vector3(horiz.x, 0f, horiz.z);
        if (look.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(look);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, turnLerp * Time.fixedDeltaTime);
        }
    }
}

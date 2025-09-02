using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(PhotonView))]
public class ArrowProjectile : MonoBehaviourPun
{
    Rigidbody rb;
    int ownerViewId;
    float damage;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    public void Launch(int ownerViewId, float speed, float damage)
    {
        this.ownerViewId = ownerViewId;
        this.damage = damage;
        rb.linearVelocity = transform.forward * speed;
    }

    void OnCollisionEnter(Collision c)
    {
        if (!photonView.IsMine) return;

        var dmg = c.collider.GetComponentInParent<Damageable>();
        var pv  = c.collider.GetComponentInParent<PhotonView>();

        if (dmg != null && pv != null && pv.ViewID != ownerViewId)
        {
            dmg.RPC_ApplyDamage(damage, transform.position);
        }

        PhotonNetwork.Destroy(gameObject);
    }
}

using Photon.Pun;
using UnityEngine;

public class Damageable : MonoBehaviourPun
{
    public float maxHP = 100f;
    public float hp = 100f;

    // Optional: hook to pawn’s combat for block
    PawnCombat combat;

    void Awake()
    {
        combat = GetComponent<PawnCombat>();
        hp = Mathf.Clamp(hp, 0f, maxHP);
    }

    public void RPC_ApplyDamage(float dmg, Vector3 fromWorld)
    {
        photonView.RPC(nameof(RPC_DoDamage), RpcTarget.All, dmg);
    }

    [PunRPC]
    void RPC_DoDamage(float dmg)
    {
        // Phase 1: simple block reduction
        if (combat && combat.isBlocking)
            dmg *= combat.blockDamageMultiplier;

        hp = Mathf.Max(0f, hp - dmg);

        if (hp <= 0f)
        {
            // TODO: simple respawn or disable
            // For now: just log
            Debug.Log($"{name} died");
        }
    }
}

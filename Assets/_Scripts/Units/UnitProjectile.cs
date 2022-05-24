using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
/// <summary>
///  單位子彈類 
/// </summary>
public class UnitProjectile : NetworkBehaviour
{
    [SerializeField] private Rigidbody rb = null;
    [SerializeField] private int damageToDeal = 20;
    [SerializeField] private float destoryAfterSeconds = 5f;
    [SerializeField] private float launchForce = 10;

    void Start()
    {
        rb.velocity = transform.forward * launchForce;
    }

    public override void OnStartServer()
    {
        Invoke(nameof(DestroySelf), destoryAfterSeconds);
    }

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<NetworkIdentity>(out NetworkIdentity networkIdentity))
        {
            //如果TRIGGER到自己單位則返回
            if (networkIdentity.connectionToClient == connectionToClient) return;
        }

        //如果有血量可以攻擊
        if (other.TryGetComponent<Health>(out Health health))
        {
            //扣血
            health.DealDamage(damageToDeal);
        }
        DestroySelf();

    }

    [Server]
    public void DestroySelf()
    {
        NetworkServer.Destroy(gameObject);
    }
}

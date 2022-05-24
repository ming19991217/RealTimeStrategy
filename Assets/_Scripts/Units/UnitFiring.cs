using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
/// <summary>
/// 單位開火 
/// </summary>
public class UnitFiring : NetworkBehaviour
{
    [SerializeField] private Targeter targeter = null;
    [SerializeField] private GameObject projectilePrefab = null; //彈藥預製件
    [SerializeField] private Transform projectileSpawnPoint = null;
    [SerializeField] private float fireRange = 5f; //開火距離
    [SerializeField] private float fireRate = 1; //開火間距
    [SerializeField] private float rotationSpeed = 20f; //單位旋轉速度

    private float lastFireTime;


    [ServerCallback]
    private void Update()
    {
        Targetable target = targeter.GetTarget();

        if (target == null) return;

        if (!CanFireAtTarget()) { return; }

        //取得指向目標的向量
        Quaternion targetRotation = Quaternion.LookRotation(target.transform.position - transform.position);
        //轉向目標
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        //如果超過上次開火時間
        if (Time.time > (1 / fireRate) + lastFireTime)
        {
            //子彈到目標點的旋轉量
            Quaternion projectileRotation = Quaternion.LookRotation(target.GetAimAtPoint().position - projectileSpawnPoint.position);

            //實例彈藥
            GameObject projectileInstance = Instantiate(projectilePrefab, projectileSpawnPoint.position, projectileRotation);

            //network生成子彈
            NetworkServer.Spawn(projectileInstance, connectionToClient);
            lastFireTime = Time.time;
        }

    }

    //檢測是否可以開火
    [Server]
    private bool CanFireAtTarget()
    {
        return (targeter.GetTarget().transform.position - transform.position).sqrMagnitude <= fireRange * fireRange;
    }
}

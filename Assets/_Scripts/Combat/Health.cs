using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
/// <summary>
/// 生命類 
/// </summary>
public class Health : NetworkBehaviour
{
    [SerializeField] private int maxHealth = 100;
    [SyncVar(hook = nameof(HandleHealthUpdated))] private int currentHealth; //同步當前血量

    public event Action ServerOnDie; //單位血量為0 調用
    public event Action<int, int> ClientOnHealthUpdated;//客戶端血量更新


    #region Server
    public override void OnStartServer()
    {
        currentHealth = maxHealth;

        UnitBase.ServerOnPlayerDie += ServerHandlePlayerDie; //玩家死亡 單位銷毀
    }

    public override void OnStopServer()
    {
        UnitBase.ServerOnPlayerDie -= ServerHandlePlayerDie;
    }

    [Server]
    private void ServerHandlePlayerDie(int connectionId) //玩家死亡後 銷毀單位
    {
        if (connectionToClient.connectionId != connectionId) return;

        DealDamage(currentHealth);
    }

    //扣血
    [Server]
    public void DealDamage(int damageAmount)
    {
        if (currentHealth == 0) return;
        currentHealth = Mathf.Max(currentHealth - damageAmount, 0);

        if (currentHealth != 0) return;
        ServerOnDie?.Invoke();

        Debug.Log("We died");
    }

    #endregion

    #region Client

    //當血量變數改變時調用事件
    private void HandleHealthUpdated(int oldHealth, int newHealth)
    {
        //更新客戶端單位UI
        ClientOnHealthUpdated?.Invoke(newHealth, maxHealth);
    }



    #endregion
}

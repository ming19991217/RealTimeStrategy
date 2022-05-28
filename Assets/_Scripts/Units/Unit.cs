using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

public class Unit : NetworkBehaviour
{
    [SerializeField] private int resourceCost = 10;
    [SerializeField] private Health health = null;
    [SerializeField] private UnitMovement unitMovement = null;
    [SerializeField] private Targeter targeter = null;
    [SerializeField] private UnityEvent OnSelected; //被選擇時執行
    [SerializeField] private UnityEvent OnDeselected; //取消選擇執行


    //這裡分成Server和Authority兩種的原因:
    //當單位生成時 會同時調用 OnStartServer OnStartClient
    //前者是讓伺服器端 知道所有人的單位 以及 伺服器端單位的註冊
    //後者是讓客戶端 註冊自己所擁有的單位
    public static event Action<Unit> ServerOnUnitSpawned; //當單位誕生 伺服器端
    public static event Action<Unit> ServerOnUnitDespawned; //當單位消失 伺服器端
    public static event Action<Unit> AuthorityOnUnitSpawned; //當單位誕生 客戶端
    public static event Action<Unit> AuthorityOnUnitDespawned; //當單位消失 客戶端

    //當選擇單位後 需要到unitmovement進行調用
    public UnitMovement GetUnitMovement() { return unitMovement; }
    public Targeter GetTargeter() { return targeter; }
    public int GetResourceCost() { return resourceCost; }


    #region Server

    //伺服器端 當單位生成時在伺服器端調用
    public override void OnStartServer() //
    {
        ServerOnUnitSpawned?.Invoke(this);//當單位生成時 向RTSPlayerz註冊

        health.ServerOnDie += ServerHandleDie;
    }


    public override void OnStopServer()//
    {
        ServerOnUnitDespawned?.Invoke(this); //當單位結束時 取消訂閱

        health.ServerOnDie -= ServerHandleDie;
    }

    [Server]
    private void ServerHandleDie() //單位死亡
    {
        NetworkServer.Destroy(gameObject);
    }

    #endregion

    #region Client

    // 當單位生成時 在擁有該對象的客戶端調用
    public override void OnStartAuthority()
    {
        //如果是客戶端 並且 有單位權限
        AuthorityOnUnitSpawned?.Invoke(this);
    }
    public override void OnStopClient()
    {
        if (!hasAuthority) return;
        //如果是客戶端 並且 有單位權限
        AuthorityOnUnitDespawned?.Invoke(this);
    }

    //選擇單位方法
    [Client]
    public void Select()
    {
        if (!hasAuthority) return;
        OnSelected?.Invoke();
    }
    [Client]
    public void Deselect()
    {
        if (!hasAuthority) return;
        OnDeselected?.Invoke();
    }
    #endregion 
}

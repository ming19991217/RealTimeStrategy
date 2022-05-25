using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
/// <summary>
/// 遊戲檢測
/// </summary>
public class GameOverHandler : NetworkBehaviour
{

    public static event Action ServerOnGameOver;
    public static event Action<string> ClientOnGameOver;//客戶端調用遊戲結束事件

    private List<UnitBase> bases = new List<UnitBase>(); //玩家基地列表

    #region  Server

    public override void OnStartServer()
    {
        //註冊製造者創建、死亡事件
        UnitBase.ServerOnBaseSpawned += ServerHandleBaseSpawned;
        UnitBase.ServerOnBaseDespawned += ServerHandleBaseDespawned;
    }

    public override void OnStopServer()
    {
        //取消註冊製造者創建、死亡事件
        UnitBase.ServerOnBaseSpawned -= ServerHandleBaseSpawned;
        UnitBase.ServerOnBaseDespawned -= ServerHandleBaseDespawned;

    }

    //當單位生成者創建調用 註冊到列表
    [Server]
    private void ServerHandleBaseSpawned(UnitBase unitBase)
    {
        bases.Add(unitBase);
    }

    //單位基地被摧毀時調用
    [Server]
    private void ServerHandleBaseDespawned(UnitBase unitBase)
    {
        bases.Remove(unitBase);

        if (bases.Count != 1) return;

        int playerId = bases[0].connectionToClient.connectionId;

        RpcGameOver($"Player {playerId}");

        ServerOnGameOver?.Invoke();
    }
    #endregion

    #region  Client;

    [ClientRpc] //伺服器呼叫客戶端調用 
    private void RpcGameOver(string winner)
    {
        //在各個客戶端顯示遊戲結束
        ClientOnGameOver?.Invoke(winner);
    }


    #endregion
}

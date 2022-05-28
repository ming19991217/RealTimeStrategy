using System;
using Mirror;
using UnityEngine;
using UnityEngine.AI;
/// <summary>
/// 單位移動 
/// </summary>
public class UnitMovement : NetworkBehaviour
{
    [SerializeField] private NavMeshAgent agent = null;
    [SerializeField] private Targeter targeter = null;
    [SerializeField] private float chaseRange = 10f; //追擊距離
    #region Server

    public override void OnStartServer()
    {
        GameOverHandler.ServerOnGameOver += ServerHandleGameOver;
    }



    public override void OnStopServer()
    {
        GameOverHandler.ServerOnGameOver -= ServerHandleGameOver;
    }

    //讓update onTriggerEnter 之類的調用不報錯 使用serverCallback
    [ServerCallback]
    private void Update()
    {
        Targetable target = targeter.GetTarget();

        //如果目標不是空
        if (target != null)
        {
            //計算目標與自身距離
            if ((target.transform.position - transform.position).sqrMagnitude > chaseRange * chaseRange)
            {
                agent.SetDestination(target.transform.position);
            }
            else if (agent.hasPath)
            {
                agent.ResetPath();
            }
            return;
        }

        //如果沒有指定路徑
        if (!agent.hasPath) return;

        //判斷是否到達距離
        if (agent.remainingDistance > agent.stoppingDistance) return;

        //清空路徑
        agent.ResetPath();
    }


    [Command] //客戶端呼叫伺服器去執行一個方法
    public void CmdMove(Vector3 position)
    {
        ServerMove(position);
    }

    [Server] //給伺服器直接調用的移動方法
    public void ServerMove(Vector3 position)
    {
        targeter.ClearTarget();

        if (!NavMesh.SamplePosition(position, out NavMeshHit hit, 1f, NavMesh.AllAreas)) return;

        agent.SetDestination(hit.position);
    }

    [Server]
    private void ServerHandleGameOver()
    {
        agent.ResetPath();
    }
    #endregion

}

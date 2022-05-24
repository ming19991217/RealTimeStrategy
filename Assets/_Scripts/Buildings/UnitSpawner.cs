using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;
/// <summary>
/// 單位生產者 
/// </summary>
public class UnitSpawner : NetworkBehaviour, IPointerClickHandler
{
    [SerializeField] private Health health = null;
    [SerializeField] private GameObject unitPrefab = null; //產生器要產生的預製件
    [SerializeField] private Transform unitSpawnPoint = null; // 產生器產生地點


    #region Server 

    public override void OnStartServer()
    {
        //訂閱生成建築死亡事件
        health.ServerOnDie += ServerHandleDie;
    }

    public override void OnStopServer()
    {
        health.ServerOnDie -= ServerHandleDie;
    }

    [Server]
    private void ServerHandleDie()
    {
        //生成建築死亡
        // NetworkServer.Destroy(gameObject);
    }

    [Command]
    private void CmdSpawnUnit() //客戶端命令伺服器 產生單位
    {
        //由伺服器端的客戶端實例單位
        GameObject unitInstance = Instantiate(unitPrefab, unitSpawnPoint.position, unitSpawnPoint.rotation);

        //並交由生成系統去完成生成
        //ownerConnection 如果不輸入生成 那麼他不屬於如何客戶端 只是Server only object
        //connectionToClient 當我cmd這個方法 就將單位歸屬於我
        NetworkServer.Spawn(unitInstance, connectionToClient);
    }
    #endregion



    #region Client
    public void OnPointerClick(PointerEventData eventData)
    {
        //使用條件： EventSystem mainCamera要掛PhysicsRaycaster
        //判斷點擊左鍵
        if (eventData.button != PointerEventData.InputButton.Left) return;

        //判斷這個物件是否屬於調用的客戶端
        if (!hasAuthority) return;

        //生成物件
        CmdSpawnUnit();
    }



    #endregion
}

using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
/// <summary>
/// 單位生產者 
/// </summary>
public class UnitSpawner : NetworkBehaviour, IPointerClickHandler
{
    [SerializeField] private Health health = null;
    [SerializeField] private Unit unitPrefab = null; //產生器要產生的預製件
    [SerializeField] private Transform unitSpawnPoint = null; // 產生器產生地點
    [SerializeField] private TMP_Text remainingUnitsText = null; //列隊ui
    [SerializeField] private Image unitProgressImage = null;//生產進度ui
    [SerializeField] private int maxUnitQueue = 5; //最大列隊容量
    [SerializeField] private float spawnMoveRange = 7;
    [SerializeField] private float unitSpawnDuration = 5f; //生產時間

    [SyncVar(hook = nameof(ClientHandleQueuedUnitsUpdated))]
    private int queuedUnits;//排隊生產單位數量
    [SyncVar]
    private float unitTimer; //生產進度計時

    private float progressImageVelocity;

    //因為需要客戶和伺服器端做判斷 所以不加serverCallBack
    private void Update()
    {
        if (isServer)//如果是伺服器端
        {
            ProduceUnits(); //生產單位
        }
        if (isClient) //如果是客戶端
        {
            UpdateTimerDisplay(); //更新生產ui
        }
    }


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
    private void ProduceUnits() //生產單位
    {
        if (queuedUnits == 0) { return; } //如果沒有排隊生產的單位

        unitTimer += Time.deltaTime;

        if (unitTimer < unitSpawnDuration) return; //判斷生產是否完成

        //實例單位
        GameObject unitInstance = Instantiate(unitPrefab.gameObject, unitSpawnPoint.position, unitSpawnPoint.rotation);
        //並交由生成系統去完成生成
        //ownerConnection 如果不輸入生成 那麼他不屬於如何客戶端 只是Server only object
        //connectionToClient 當我cmd這個方法 就將單位歸屬於我
        NetworkServer.Spawn(unitInstance, connectionToClient);

        //隨機生成地點
        Vector3 spawnOffset = Random.insideUnitSphere * spawnMoveRange;
        spawnOffset.y = unitSpawnPoint.position.y;
        //移動單位到初始點
        UnitMovement unitMovement = unitInstance.GetComponent<UnitMovement>();
        unitMovement.ServerMove(unitSpawnPoint.position + spawnOffset);

        //生產序列-1
        queuedUnits--;
        unitTimer = 0f;

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
        if (queuedUnits == maxUnitQueue) return;

        //資源數
        RTSPlayer player = connectionToClient.identity.GetComponent<RTSPlayer>();
        //資源數不夠
        if (player.GetResources() < unitPrefab.GetResourceCost()) return;

        //生產排列+1
        queuedUnits++;
        //扣錢
        player.SetResources(player.GetResources() - unitPrefab.GetResourceCost());

    }
    #endregion



    #region Client

    //序列顯示
    private void UpdateTimerDisplay()
    {
        float newProgress = unitTimer / unitSpawnDuration;

        if (newProgress < unitProgressImage.fillAmount)
        {
            unitProgressImage.fillAmount = newProgress;
        }
        else
        {
            unitProgressImage.fillAmount = Mathf.SmoothDamp(
                unitProgressImage.fillAmount,
                newProgress,
                ref progressImageVelocity,
                0.1f
            );
        }
    }

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


    private void ClientHandleQueuedUnitsUpdated(int oldUnits, int newUnits)
    {
        remainingUnitsText.text = newUnits.ToString();
    }

    #endregion
}

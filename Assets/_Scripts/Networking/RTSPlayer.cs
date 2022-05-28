using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
/// <summary>
/// 存儲玩家單位 
/// </summary>
public class RTSPlayer : NetworkBehaviour
{
    [SerializeField] private LayerMask buildingBlockLayer = new LayerMask();//放置建築檢測圖層
    [SerializeField] private Building[] buildings = new Building[0]; //建築種類數組
    [SerializeField] private float buildingRangeLimit = 5f;

    //
    [SyncVar(hook = nameof(ClientHandleResourcesUpdated))] //改變資源數值時調用
    private int resources = 500;//資源數
    public event Action<int> ClientOnResourcesUpdated; //資源數值更變時調用
    public int GetResources() { return resources; }


    //
    private List<Unit> myUnits = new List<Unit>(); //玩家擁有單位
    private List<Building> myBuildings = new List<Building>();//玩家建築

    public List<Unit> GetMyUnits() { return myUnits; }
    public List<Building> GetMyBuildings() { return myBuildings; }
    //
    //判斷是否可以放置建築
    public bool CanPlaceBuilding(BoxCollider buildingCollider, Vector3 point)
    {
        //檢測放置區域碰撞
        if (Physics.CheckBox(
            point + buildingCollider.center,
            buildingCollider.size / 2,
            Quaternion.identity,
            buildingBlockLayer))
        {
            return false;
        }
        print("box true");
        //判斷放置範圍
        foreach (Building building in myBuildings)
        {
            print(building.name);
            //比較放置點和已有建築之間的距離
            if ((point - building.transform.position).sqrMagnitude <= buildingRangeLimit * buildingRangeLimit)
            {
                return true;
            }
        }
        return false; //如果不再範圍內 返回

    }
    //
    private Color teamColor = new Color();
    public Color GetTeamColor() { return teamColor; }

    #region  Server
    //玩家開始時 
    public override void OnStartServer()
    {
        //訂閱單位生成事件
        Unit.ServerOnUnitSpawned += ServerHandleUnitSpawned;
        Unit.ServerOnUnitDespawned += ServerHandleUnitDespawned;
        //訂閱建築生成摧毀事件
        Building.ServerOnBuildingSpawned += ServerHandleBuildingSpawned;
        Building.ServerOnBuildingDespawned += ServerHandleBuildingDespawned;

    }

    //玩家離開時 調用
    public override void OnStopServer()
    {
        Unit.ServerOnUnitSpawned -= ServerHandleUnitSpawned;
        Unit.ServerOnUnitDespawned -= ServerHandleUnitDespawned;

        Building.ServerOnBuildingSpawned -= ServerHandleBuildingSpawned;
        Building.ServerOnBuildingDespawned -= ServerHandleBuildingDespawned;

    }

    [Server] //設置資源數
    public void SetResources(int newResources) { resources = newResources; }
    [Server] //設置團隊顏色
    public void SetTeamColor(Color newTeamColor) { teamColor = newTeamColor; }


    //所以單位生成時 伺服器端都會調用 讓伺服器端知道所以人的擁有單位
    private void ServerHandleUnitSpawned(Unit unit)
    {
        // 每個單位創建時 每個RTSPlayer的serverHandleUnitSpawned都會被調用到
        // 需要判斷單位所屬的id是否為當前id
        if (unit.connectionToClient.connectionId != connectionToClient.connectionId) return;

        myUnits.Add(unit);
    }

    //當摧毀單位時調用
    private void ServerHandleUnitDespawned(Unit unit)
    {
        if (unit.connectionToClient.connectionId != connectionToClient.connectionId) return;

        myUnits.Remove(unit);

    }

    //伺服器端處理建築摧毀
    private void ServerHandleBuildingDespawned(Building building)
    {
        if (building.connectionToClient.connectionId != connectionToClient.connectionId) return;
        myBuildings.Remove(building);
    }

    //伺服器端處理建築生成
    private void ServerHandleBuildingSpawned(Building building)
    {
        if (building.connectionToClient.connectionId != connectionToClient.connectionId) return;
        myBuildings.Add(building);

    }

    //放置建築
    [Command]
    internal void CmdTryPlaceBuilding(int buildingId, Vector3 point)
    {
        Building buildingToPlace = null;
        foreach (Building building in buildings)
        {
            //找到要求建造的建築
            if (building.GetId() == buildingId)
            {
                buildingToPlace = building;
                break;
            }
        }
        if (buildingToPlace == null) return;

        BoxCollider buildingCollider = buildingToPlace.GetComponent<BoxCollider>();
        if (!CanPlaceBuilding(buildingCollider, point)) return;

        //如果資源數小於建築價格
        if (resources < buildingToPlace.GetPrice()) return;
        //實例建築
        GameObject buildingInatance = Instantiate(buildingToPlace.gameObject, point, buildingToPlace.transform.rotation);
        NetworkServer.Spawn(buildingInatance, connectionToClient);

        //扣錢
        SetResources(resources - buildingToPlace.GetPrice());
    }


    #endregion

    #region Client


    public override void OnStartAuthority()
    {
        //因為host也會調用onStartClient 
        //所以限制僅客戶端才能調用
        //if (!isClientOnly) return;
        if (NetworkServer.active) return;

        //訂閱單位生成事件
        Unit.AuthorityOnUnitSpawned += AuthorityHandleUnitSpawned;
        Unit.AuthorityOnUnitDespawned += AuthorityHandleUnitDespawned;

        //訂閱建築生成銷毀事件
        Building.AuthorityOnBuildingSpawned += AuthorityHandleBuildingSpawned;
        Building.AuthorityOnBuildingDespawned += AuthorityHandleBuildingDespawned;
    }

    public override void OnStopClient()
    {
        if (!isClientOnly || !hasAuthority) return;

        // 取消訂閱單位生成事件
        Unit.AuthorityOnUnitSpawned -= AuthorityHandleUnitSpawned;
        Unit.AuthorityOnUnitDespawned -= AuthorityHandleUnitDespawned;
        //取消訂閱建築生成銷毀事件
        Building.AuthorityOnBuildingSpawned -= AuthorityHandleBuildingSpawned;
        Building.AuthorityOnBuildingDespawned -= AuthorityHandleBuildingDespawned;
    }


    //當資源數改變
    private void ClientHandleResourcesUpdated(int oldResources, int newResources)
    {
        ClientOnResourcesUpdated?.Invoke(newResources);
    }


    //創建單位 註冊
    private void AuthorityHandleUnitSpawned(Unit unit)
    {
        myUnits.Add(unit);
    }
    //當摧毀單位時調用
    private void AuthorityHandleUnitDespawned(Unit unit)
    {
        //每個RTSPlayer都會調用 所以需要判讀是否為這個RTSPlayer所擁有
        myUnits.Remove(unit);
    }

    //創建建築
    private void AuthorityHandleBuildingSpawned(Building building)
    {
        myBuildings.Add(building);
    }
    private void AuthorityHandleBuildingDespawned(Building building)
    {
        myBuildings.Remove(building);
    }



    #endregion
}

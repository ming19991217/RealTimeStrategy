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
    [SerializeField] private Building[] buildings = new Building[0]; //建築種類數組
    private List<Unit> myUnits = new List<Unit>(); //玩家擁有單位
    private List<Building> myBuildings = new List<Building>();//玩家建築

    public List<Unit> GetMyUnits() { return myUnits; }
    public List<Building> GetMyBuildings() { return myBuildings; }

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

    //伺服器端處理建築生成
    private void ServerHandleBuildingDespawned(Building building)
    {
        if (building.connectionToClient.connectionId != connectionToClient.connectionId) return;
        myBuildings.Add(building);
    }

    //伺服器端處理建築摧毀
    private void ServerHandleBuildingSpawned(Building building)
    {
        if (building.connectionToClient.connectionId != connectionToClient.connectionId) return;
        myBuildings.Remove(building);

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

        GameObject buildingInatance = Instantiate(buildingToPlace.gameObject, point, buildingToPlace.transform.rotation);
        NetworkServer.Spawn(buildingInatance, connectionToClient);
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

using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
/// <summary>
/// 建築類 價格 建築id 
/// </summary>
public class Building : NetworkBehaviour
{
    [SerializeField] private GameObject buildingPreview = null;
    [SerializeField] private Sprite icon = null;
    [SerializeField] private int id = -1;
    [SerializeField] private int price = 100;

    public static event Action<Building> ServerOnBuildingSpawned; //當單位誕生 伺服器端
    public static event Action<Building> ServerOnBuildingDespawned; //當單位消失 伺服器端

    public static event Action<Building> AuthorityOnBuildingSpawned; //客戶端 建築創建
    public static event Action<Building> AuthorityOnBuildingDespawned; //客戶端 建築銷毀

    public GameObject GetBuildingPreview() { return buildingPreview; }
    public Sprite GetIcon() { return icon; }
    public int GetId() { return id; }
    public int GetPrice() { return price; }


    #region Server
    public override void OnStartServer()
    {
        ServerOnBuildingSpawned?.Invoke(this);
    }
    public override void OnStopServer()
    {
        ServerOnBuildingDespawned?.Invoke(this);
    }
    #endregion

    #region Client

    public override void OnStartAuthority()
    {
        AuthorityOnBuildingSpawned?.Invoke(this);
    }

    public override void OnStopClient()
    {
        if (!hasAuthority) return;

        AuthorityOnBuildingDespawned?.Invoke(this);
    }

    #endregion

}

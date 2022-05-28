using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
/// <summary>
/// 顯示資源數 
/// </summary>
public class ResourcesDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text resourcesText = null;
    private RTSPlayer player;

    private void Update()
    {
        if (player == null)
        {
            //因為現在沒有連線大廳 所以獲取player目前用update 
            player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();

            if (player != null)
            {
                ClientHandleResourcesUpdated(player.GetResources());
                player.ClientOnResourcesUpdated += ClientHandleResourcesUpdated;
            }
        }
    }
    private void OnDestroy()
    {
        player.ClientOnResourcesUpdated -= ClientHandleResourcesUpdated;
    }

    //更新資源檢視
    private void ClientHandleResourcesUpdated(int resources)
    {
        resourcesText.text = $"Resources:{resources}";
    }

}

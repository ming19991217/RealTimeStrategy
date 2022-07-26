using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
using System;

public class RTSNetworkManager : NetworkManager
{
    [SerializeField] private GameObject unitBasePrefab = null;
    [SerializeField] private GameOverHandler gameOverHandlerPrefab = null;

    private bool isGameInProgress = false;
    public List<RTSPlayer> Players { get; } = new List<RTSPlayer>();

    public static event Action ClientOnConnected; //客戶端連接 客戶端調用事件
    public static event Action ClientOnDisconnected; //客戶端斷開，客戶端調用


    #region Server

    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        //如果遊戲進行中 禁止連接
        if (!isGameInProgress) return;

        conn.Disconnect();
    }

    //當客戶端斷開連接，伺服器斷調用
    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        //移除玩家
        RTSPlayer player = conn.identity.GetComponent<RTSPlayer>();

        Players.Remove(player);

        base.OnServerDisconnect(conn);
    }

    //當伺服器停止
    public override void OnStopServer()
    {
        Players.Clear();
        isGameInProgress = false;
    }

    //開始遊戲
    public void StartGame()
    {
        //人數小於2返回
        if (Players.Count < 2) return;

        isGameInProgress = true;

        ServerChangeScene("Scene_Map_01");
    }

    //當玩家加入伺服器 給一個單位生成器
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);


        RTSPlayer player = conn.identity.GetComponent<RTSPlayer>();

        Players.Add(player);
        player.SetDisplayName($"Player{Players.Count}");

        //設置玩家顏色
        player.SetTeamColor(new Color(
            UnityEngine.Random.Range(0f, 1f),
            UnityEngine.Random.Range(0f, 1f),
        UnityEngine.Random.Range(0f, 1f)
        ));

        player.SetPartyOwner(Players.Count == 1);
    }

    //當伺服器端完成場景轉換
    public override void OnServerSceneChanged(string sceneName)
    {
        //如果場景是主場景
        if (SceneManager.GetActiveScene().name.StartsWith("Scene_Map"))
        {
            // 生成gameoverhandler
            GameOverHandler gameOverHandlerInstance = Instantiate(gameOverHandlerPrefab);

            NetworkServer.Spawn(gameOverHandlerInstance.gameObject);

            //爲每個玩家創建基地
            foreach (RTSPlayer player in Players)
            {
                GameObject baseInstance = Instantiate(unitBasePrefab, GetStartPosition().position, Quaternion.identity);
                NetworkServer.Spawn(baseInstance, player.connectionToClient);
            }
        }
    }
    #endregion

    #region Client
    //當客戶端連接，在客戶端調用
    public override void OnClientConnect()
    {
        base.OnClientConnect();

        ClientOnConnected?.Invoke();
    }
    //當斷開連接，在客戶端調用
    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();

        ClientOnDisconnected?.Invoke();
    }

    public override void OnStartClient()
    {
        Players.Clear();
    }

    #endregion





}

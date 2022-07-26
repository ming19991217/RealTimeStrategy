using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;
/// <summary>
/// 單位選擇
/// </summary>
public class UnitSelectionHandler : MonoBehaviour
{
    [SerializeField] private RectTransform unitSelectionArea = null; //選擇框ui
    [SerializeField] private LayerMask layerMask;

    private Vector2 startPosition;

    private RTSPlayer player; //為了知道單位是否為己方
    private Camera mainCamera;
    public List<Unit> SelectedUnits { get; } = new List<Unit>();
    private void Start()
    {
        mainCamera = Camera.main;
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();

        Unit.AuthorityOnUnitDespawned += AuthorityHandleUnitDespawned; //單位死亡後 移除選擇列表

        GameOverHandler.ClientOnGameOver += ClientHandleGameOver; //遊戲結束後 停止選擇功能
    }
    private void OnDestroy()
    {
        Unit.AuthorityOnUnitDespawned -= AuthorityHandleUnitDespawned;
        GameOverHandler.ClientOnGameOver -= ClientHandleGameOver;
    }

    private void Update()
    {

        //按下左鍵 清空選擇
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            StartSelectionArea();
        }
        //鬆開右鍵 選擇單位
        else if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            ClearSelectionArea();
        }
        else if (Mouse.current.leftButton.isPressed) //持續按住右鍵
        {
            UpdateSelectionArea();
        }
    }

    //選擇區域初始
    private void StartSelectionArea()
    {
        //如果沒有按住shift 清除上次選擇
        if (!Keyboard.current.leftShiftKey.isPressed)
        {
            //將所以已選單位取消選擇
            foreach (var selectedUnit in SelectedUnits)
            {
                selectedUnit.Deselect();
            }
            SelectedUnits.Clear();
        }

        //開啟選擇框
        unitSelectionArea.gameObject.SetActive(true);
        startPosition = Mouse.current.position.ReadValue();

        UpdateSelectionArea();
    }

    //更新選擇區域框
    private void UpdateSelectionArea()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        float areaWidth = mousePosition.x - startPosition.x;
        float areaHeight = mousePosition.y - startPosition.y;
        //選擇框大小
        unitSelectionArea.sizeDelta = new Vector2(Mathf.Abs(areaWidth), Mathf.Abs(areaHeight));
        unitSelectionArea.anchoredPosition = startPosition + new Vector2(areaWidth / 2, areaHeight / 2);

    }

    //清空選擇框
    private void ClearSelectionArea()
    {
        unitSelectionArea.gameObject.SetActive(false);

        //如果選擇框模長為0 則進行單選
        if (unitSelectionArea.sizeDelta.magnitude == 0)
        {
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask)) return;

            if (!hit.collider.TryGetComponent<Unit>(out Unit unit)) return;

            SelectedUnits.Add(unit);

            foreach (var selectUnit in SelectedUnits)
            {
                selectUnit.Select();
            }

            return;
        }

        //計算選擇框範圍
        //取得左下角座標
        Vector2 min = unitSelectionArea.anchoredPosition - (unitSelectionArea.sizeDelta / 2);
        //右上角座標
        Vector2 max = unitSelectionArea.anchoredPosition + (unitSelectionArea.sizeDelta / 2);

        foreach (var unit in player.GetMyUnits())
        {
            //如果已經有上次選擇的單位 跳過;
            if (SelectedUnits.Contains(unit)) continue;

            Vector3 screenPosition = mainCamera.WorldToScreenPoint(unit.transform.position);

            //如果單位在範圍內
            if (screenPosition.y > min.y && screenPosition.y < max.y &&
            screenPosition.x > min.x && screenPosition.x < max.x)
            {
                SelectedUnits.Add(unit);
                unit.Select();
            }
        }


    }

    //當單位消失調用 將單位移除選擇列表
    private void AuthorityHandleUnitDespawned(Unit unit)
    {
        SelectedUnits.Remove(unit);
    }



    //遊戲結束客戶端
    private void ClientHandleGameOver(string winner)
    {
        enabled = false;
    }






}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
/// <summary>
/// 單位命令者 結合UnitSelectionHandler去調用unit移動命令 或 攻擊
/// </summary>
public class UnitCommandGiver : MonoBehaviour
{
    [SerializeField] private UnitSelectionHandler unitSelectionHandler = null;
    [SerializeField] private LayerMask layerMask;
    private Camera mainCamera;
    private void Start()
    {
        mainCamera = Camera.main;

        GameOverHandler.ClientOnGameOver += ClientHandleGameOver; //遊戲結束時 停止使用
    }



    private void Update()
    {
        //檢測是否點擊右鍵
        if (!Mouse.current.rightButton.wasPressedThisFrame) return;
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask)) return;
        //如果點擊目標是可攻擊對象
        if (hit.collider.TryGetComponent<Targetable>(out Targetable target))
        {
            //是自己人 移動
            if (target.hasAuthority)
            {
                TryMove(hit.point);
                return;
            }
            //否則攻擊
            TryTarget(target);
            return;
        }

        //嘗試單位移動
        TryMove(hit.point);
    }

    private void TryTarget(Targetable target)
    {
        foreach (var unit in unitSelectionHandler.SelectedUnits)
        {
            unit.GetTargeter().SetTarget(target.gameObject);
        }
    }

    private void TryMove(Vector3 point)
    {
        foreach (var unit in unitSelectionHandler.SelectedUnits)
        {
            unit.GetUnitMovement().CmdMove(point);
        }
    }
    private void ClientHandleGameOver(string winner)
    {
        enabled = false;
    }
}

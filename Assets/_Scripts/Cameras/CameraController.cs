using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 鏡頭控制 
/// </summary>

public class CameraController : NetworkBehaviour
{

    [SerializeField] private Transform playerCameraTransform = null; //玩家攝影機
    [SerializeField] private float speed = 20f; //移動速度
    [SerializeField] private float screenBorderThickness = 10f;  //熒幕邊緣範圍
    [SerializeField] private Vector2 screenXLimits = Vector2.zero; //限制相機位置
    [SerializeField] private Vector2 screenZLimits = Vector2.zero;


    private Vector2 previousInput; //鍵盤輸入值

    private Controls controls;  //input controls

    public override void OnStartAuthority()
    {
        playerCameraTransform.gameObject.SetActive(true); //開啓玩家自身攝影機

        controls = new Controls();

        //鍵盤執行、結束時調用 設定鍵盤輸入值
        controls.Player.MoveCamera.performed += SetPreciousInput;
        controls.Player.MoveCamera.canceled += SetPreciousInput;

        controls.Enable();
    }

    [ClientCallback]
    private void Update()
    {
        //isFocused 鼠標是否在遊戲內
        if (!hasAuthority || !Application.isFocused) { return; }

        //更新攝影機位置
        UpdateCameraPosition();
    }

    private void UpdateCameraPosition()
    {
        Vector3 pos = playerCameraTransform.position;

        //如果鍵盤輸入爲空，執行鼠標移動
        if (previousInput == Vector2.zero)
        {

            Vector3 cursorMovement = Vector3.zero;
            //獲取鼠標位置
            Vector2 cursorPosition = Mouse.current.position.ReadValue();

            //比較熒幕位置
            if (cursorPosition.y >= Screen.height - screenBorderThickness)
            {
                cursorMovement.z -= 1;
            }
            else if (cursorPosition.y <= screenBorderThickness)
            {
                cursorMovement.z += 1;
            }

            if (cursorPosition.x >= Screen.width - screenBorderThickness)
            {
                cursorMovement.x -= 1;
            }
            else if (cursorPosition.x <= screenBorderThickness)
            {
                cursorMovement.x += 1;
            }

            pos += cursorMovement.normalized * speed * Time.deltaTime;
        }
        else //鍵盤移動
        {
            pos += new Vector3(-previousInput.x, 0f, -previousInput.y) * speed * Time.deltaTime;
        }

        //限制移動位置
        pos.x = Mathf.Clamp(pos.x, screenXLimits.x, screenZLimits.y);
        pos.z = Mathf.Clamp(pos.z, screenZLimits.x, screenZLimits.y);

        //移動鏡頭
        playerCameraTransform.position = pos;
    }

    //獲得鍵盤移動輸入
    private void SetPreciousInput(InputAction.CallbackContext ctx)
    {

        previousInput = ctx.ReadValue<Vector2>();
    }
}


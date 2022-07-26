using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Minmap : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [SerializeField] private RectTransform minimapRect = null;
    [SerializeField] private float mapScale = 20f;
    [SerializeField] private float offset = -6;

    private Transform playerCameraTransform;

    private void Update()
    {
        if (playerCameraTransform != null) return;

        if (NetworkClient.connection.identity == null) return;

        playerCameraTransform = NetworkClient.connection.identity.GetComponent<RTSPlayer>().GetCameraTransform();
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        MoveCamera();
    }

    public void OnDrag(PointerEventData eventData)
    {
        MoveCamera();
    }
    private void MoveCamera()
    {

        Vector2 mousePos = Mouse.current.position.ReadValue();

        //判斷滑鼠座標是否在小地圖上
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            minimapRect, mousePos, null, out Vector2 localPoint
        )) return;

        //計算鼠標位置=小地圖的百分比
        Vector2 lerp = new Vector2((
            (localPoint.x - minimapRect.rect.x) / minimapRect.rect.width),
            (localPoint.y - minimapRect.rect.y) / minimapRect.rect.height);

        //用lerp取得地圖百分比的位置，mapscale=地圖長度的一半
        Vector3 newCameraPos = new Vector3(Mathf.Lerp(mapScale, -mapScale, lerp.x), playerCameraTransform.position.y, Mathf.Lerp(mapScale, -mapScale, lerp.y));

        playerCameraTransform.position = newCameraPos + new Vector3(0f, 0f, offset);
    }


}

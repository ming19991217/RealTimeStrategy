using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
/// <summary>
/// 建築按鈕(ui) 建築拖拽生成 
/// </summary>
public class BuildingButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Building building = null; //欲生成建築building類
    [SerializeField] private Image iconImage = null;
    [SerializeField] private TMP_Text priceText = null;
    [SerializeField] private LayerMask floorMask = new LayerMask(); //拖拽建築預覽需用

    private Camera mainCamera;
    private RTSPlayer player;
    private GameObject buildingPreviewInstance; //建築預製件
    private Renderer buildingRendererInstance;//建築renderer組件 預覽用
    private BoxCollider buildingCollider;


    private void Start()
    {
        mainCamera = Camera.main;

        iconImage.sprite = building.GetIcon();
        priceText.text = building.GetPrice().ToString();
        buildingCollider = building.GetComponent<BoxCollider>();
    }

    private void Update()
    {
        if (player == null) //獲得play組件
        {
            player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        }

        //如果已經生成建築預製件
        if (buildingPreviewInstance == null) return;
        UpdateBuildingPreview();
    }

    //當鼠標點擊建築按鈕
    public void OnPointerDown(PointerEventData eventData)
    {
        //是左鍵
        if (eventData.button != PointerEventData.InputButton.Left) return;

        //有錢才能拖拽
        if (player.GetResources() < building.GetPrice()) return;

        //生成建築實例
        buildingPreviewInstance = Instantiate(building.GetBuildingPreview());
        buildingRendererInstance = buildingPreviewInstance.GetComponentInChildren<Renderer>();

        buildingPreviewInstance.SetActive(false);
    }



    //當鼠標抬起
    public void OnPointerUp(PointerEventData eventData)
    {
        //判斷是否生成建築
        if (buildingPreviewInstance == null) return;

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask))
        {
            //放置建築於選擇位置
            player.CmdTryPlaceBuilding(building.GetId(), hit.point);
        }

        Destroy(buildingPreviewInstance);
    }

    //玩家按住滑鼠並未鬆開時
    private void UpdateBuildingPreview()
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask)) return;

        buildingPreviewInstance.transform.position = hit.point;

        if (!buildingPreviewInstance.activeSelf)
        {
            buildingPreviewInstance.SetActive(true);
        }
        Color color = player.CanPlaceBuilding(buildingCollider, hit.point) ? Color.green : Color.red;
        buildingRendererInstance.material.SetColor("_BaseColor", color);
    }


}

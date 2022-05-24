using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 根據相機視角調整旋轉組件（ui 血條） 
/// </summary>
public class FaceCamera : MonoBehaviour
{
    private Transform mainCameraTransform;
    void Start()
    {
        mainCameraTransform = Camera.main.transform;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.LookAt(
            transform.position + mainCameraTransform.rotation * Vector3.forward,
        mainCameraTransform.rotation * Vector3.up);
    }
}

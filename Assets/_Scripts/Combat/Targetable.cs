using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
/// <summary>
/// 可攻擊的 
/// </summary>
public class Targetable : NetworkBehaviour
{
    [SerializeField] private Transform aimAtPoint = null;
    public Transform GetAimAtPoint() { return aimAtPoint; }

}

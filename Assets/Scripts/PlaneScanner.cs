using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;


public class PlaneScanner : MonoBehaviour
{
    public ARPlaneManager planeManager;

    private void Update()
    {
        foreach (var plane in planeManager.trackables)
        {
            //����� ũ���� ������ plane��
            if(plane.size.x * plane.size.y > 0.5f)
            {
                Debug.Log($"������ Plane ��ġ:{plane.transform.position} ũ��: {plane.size}");
            }
        }
    }
}

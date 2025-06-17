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
            //충분한 크기의 평평한 plane만
            if(plane.size.x * plane.size.y > 0.5f)
            {
                Debug.Log($"감지된 Plane 위치:{plane.transform.position} 크기: {plane.size}");
            }
        }
    }
}

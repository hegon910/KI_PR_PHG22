using UnityEngine;

public class GhostVisibilityController : MonoBehaviour
{
    public Material ghostMaterial; // 귀신의 머테리얼

    void Update()
    {
        Vector3 camPos = Camera.main.transform.position;
        Vector3 camForward = Camera.main.transform.forward;

        // 클리핑 평면 = 카메라 앞쪽 평면
        Vector3 normal = camForward;
        float d = -Vector3.Dot(normal, camPos);
        Vector4 plane = new Vector4(normal.x, normal.y, normal.z, d);

        ghostMaterial.SetVector("_ClippingPlane", plane);
    }
}
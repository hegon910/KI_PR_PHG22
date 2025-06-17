using UnityEngine;

public class GhostVisibilityController : MonoBehaviour
{
    public Material ghostMaterial; // �ͽ��� ���׸���

    void Update()
    {
        Vector3 camPos = Camera.main.transform.position;
        Vector3 camForward = Camera.main.transform.forward;

        // Ŭ���� ��� = ī�޶� ���� ���
        Vector3 normal = camForward;
        float d = -Vector3.Dot(normal, camPos);
        Vector4 plane = new Vector4(normal.x, normal.y, normal.z, d);

        ghostMaterial.SetVector("_ClippingPlane", plane);
    }
}
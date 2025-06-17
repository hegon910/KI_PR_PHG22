using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PlaneRecorder : MonoBehaviour
{
    public ARPlaneManager planeManager;

    private float minYRecorded = float.MaxValue;

    //  허용 높이 편차를 늘려서 자잘한 Y차이 허용 (0.2f → 0.4f)
    private float allowedHeightOffset = 0.4f;

    public List<ARPlane> validFloorPlanes = new List<ARPlane>();
    private HashSet<ARPlane> usedPlanes = new HashSet<ARPlane>();

    void Update()
    {
        

        validFloorPlanes.Clear();
        minYRecorded = float.MaxValue;

        // Step 1: 가장 낮은 바닥 Plane 찾기
        foreach (var plane in planeManager.trackables)
        {
            if (plane.alignment != PlaneAlignment.HorizontalUp) continue;

            if (plane.size.x * plane.size.y < 0.5f) continue;

            float y = plane.transform.position.y;
            if (y < minYRecorded)
                minYRecorded = y;

           
        }

        // Step 2: 기준 바닥에 가까운 Plane만 필터링
        foreach (var plane in planeManager.trackables)
        {
            if (plane.alignment != PlaneAlignment.HorizontalUp) continue;
            if (plane.size.x * plane.size.y < 0.5f) continue;

            float y = plane.transform.position.y;
            if (Mathf.Abs(y - minYRecorded) > allowedHeightOffset) continue;

            //  평면 기울기가 너무 큰 것도 제외
            if (Mathf.Abs(plane.transform.up.y - 1f) > 0.1f) continue;

            validFloorPlanes.Add(plane);
        }

        Debug.Log($"[PlaneRecorder] valid floor count: {validFloorPlanes.Count}");
    }

    public Vector3 GetRandomSpawnPoint()
    {
        if (validFloorPlanes.Count == 0) return Vector3.zero;

        ARPlane chosen = validFloorPlanes[Random.Range(0, validFloorPlanes.Count)];
        return chosen.transform.position + Vector3.up * 0.02f; //  약간만 띄움 (0.05 → 0.02)
    }

    public Vector3 GetSpawnPointHiddenFromCamera(float minDistance = 2.0f, float maxViewAngle = 30f)
    {
        List<ARPlane> candidates = new List<ARPlane>();
        Transform cam = Camera.main.transform;

        foreach (var plane in validFloorPlanes)
        {
            if (usedPlanes.Contains(plane)) continue;

            Vector3 spawnPoint = plane.transform.position + Vector3.up * 0.02f; //  center → transform.position

            Vector3 toSpawn = spawnPoint - cam.position;
            float distance = toSpawn.magnitude;
            if (distance < minDistance) continue;

            float angle = Vector3.Angle(cam.forward, toSpawn.normalized);
            if (angle < maxViewAngle) continue;

            //  Raycast로 벽 등 장애물에 가려져 있는지 확인
            if (Physics.Raycast(cam.position, toSpawn.normalized, out RaycastHit hit, distance))
                continue;

            int nearbyWallCount = 0;
            float checkDistance = 0.4f;
            Vector3[] directions = {Vector3.forward, Vector3.back, Vector3.left, Vector3.right
            };

            foreach(var dir in directions)
            {
                if (Physics.Raycast(spawnPoint, dir, checkDistance)) nearbyWallCount++;
            }
            if(nearbyWallCount > 2) continue; // 4방향 중 3개 이상 벽이 있으면 제외

            if (Physics.CheckSphere(spawnPoint + Vector3.up * 0.5f, 0.3f)) continue;

            candidates.Add(plane);
        }

        if (candidates.Count == 0) return Vector3.zero;

        ARPlane chosenPlane = candidates[Random.Range(0, candidates.Count)];
        usedPlanes.Add(chosenPlane);
        return chosenPlane.transform.position + Vector3.up * 0.02f;
    }
}
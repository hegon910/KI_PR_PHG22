using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Unity.Collections;
using System.Collections.Generic;
using System.Linq;

public class HorrorGameFSM : MonoBehaviour
{
    public HorrorGameState currentState = HorrorGameState.Idle;
    [SerializeField] private PlaneRecorder planeRecorder;

    [Header("Timing Settings")]
    [SerializeField] private float minScanTime = 10f;
    [SerializeField] private int minFloorPlanes = 5;

    [Header("Ghost Settings")]
    public GameObject ghostPrefab;
    private GameObject currentGhost;
    private float ghostWatchTimer;
    private int ghostTriggerCount;

    [Header("AR Components")]
    public ARRaycastManager raycastManager;
    public LayerMask obstacleMask;

    private float stateTimer;
    private List<ARRaycastHit> hits = new();
    [SerializeField] private TMPro.TextMeshProUGUI debugText;

    #region Unity Life‑cycle
    private void Start()
    {
        Log("초기 상태 : Idle");
    }

    private void Update()
    {
        stateTimer += Time.deltaTime;

        switch (currentState)
        {
            case HorrorGameState.Idle:
                if (PlayerStartedWalking())
                    TransitionTo(HorrorGameState.ScanningRoom);
                break;

            case HorrorGameState.ScanningRoom:
                if (stateTimer >= minScanTime && planeRecorder.validFloorPlanes.Count >= minFloorPlanes)
                    TransitionTo(HorrorGameState.GhostPlanted);
                Log($"[스캔중] Time: {stateTimer:F1}/{minScanTime} | Planes: {planeRecorder.validFloorPlanes.Count}/{minFloorPlanes}");
                break;

            case HorrorGameState.GhostPlanted:
                SpawnGhostIfNeeded();
                TrackGhostLooking();
                break;

            case HorrorGameState.GameOver:
                break;
        }
    }
    #endregion

    #region State Helpers
    void SpawnGhostIfNeeded()
    {
        if (currentGhost != null || ghostPrefab == null) return;

        foreach (var plane in planeRecorder.validFloorPlanes)
        {
            if (!IsHorizontal(plane)) continue;
            if (!IsPlaneVisibleToCamera(plane)) continue;

            Vector3 spawnPos = GetPlaneVisualCenter(plane);
            if (!IsPositionClearAround(spawnPos, 1.0f)) continue;

            Quaternion rotation = Quaternion.LookRotation(Camera.main.transform.forward);
            rotation = Quaternion.Euler(0, rotation.eulerAngles.y, 0);

            currentGhost = Instantiate(ghostPrefab, spawnPos, rotation);
            Log("귀신 등장");
            return;
        }

        Log("귀신 등장 가능한 장소 없음");
    }

    void TrackGhostLooking()
    {
        if (currentGhost == null) return;

        if (IsPlayerLookingAtGhost(currentGhost.transform))
        {
            ghostWatchTimer += Time.deltaTime;
            if (ghostWatchTimer >= 1.5f)
            {
                ghostTriggerCount++;
                Log($"귀신 인지 횟수 : {ghostTriggerCount}");

                if (ghostTriggerCount >= 3)
                {
                    TransitionTo(HorrorGameState.GameOver);
                }
                else
                {
                    Destroy(currentGhost);
                    ghostWatchTimer = 0f;
                    TransitionTo(HorrorGameState.GhostPlanted);
                }
            }
        }
        else
        {
            ghostWatchTimer = 0f;
        }
    }
    #endregion

    #region Utility Methods
    bool IsPlaneVisibleToCamera(ARPlane plane)
    {
        Vector3 viewportPoint = Camera.main.WorldToViewportPoint(plane.transform.position);
        return viewportPoint.z > 0 && viewportPoint.x >= 0 && viewportPoint.x <= 1 && viewportPoint.y >= 0 && viewportPoint.y <= 1;
    }

    bool IsHorizontal(ARPlane plane)
    {
        return Vector3.Dot(plane.normal, Vector3.up) > 0.9f;
    }

    bool IsPositionClearAround(Vector3 position, float distance)
    {
        Vector3[] directions =
        {
            Vector3.forward, Vector3.back,
            Vector3.right, Vector3.left,
            Vector3.up, Vector3.down
        };

        foreach (var dir in directions)
        {
            if (Physics.Raycast(position, dir, distance, obstacleMask))
                return false;
        }
        return true;
    }

    Vector3 GetPlaneVisualCenter(ARPlane plane)
    {
        if (!plane.boundary.IsCreated || plane.boundary.Length == 0)
            return plane.center;

        Vector2 avg = plane.boundary.Aggregate(Vector2.zero, (acc, v) => acc + v) / plane.boundary.Length;
        return plane.transform.TransformPoint(new Vector3(avg.x, 0, avg.y));
    }

    bool IsPlayerLookingAtGhost(Transform ghost)
    {
        Vector3 toGhost = (ghost.position - Camera.main.transform.position).normalized;
        float dot = Vector3.Dot(Camera.main.transform.forward, toGhost);
        return dot > Mathf.Cos(30f * Mathf.Deg2Rad); // 30도 이내만 인식
    }

    void TransitionTo(HorrorGameState nextState)
    {
        Log($"State Changed : {currentState} -> {nextState}");
        currentState = nextState;
        stateTimer = 0f;
    }

    bool PlayerStartedWalking() => true; // 실제 구현 필요

    void Log(string msg)
    {
        Debug.Log(msg);
        if (debugText != null)
            debugText.text += $"\n{msg}";
    }
    #endregion
}

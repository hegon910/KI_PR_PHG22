// HorrorGameFSM.cs (리팩토링 + 걸어오기 연출 + 페이드아웃 처리)

using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections.Generic;
using System.Linq;

public class HorrorGameFSM : MonoBehaviour
{
    public HorrorGameState currentState = HorrorGameState.Idle;
    [SerializeField] private PlaneRecorder planeRecorder;

    [Header("Timing Settings")]
    [SerializeField] private float minScanTime = 10f;
    [SerializeField] private int minFloorPlanes = 5;
    [SerializeField] private CanvasGroup fadePanel; 
    [SerializeField] private float screenFadeSpeed = 1f;

    [Header("Ghost Settings")]
    public GameObject ghostPrefab;
    private GameObject currentGhost;
    private float ghostWatchTimer;
    private int ghostTriggerCount;
    private Vector3? lastGhostPosition;
    [SerializeField] private float minDistanceBetweenGhosts = 2.5f;
    [SerializeField] private float approachSpeed = 0.5f;
    [SerializeField] private float fadeOutDistance = 0.5f;

    [Header("AR Components")]
    public LayerMask obstacleMask;
    private float stateTimer;
    [SerializeField] private TMPro.TextMeshProUGUI debugText;

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
                if (currentGhost != null)
                    MoveGhostTowardCamera();
                break;
        }
    }

    void SpawnGhostIfNeeded()
    {
        if (currentGhost != null || ghostPrefab == null) return;

        Vector3 spawnPos = planeRecorder.GetSpawnPointHiddenFromCamera(2.5f, 30f);
        if (spawnPos == Vector3.zero)
        {
            Log("귀신 생성 실패: 적절한 위치 없음");
            return;
        }
        if (IsNearWall(spawnPos))
            return;

        Quaternion rotation = Quaternion.LookRotation(Camera.main.transform.forward);
        rotation = Quaternion.Euler(0, rotation.eulerAngles.y, 0);
        currentGhost = Instantiate(ghostPrefab, spawnPos, rotation);
        lastGhostPosition = spawnPos;
        Log("귀신 미리 등장 (존재만 함)");
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

                var audio = currentGhost.GetComponent<AudioSource>();
                if (audio != null && !audio.isPlaying)
                    audio.Play();

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

    void MoveGhostTowardCamera()
    {
        if (currentGhost == null) return;

        Transform cam = Camera.main.transform;
        Vector3 direction = (cam.position - currentGhost.transform.position).normalized;
        direction.y = 0f;
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            currentGhost.transform.rotation = Quaternion.Slerp(currentGhost.transform.rotation, lookRotation, Time.deltaTime * 5f);
        }

        Animator animator = currentGhost.GetComponentInChildren<Animator>();
        if (animator != null)
            animator.SetBool("Walking", true);

        currentGhost.transform.position = Vector3.MoveTowards(
            currentGhost.transform.position,
            cam.position,
            approachSpeed * Time.deltaTime
        );

        if (fadePanel != null)
        {
            fadePanel.alpha = Mathf.MoveTowards(fadePanel.alpha, 1f, screenFadeSpeed * Time.deltaTime);
        }

        float distance = Vector3.Distance(currentGhost.transform.position, cam.position);

        if (TryGetGhostMaterial(out Material ghostMat))
        {
            Color color = ghostMat.color;
            color.a = Mathf.Clamp01(distance / fadeOutDistance);
            ghostMat.color = color;
        }

        if (distance <= 0.1f)
        {
            Log("게임 오버: 귀신이 도달함");
            Destroy(currentGhost);
        }
    }

    bool IsNearWall(Vector3 spawnPoint, float minWallDistance = 0.4f)
    {
        foreach (var wall in planeRecorder.verticalPlanes)
        {
            float dist = Vector3.Distance(spawnPoint, wall.transform.position);
            if (dist < minWallDistance)
                return true;
        }
        return false;
    }
    bool TryGetGhostMaterial(out Material mat)
    {
        mat = null;
        if (currentGhost == null) return false;
        var renderer = currentGhost.GetComponentInChildren<Renderer>();
        if (renderer == null) return false;
        mat = renderer.material;
        return true;
    }

    bool IsPlayerLookingAtGhost(Transform ghost)
    {
        Vector3 toGhost = (ghost.position - Camera.main.transform.position).normalized;
        float dot = Vector3.Dot(Camera.main.transform.forward, toGhost);
        return dot > Mathf.Cos(30f * Mathf.Deg2Rad);
    }

    void TransitionTo(HorrorGameState nextState)
    {
        Log($"State Changed : {currentState} -> {nextState}");
        currentState = nextState;
        stateTimer = 0f;
    }

    bool PlayerStartedWalking() => true; // 구현 필요 시 교체

    void Log(string msg)
    {
        Debug.Log(msg);
        if (debugText != null)
            debugText.text += $"\n{msg}";
    }
}
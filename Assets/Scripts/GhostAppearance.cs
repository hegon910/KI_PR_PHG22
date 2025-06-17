using UnityEngine;

public class GhostAppearance : MonoBehaviour
{
    [SerializeField] private float delay = 0.5f;        // Plane 안정화 대기 시간
    [SerializeField] private float fadeDuration = 1f;   // 등장 페이드 시간

    private SkinnedMeshRenderer skinnedRenderer;
    private Material ghostMaterial;

    void Start()
    {
        skinnedRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        if (skinnedRenderer == null)
        {
            Debug.LogWarning("SkinnedMeshRenderer가 없습니다.");
            return;
        }

        ghostMaterial = skinnedRenderer.material;
        Color color = ghostMaterial.GetColor("_BaseColor");
        color.a = 0f;
        ghostMaterial.SetColor("_BaseColor", color);

        transform.localScale = Vector3.zero;
        Invoke(nameof(StartAppearance), delay);
    }

    System.Collections.IEnumerator AppearRoutine()
    {
        float timer = 0f;
        Vector3 targetScale = Vector3.one;
        Color color = ghostMaterial.GetColor("_BaseColor");

        while (timer < fadeDuration)
        {
            float t = timer / fadeDuration;

            transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, t);
            color.a = t;
            ghostMaterial.SetColor("_BaseColor", color);

            timer += Time.deltaTime;
            yield return null;
        }

        transform.localScale = targetScale;
        color.a = 1f;
        ghostMaterial.SetColor("_BaseColor", color);
    }

    void StartAppearance()
    {
        StartCoroutine(AppearRoutine());
    }

  
}
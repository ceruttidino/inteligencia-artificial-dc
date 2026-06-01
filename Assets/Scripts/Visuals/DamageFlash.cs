using System.Collections;
using UnityEngine;

public class DamageFlash : MonoBehaviour
{
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Color flashColor = Color.red;
    [SerializeField] private float flashTime = 0.1f;

    private Color originalColor;
    private Material material;

    private void Awake()
    {
        material = targetRenderer.material;
        originalColor = material.color;
    }

    public void Flash()
    {
        StopAllCoroutines();
        StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        material.color = flashColor;

        yield return new WaitForSeconds(flashTime);

        material.color = originalColor;
    }
}

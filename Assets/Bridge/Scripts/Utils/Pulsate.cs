using System.Collections;
using UnityEngine;

 // Attached to the guide game unit prefab, serialized in BridgeTypeSO.

public class Pulsate : MonoBehaviour
{
    [SerializeField] float speed = 1.0f;
    [SerializeField] float maxOpacity = 1.0f;
    [SerializeField] float minOpacity = 0.0f;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        StartCoroutine(PulsateOpacity());
    }

    IEnumerator PulsateOpacity()
    {
        while (true)
        {
            // Gradually increase the opacity to 1.0
            for (float i = minOpacity; i <= maxOpacity; i += Time.deltaTime * speed)
            {
                SetOpacity(i);
                yield return null;
            }

            // Gradually decrease the opacity to 0.0
            for (float i = maxOpacity; i >= minOpacity; i -= Time.deltaTime * speed)
            {
                SetOpacity(i);
                yield return null;
            }
        }
    }

    void SetOpacity(float opacity)
    {
        Color color = spriteRenderer.color;
        color.a = opacity;
        spriteRenderer.color = color;
    }
}
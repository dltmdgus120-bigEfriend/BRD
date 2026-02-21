using UnityEngine;
using System.Collections;

public class FocusIndicator : MonoBehaviour
{
    public float blinkInterval = 0.1f; // ±ôºýÀÌ´Â ¼Óµµ
    private SpriteRenderer sr;
    private Coroutine blinkCoroutine;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    
    void OnEnable()
    {
        if (sr != null) sr.enabled = true;
        blinkCoroutine = StartCoroutine(BlinkRoutine());
    }

    
    void OnDisable()
    {
        if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);
        if (sr != null) sr.enabled = false;
    }

    IEnumerator BlinkRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(blinkInterval);
            if (sr != null) sr.enabled = !sr.enabled;
        }
    }
}
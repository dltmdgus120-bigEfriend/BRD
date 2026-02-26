using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    public TextMeshPro textMesh;

    [Header("애니메이션 설정")]
    public float floatSpeed = 2f;
    public float fadeTime = 1f;

    private float timer;
    private Color textColor;
    private Vector3 originalScale;
    private Camera mainCam; // 카메라 캐싱

    void Awake()
    {
        originalScale = transform.localScale;
        mainCam = Camera.main;
    }

    // 풀에서 꺼내져서 켜질 때마다 카메라를 정면으로 바라보게 세팅!
    void OnEnable()
    {
        if (mainCam == null) mainCam = Camera.main;
        if (mainCam != null)
        {
            transform.rotation = mainCam.transform.rotation;
        }
    }

    public void Setup(int damageAmount, AttackType type, bool isCrit = false)
    {
        textMesh.text = damageAmount.ToString();

        switch (type)
        {
            case AttackType.Physical: textMesh.color = new Color(1f, 0.2f, 0.2f); break;
            case AttackType.Magic: textMesh.color = new Color(0.2f, 0.5f, 1f); break;
            case AttackType.Fixed: textMesh.color = Color.white; break;
        }

        if (isCrit)
        {
            transform.localScale = originalScale * 1.5f;
            textMesh.fontStyle = FontStyles.Bold;
        }
        else
        {
            transform.localScale = originalScale;
            textMesh.fontStyle = FontStyles.Normal;
        }

        textColor = textMesh.color;
        timer = fadeTime;
    }

    void Update()
    {
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        timer -= Time.deltaTime;
        textColor.a = timer / fadeTime;
        textMesh.color = textColor;

        if (timer <= 0)
        {
            PoolManager.Instance.ReturnDamagePopup(gameObject);
        }
    }
}

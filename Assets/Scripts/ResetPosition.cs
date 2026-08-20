using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class ResetPosition : MonoBehaviour
{
    private RectTransform rectTransform;

    private Vector2 startAnchoredPosition;
    private Quaternion startRotation;
    private Vector3 startScale;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        startAnchoredPosition = rectTransform.anchoredPosition;
        startRotation = rectTransform.localRotation;
        startScale = rectTransform.localScale;
    }

    private void OnEnable()
    {
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }

        rectTransform.anchoredPosition = startAnchoredPosition;
        rectTransform.localRotation = startRotation;
        rectTransform.localScale = startScale;
    }
}

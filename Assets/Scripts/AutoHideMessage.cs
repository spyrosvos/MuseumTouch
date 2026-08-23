using System.Collections;
using UnityEngine;

/// <summary>
/// Μπαίνει πάνω στο ίδιο το μήνυμα.
/// Μόλις το μήνυμα ενεργοποιηθεί, ξεκινάει αντίστροφη μέτρηση και σβήνει μόνο του.
/// Το κουμπί χρειάζεται μόνο ένα SetActive(true) - τίποτα άλλο.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class AutoHideMessage : MonoBehaviour
{
    [Header("Timing")]
    [Tooltip("Πόσο μένει ορατό πριν αρχίσει να σβήνει")]
    [SerializeField] private float visibleDuration = 2f;

    [Tooltip("Πόσο διαρκεί το σβήσιμο")]
    [SerializeField] private float fadeDuration = 0.4f;

    [Header("Appearance")]
    [Tooltip("Πόσο διαρκεί το φανέρωμα στην αρχή")]
    [SerializeField] private float fadeInDuration = 0.2f;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        StopAllCoroutines();
        StartCoroutine(ShowThenHide());
    }

    private IEnumerator ShowThenHide()
    {
        // Φανέρωμα
        yield return Fade(0f, 1f, fadeInDuration);

        // Παραμονή
        yield return new WaitForSeconds(visibleDuration);

        // Σβήσιμο
        yield return Fade(1f, 0f, fadeDuration);

        gameObject.SetActive(false);
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        canvasGroup.alpha = from;

        if (duration <= 0f)
        {
            canvasGroup.alpha = to;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            canvasGroup.alpha = Mathf.Lerp(from, to, t * t * (3f - 2f * t));
            yield return null;
        }

        canvasGroup.alpha = to;
    }
}

using UnityEngine;

/// <summary>
/// Κρατάει το αντικείμενο μέσα στα όρια μιας περιοχής.
/// Δουλεύει ανεξάρτητα από το ποιο script το μετακινεί, γιατί διορθώνει
/// τη θέση στο LateUpdate - δηλαδή αφού έχουν τρέξει όλα τα υπόλοιπα.
/// Λειτουργεί σωστά και όταν το δωμάτιο είναι στραμμένο.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class ClampInsideArea : MonoBehaviour
{
    [Header("Bounds")]
    [Tooltip("Η περιοχή μέσα στην οποία επιτρέπεται να κινείται. Συνήθως το content του δωματίου.")]
    [SerializeField] private RectTransform boundsArea;

    [Header("Margin")]
    [Tooltip("Πόσο επιτρέπεται να ξεπεράσει τα όρια. Αρνητικό = μένει πιο μέσα.")]
    [SerializeField] private float padding = 0f;

    [Tooltip("ON: αρκεί να φαίνεται ένα μέρος της κάρτας. OFF: πρέπει να χωράει ολόκληρη.")]
    [SerializeField] private bool allowPartiallyOutside = false;

    [Tooltip("Αν είναι ON και η κάρτα βγαίνει μερικώς έξω, πόσο τουλάχιστον πρέπει να φαίνεται")]
    [SerializeField] private float minimumVisible = 80f;

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        // Αν δεν ορίστηκε περιοχή, χρησιμοποιεί τον γονέα του γονέα (συνήθως το content)
        if (boundsArea == null && transform.parent != null && transform.parent.parent != null)
            boundsArea = transform.parent.parent as RectTransform;
    }

    private void LateUpdate()
    {
        if (boundsArea == null)
            return;

        Rect area = boundsArea.rect;

        // Θέση της κάρτας μέσα στο σύστημα συντεταγμένων της περιοχής
        Vector2 local = boundsArea.InverseTransformPoint(rectTransform.position);

        Vector2 size = rectTransform.rect.size;
        float halfWidth = size.x * 0.5f;
        float halfHeight = size.y * 0.5f;

        if (allowPartiallyOutside)
        {
            halfWidth = Mathf.Max(0f, halfWidth - (size.x - minimumVisible));
            halfHeight = Mathf.Max(0f, halfHeight - (size.y - minimumVisible));
        }

        float minX = area.xMin + halfWidth - padding;
        float maxX = area.xMax - halfWidth + padding;
        float minY = area.yMin + halfHeight - padding;
        float maxY = area.yMax - halfHeight + padding;

        // Αν η κάρτα είναι μεγαλύτερη από την περιοχή, την κεντράρουμε
        Vector2 clamped;
        clamped.x = (minX > maxX) ? area.center.x : Mathf.Clamp(local.x, minX, maxX);
        clamped.y = (minY > maxY) ? area.center.y : Mathf.Clamp(local.y, minY, maxY);

        if (clamped != local)
            rectTransform.position = boundsArea.TransformPoint(clamped);
    }
}

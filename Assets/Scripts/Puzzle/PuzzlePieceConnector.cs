using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class PuzzlePieceConnector : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("References")]
    [SerializeField] private RectTransform puzzleArea;
    [SerializeField] private RectTransform ownSnapPoint;
    [SerializeField] private RectTransform otherSnapPoint;
    [SerializeField] private PuzzlePieceConnector otherPiece;
    [SerializeField] private PuzzlePairManager puzzleManager;

    [Header("Settings")]
    [Tooltip("Σε μονάδες UI του PuzzleArea - ίδιες μονάδες με το Width/Height στον Inspector")]
    [SerializeField] private float snapDistance = 50f;
    [SerializeField] private bool lockAfterSnap = true;

    private RectTransform rectTransform;
    private Camera uiCamera;
    private Vector2 pointerOffset;
    private bool snapped = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        // Βρίσκει μόνο του τη σωστή κάμερα ανάλογα με το render mode του Canvas
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            uiCamera = canvas.worldCamera;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (snapped)
            return;

        // Το κομμάτι που σέρνεις πάει μπροστά από το άλλο
        transform.SetAsLastSibling();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            puzzleArea,
            eventData.position,
            uiCamera,
            out Vector2 localPointerPosition
        );

        pointerOffset = rectTransform.anchoredPosition - localPointerPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (snapped)
            return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            puzzleArea,
            eventData.position,
            uiCamera,
            out Vector2 localPointerPosition
        );

        Vector2 newPosition = localPointerPosition + pointerOffset;
        rectTransform.anchoredPosition = ClampInsidePuzzleArea(newPosition);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (snapped || ownSnapPoint == null || otherSnapPoint == null)
            return;

        // Αν ο παίκτης δεν έχει βρει ακόμα το άλλο κομμάτι, δεν γίνεται τίποτα
        if (!otherSnapPoint.gameObject.activeInHierarchy)
            return;

        Vector2 ownLocal = puzzleArea.InverseTransformPoint(ownSnapPoint.position);
        Vector2 otherLocal = puzzleArea.InverseTransformPoint(otherSnapPoint.position);

        if (Vector2.Distance(ownLocal, otherLocal) <= snapDistance)
            SnapToOtherPiece(ownLocal, otherLocal);
    }

    private void SnapToOtherPiece(Vector2 ownLocal, Vector2 otherLocal)
    {
        // Μετακίνηση σε local units -> δουλεύει σε κάθε ανάλυση
        rectTransform.anchoredPosition += (otherLocal - ownLocal);

        MarkAsSnapped();

        // Κλειδώνει και το άλλο κομμάτι, ώστε να μην ξεκολλάει η ένωση
        if (otherPiece != null)
            otherPiece.MarkAsSnapped();

        // Στέλνει τη θέση της ραφής, ώστε η κάρτα να γεννηθεί ακριβώς εκεί
        if (puzzleManager != null)
            puzzleManager.CompletePuzzle(ownSnapPoint.position);
    }

    public void MarkAsSnapped()
    {
        snapped = true;

        if (!lockAfterSnap)
            return;

        Image image = GetComponent<Image>();
        if (image != null)
            image.raycastTarget = false;
    }

    private Vector2 ClampInsidePuzzleArea(Vector2 position)
    {
        if (puzzleArea == null)
            return position;

        Rect area = puzzleArea.rect;

        float halfWidth = rectTransform.rect.width * 0.5f;
        float halfHeight = rectTransform.rect.height * 0.5f;

        position.x = Mathf.Clamp(position.x, area.xMin + halfWidth, area.xMax - halfWidth);
        position.y = Mathf.Clamp(position.y, area.yMin + halfHeight, area.yMax - halfHeight);

        return position;
    }
}

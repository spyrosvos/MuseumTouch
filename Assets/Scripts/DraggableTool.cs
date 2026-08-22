using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class DraggableTool : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Identity")]
    [Tooltip("Περιγραφικό όνομα - φαίνεται μόνο στον Editor")]
    [SerializeField] private string toolName = "Tool";

    [Header("Drag layer")]
    [Tooltip("’δειο RectTransform στο τέλος του Canvas. Το εργαλείο πηγαίνει εκεί όσο σέρνεται, ώστε να ζωγραφίζεται πάνω από όλα.")]
    [SerializeField] private RectTransform dragLayer;

    [Header("Feel")]
    [SerializeField] private float dragScale = 1.1f;
    [SerializeField] private float returnDuration = 0.2f;

    [Header("After successful use")]
    [Tooltip("ON: το εργαλείο εξαφανίζεται μετά τη χρήση. OFF: γυρίζει στην εργαλειοθήκη.")]
    [SerializeField] private bool consumeAfterUse = false;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Camera uiCamera;

    private Transform homeParent;
    private Vector2 homePosition;
    private int homeSiblingIndex;
    private bool returning = false;

    public string ToolName => toolName;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            uiCamera = canvas.worldCamera;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (returning)
            return;

        // Θυμόμαστε πού ανήκει, για να μπορεί να γυρίσει
        homeParent = transform.parent;
        homePosition = rectTransform.anchoredPosition;
        homeSiblingIndex = transform.GetSiblingIndex();

        if (dragLayer != null)
            transform.SetParent(dragLayer, true);

        transform.SetAsLastSibling();
        rectTransform.localScale = Vector3.one * dragScale;

        // ’υλο, ώστε το raycast να "βλέπει" τι υπάρχει από κάτω
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (returning)
            return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)transform.parent,
            eventData.position,
            uiCamera,
            out Vector2 localPoint
        );

        rectTransform.anchoredPosition = localPoint;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        rectTransform.localScale = Vector3.one;

        // Τι βρίσκεται κάτω από το δάχτυλο τη στιγμή που αφήνουμε;
        ToolTarget target = null;
        GameObject hovered = eventData.pointerCurrentRaycast.gameObject;

        if (hovered != null)
            target = hovered.GetComponentInParent<ToolTarget>();

        if (target != null && target.TryUseTool(this))
        {
            if (consumeAfterUse)
            {
                gameObject.SetActive(false);
                RestoreHomeTransform();
                return;
            }
        }

        StartCoroutine(ReturnHome());
    }

    private System.Collections.IEnumerator ReturnHome()
    {
        returning = true;

        Vector2 start = rectTransform.anchoredPosition;
        Vector3 startWorld = rectTransform.position;

        RestoreHomeTransform();

        // Ξεκινάμε από εκεί που το άφησε ο παίκτης και γλιστράμε πίσω
        rectTransform.position = startWorld;
        Vector2 from = rectTransform.anchoredPosition;

        float elapsed = 0f;
        while (elapsed < returnDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / returnDuration);
            float eased = t * t * (3f - 2f * t);

            rectTransform.anchoredPosition = Vector2.Lerp(from, homePosition, eased);
            yield return null;
        }

        rectTransform.anchoredPosition = homePosition;
        returning = false;
    }

    private void RestoreHomeTransform()
    {
        if (homeParent == null)
            return;

        transform.SetParent(homeParent, false);
        transform.SetSiblingIndex(homeSiblingIndex);
        rectTransform.anchoredPosition = homePosition;
        rectTransform.localScale = Vector3.one;
    }
}

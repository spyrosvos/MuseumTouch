using System.Collections.Generic;
using UnityEngine;

public class TouchableWindow : MonoBehaviour {
    [SerializeField] private RectTransform canvasRectTransform;

    private RectTransform rectTransform;

    private readonly Dictionary<int, Vector2> activeTouches = new();
    private readonly Dictionary<int, Vector2> previousTouches = new();

    private void Awake() {
        rectTransform = GetComponent<RectTransform>();
    }

    public bool CanAcceptTouch() {
        return activeTouches.Count < 2;
    }

    public void BeginTouch(int fingerId, Vector2 screenPosition) {
        if (!CanAcceptTouch())
            return;

        activeTouches[fingerId] = screenPosition;
        previousTouches[fingerId] = screenPosition;

        BringToFront();
    }

    public void MoveTouch(int fingerId, Vector2 screenPosition) {
        if (!activeTouches.ContainsKey(fingerId))
            return;

        previousTouches[fingerId] = activeTouches[fingerId];
        activeTouches[fingerId] = screenPosition;
    }

    public void EndTouch(int fingerId) {
        activeTouches.Remove(fingerId);
        previousTouches.Remove(fingerId);
    }

    public void UpdateWindowTransform() {
        if (activeTouches.Count == 1) {
            DragWithOneTouch();
        }
        else if (activeTouches.Count == 2) {
            RotateWithTwoTouches();
        }
    }

    private void DragWithOneTouch() {
        foreach (var pair in activeTouches) {
            int fingerId = pair.Key;

            Vector2 currentScreen = activeTouches[fingerId];
            Vector2 previousScreen = previousTouches[fingerId];

            Vector2 currentLocal;
            Vector2 previousLocal;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRectTransform,
                currentScreen,
                null,
                out currentLocal
            );

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRectTransform,
                previousScreen,
                null,
                out previousLocal
            );

            Vector2 delta = currentLocal - previousLocal;
            rectTransform.anchoredPosition += delta;

            break;
        }
    }

    private void RotateWithTwoTouches() {
        var enumerator = activeTouches.GetEnumerator();

        enumerator.MoveNext();
        int firstId = enumerator.Current.Key;

        enumerator.MoveNext();
        int secondId = enumerator.Current.Key;

        Vector2 previousVector = previousTouches[secondId] - previousTouches[firstId];
        Vector2 currentVector = activeTouches[secondId] - activeTouches[firstId];

        float previousAngle = Mathf.Atan2(previousVector.y, previousVector.x) * Mathf.Rad2Deg;
        float currentAngle = Mathf.Atan2(currentVector.y, currentVector.x) * Mathf.Rad2Deg;

        float deltaAngle = currentAngle - previousAngle;

        rectTransform.Rotate(0f, 0f, deltaAngle);
    }

    private void BringToFront() {
        transform.SetAsLastSibling();
    }
}
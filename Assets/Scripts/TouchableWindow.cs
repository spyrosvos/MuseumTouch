using System.Collections.Generic;
using UnityEngine;

public class TouchableWindow : MonoBehaviour {
    [Header("References")]
    [SerializeField] private RectTransform canvasRectTransform;

    [Header("Drag feel")]
    [SerializeField] private float dragResponsiveness = 25f;
    [SerializeField] private float linearDamping = 7f;
    [SerializeField] private float maxLinearSpeed = 3000f;

    [Header("Rotation feel")]
    [SerializeField] private float rotationResponsiveness = 25f;
    [SerializeField] private float angularDamping = 7f;
    [SerializeField] private float maxAngularSpeed = 720f;

    [Header("Two-finger behaviour")]
    [SerializeField] private bool allowTwoFingerTranslation = true;

    [Header("Stop thresholds")]
    [SerializeField] private float stopLinearSpeed = 5f;
    [SerializeField] private float stopAngularSpeed = 2f;

    private RectTransform rectTransform;
    private Canvas parentCanvas;
    private Camera uiCamera;

    private readonly Dictionary<int, Vector2> currentTouches = new();
    private readonly Dictionary<int, Vector2> lastProcessedTouches = new();

    private Vector2 linearVelocity;
    private float angularVelocity;

    private void Awake() {
        rectTransform = GetComponent<RectTransform>();

        parentCanvas = GetComponentInParent<Canvas>();

        if (canvasRectTransform == null && parentCanvas != null) {
            canvasRectTransform = parentCanvas.GetComponent<RectTransform>();
        }

        if (parentCanvas != null && parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay) {
            uiCamera = parentCanvas.worldCamera;
        }
        else {
            uiCamera = null;
        }
    }

    private void Update() {
        float dt = Time.unscaledDeltaTime;

        if (dt <= 0f)
            return;

        if (currentTouches.Count == 1) {
            UpdateOneFingerDrag(dt);
        }
        else if (currentTouches.Count == 2) {
            UpdateTwoFingerRotation(dt);
        }
        else {
            UpdateInertia(dt);
        }

        MarkActiveTouchesAsProcessed();
    }

    public bool CanAcceptTouch() {
        return currentTouches.Count < 2;
    }

    public void BeginTouch(int fingerId, Vector2 screenPosition) {
        if (!CanAcceptTouch())
            return;

        currentTouches[fingerId] = screenPosition;
        lastProcessedTouches[fingerId] = screenPosition;

        BringToFront();

        // When a visitor grabs a moving window, make it feel controlled.
        // You can comment these out if you want it to preserve momentum while grabbed.
        linearVelocity *= 0.35f;
        angularVelocity *= 0.35f;
    }

    public void MoveTouch(int fingerId, Vector2 screenPosition) {
        if (!currentTouches.ContainsKey(fingerId))
            return;

        currentTouches[fingerId] = screenPosition;
    }

    public void EndTouch(int fingerId) {
        currentTouches.Remove(fingerId);
        lastProcessedTouches.Remove(fingerId);

        // Prevent jumps when going from two fingers back to one finger.
        foreach (int remainingFingerId in currentTouches.Keys) {
            lastProcessedTouches[remainingFingerId] = currentTouches[remainingFingerId];
        }
    }

    private void UpdateOneFingerDrag(float dt) {
        int fingerId = GetFirstTouchId();

        Vector2 currentScreen = currentTouches[fingerId];
        Vector2 previousScreen = lastProcessedTouches[fingerId];

        Vector2 deltaLocal = ScreenDeltaToCanvasLocal(previousScreen, currentScreen);

        Vector2 desiredVelocity = deltaLocal / dt;
        desiredVelocity = Vector2.ClampMagnitude(desiredVelocity, maxLinearSpeed);

        linearVelocity = SmoothVector(linearVelocity, desiredVelocity, dragResponsiveness, dt);

        rectTransform.anchoredPosition += linearVelocity * dt;

        // With only one finger, we do not want old rotation inertia to continue.
        angularVelocity = SmoothFloat(angularVelocity, 0f, rotationResponsiveness, dt);
    }

    private void UpdateTwoFingerRotation(float dt) {
        GetTwoTouchIds(out int firstId, out int secondId);

        Vector2 firstCurrent = currentTouches[firstId];
        Vector2 secondCurrent = currentTouches[secondId];

        Vector2 firstPrevious = lastProcessedTouches[firstId];
        Vector2 secondPrevious = lastProcessedTouches[secondId];

        if (allowTwoFingerTranslation) {
            Vector2 previousCenter = (firstPrevious + secondPrevious) * 0.5f;
            Vector2 currentCenter = (firstCurrent + secondCurrent) * 0.5f;

            Vector2 deltaLocal = ScreenDeltaToCanvasLocal(previousCenter, currentCenter);

            Vector2 desiredVelocity = deltaLocal / dt;
            desiredVelocity = Vector2.ClampMagnitude(desiredVelocity, maxLinearSpeed);

            linearVelocity = SmoothVector(linearVelocity, desiredVelocity, dragResponsiveness, dt);

            rectTransform.anchoredPosition += linearVelocity * dt;
        }
        else {
            linearVelocity = SmoothVector(linearVelocity, Vector2.zero, dragResponsiveness, dt);
        }

        Vector2 previousVector = secondPrevious - firstPrevious;
        Vector2 currentVector = secondCurrent - firstCurrent;

        float previousAngle = Mathf.Atan2(previousVector.y, previousVector.x) * Mathf.Rad2Deg;
        float currentAngle = Mathf.Atan2(currentVector.y, currentVector.x) * Mathf.Rad2Deg;

        float deltaAngle = Mathf.DeltaAngle(previousAngle, currentAngle);

        float desiredAngularVelocity = deltaAngle / dt;
        desiredAngularVelocity = Mathf.Clamp(desiredAngularVelocity, -maxAngularSpeed, maxAngularSpeed);

        angularVelocity = SmoothFloat(
            angularVelocity,
            desiredAngularVelocity,
            rotationResponsiveness,
            dt
        );

        rectTransform.Rotate(0f, 0f, angularVelocity * dt);
    }

    private void UpdateInertia(float dt) {
        rectTransform.anchoredPosition += linearVelocity * dt;
        rectTransform.Rotate(0f, 0f, angularVelocity * dt);

        linearVelocity = DampVector(linearVelocity, linearDamping, dt);
        angularVelocity = DampFloat(angularVelocity, angularDamping, dt);

        if (linearVelocity.magnitude < stopLinearSpeed)
            linearVelocity = Vector2.zero;

        if (Mathf.Abs(angularVelocity) < stopAngularSpeed)
            angularVelocity = 0f;
    }

    private Vector2 ScreenDeltaToCanvasLocal(Vector2 previousScreen, Vector2 currentScreen) {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRectTransform,
            previousScreen,
            uiCamera,
            out Vector2 previousLocal
        );

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRectTransform,
            currentScreen,
            uiCamera,
            out Vector2 currentLocal
        );

        return currentLocal - previousLocal;
    }

    private void MarkActiveTouchesAsProcessed() {
        foreach (int fingerId in currentTouches.Keys) {
            lastProcessedTouches[fingerId] = currentTouches[fingerId];
        }
    }

    private int GetFirstTouchId() {
        foreach (int fingerId in currentTouches.Keys)
            return fingerId;

        return -1;
    }

    private void GetTwoTouchIds(out int firstId, out int secondId) {
        firstId = -1;
        secondId = -1;

        foreach (int fingerId in currentTouches.Keys) {
            if (firstId == -1) {
                firstId = fingerId;
            }
            else {
                secondId = fingerId;
                return;
            }
        }
    }

    private Vector2 SmoothVector(Vector2 current, Vector2 target, float sharpness, float dt) {
        float t = 1f - Mathf.Exp(-sharpness * dt);
        return Vector2.Lerp(current, target, t);
    }

    private float SmoothFloat(float current, float target, float sharpness, float dt) {
        float t = 1f - Mathf.Exp(-sharpness * dt);
        return Mathf.Lerp(current, target, t);
    }

    private Vector2 DampVector(Vector2 value, float damping, float dt) {
        return value * Mathf.Exp(-damping * dt);
    }

    private float DampFloat(float value, float damping, float dt) {
        return value * Mathf.Exp(-damping * dt);
    }

    private void BringToFront() {
        transform.SetAsLastSibling();
    }
}
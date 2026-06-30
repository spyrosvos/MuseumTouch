using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class MultitouchWindowRouter : MonoBehaviour {
    [SerializeField] private GraphicRaycaster graphicRaycaster;
    [SerializeField] private EventSystem eventSystem;

    private readonly Dictionary<int, TouchableWindow> touchOwners = new();

    private void OnEnable() {
        EnhancedTouchSupport.Enable();
    }

    private void OnDisable() {
        EnhancedTouchSupport.Disable();
    }

    private void Update() {
        // 1. Process active touches.
        foreach (var touch in Touch.activeTouches) {
            int fingerId = touch.finger.index;

            if (touch.began) {
                TryAssignTouch(fingerId, touch.screenPosition);
            }
            else if (touchOwners.TryGetValue(fingerId, out TouchableWindow window)) {
                window.MoveTouch(fingerId, touch.screenPosition);
            }
        }

        // 2. Remove touches that are no longer active.
        CleanupEndedTouches();

        // 3. Let each touched window update itself.
        HashSet<TouchableWindow> updatedWindows = new HashSet<TouchableWindow>();

        foreach (var window in touchOwners.Values) {
            if (updatedWindows.Add(window))
                window.UpdateWindowTransform();
        }
    }

    private void TryAssignTouch(int fingerId, Vector2 screenPosition) {
        WindowDragArea dragArea = GetTopmostDragArea(screenPosition);

        if (dragArea == null)
            return;

        TouchableWindow window = dragArea.window;

        if (window == null || !window.CanAcceptTouch())
            return;

        touchOwners[fingerId] = window;
        window.BeginTouch(fingerId, screenPosition);
    }

    private WindowDragArea GetTopmostDragArea(Vector2 screenPosition) {
        PointerEventData pointerData = new PointerEventData(eventSystem) {
            position = screenPosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        graphicRaycaster.Raycast(pointerData, results);

        if (results.Count == 0)
            return null;

        GameObject topObject = results[0].gameObject;

        // If the topmost thing is an interactive UI control, do not manipulate the window.
        if (IsUIControl(topObject))
            return null;

        return topObject.GetComponentInParent<WindowDragArea>();
    }

    private bool IsUIControl(GameObject obj) {
        return obj.GetComponentInParent<Button>() != null ||
               obj.GetComponentInParent<Toggle>() != null ||
               obj.GetComponentInParent<Slider>() != null ||
               obj.GetComponentInParent<Dropdown>() != null ||
               obj.GetComponentInParent<Scrollbar>() != null ||
               obj.GetComponentInParent<InputField>() != null;
    }

    private void CleanupEndedTouches() {
        List<int> endedFingers = new List<int>();

        foreach (int fingerId in touchOwners.Keys) {
            if (!IsFingerStillActive(fingerId))
                endedFingers.Add(fingerId);
        }

        foreach (int fingerId in endedFingers) {
            TouchableWindow window = touchOwners[fingerId];
            window.EndTouch(fingerId);
            touchOwners.Remove(fingerId);
        }
    }

    private bool IsFingerStillActive(int fingerId) {
        foreach (var touch in Touch.activeTouches) {
            if (touch.finger.index == fingerId)
                return true;
        }

        return false;
    }
}
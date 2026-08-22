using UnityEngine;

public class ToggleGameObject : MonoBehaviour
{
    [Header("Object to open / close")]
    [SerializeField] private GameObject targetObject;

    [Header("Optional icons")]
    [SerializeField] private GameObject closedIcon;
    [SerializeField] private GameObject openIcon;

    private void Start()
    {
        if (targetObject != null)
        {
            UpdateIcons(targetObject.activeSelf);
        }
    }

    public void Toggle()
    {
        if (targetObject == null)
            return;

        bool newState = !targetObject.activeSelf;

        targetObject.SetActive(newState);
        UpdateIcons(newState);
    }

    public void Open()
    {
        if (targetObject == null)
            return;

        targetObject.SetActive(true);
        UpdateIcons(true);
    }

    public void Close()
    {
        if (targetObject == null)
            return;

        targetObject.SetActive(false);
        UpdateIcons(false);
    }

    private void UpdateIcons(bool isOpen)
    {
        if (closedIcon != null)
            closedIcon.SetActive(!isOpen);

        if (openIcon != null)
            openIcon.SetActive(isOpen);
    }
}
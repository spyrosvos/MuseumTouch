using UnityEngine;
using UnityEngine.Events;

public class ToolTarget : MonoBehaviour
{
    [Header("Which tool does this accept?")]
    [SerializeField] private DraggableTool acceptedTool;

    [Header("Can it be used more than once?")]
    [SerializeField] private bool singleUse = true;

    [Header("What happens")]
    [Tooltip("Σωστό εργαλείο - εδώ συνδέεις ό,τι πρέπει να γίνει")]
    public UnityEvent onCorrectTool;

    [Tooltip("Λάθος εργαλείο - π.χ. ένας ήχος αποτυχίας")]
    public UnityEvent onWrongTool;

    private bool used = false;

    /// <summary>Καλείται από το DraggableTool. Επιστρέφει true αν το εργαλείο έγινε δεκτό.</summary>
    public bool TryUseTool(DraggableTool tool)
    {
        if (singleUse && used)
            return false;

        if (acceptedTool == null || tool != acceptedTool)
        {
            onWrongTool?.Invoke();
            return false;
        }

        used = true;
        onCorrectTool?.Invoke();
        return true;
    }
}
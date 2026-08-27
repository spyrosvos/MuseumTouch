using System.Collections;
using UnityEngine;

/// <summary>
/// Εμφανίζει κάρτες οδηγιών, την καθεμία ΜΟΝΟ την πρώτη φορά.
///
/// Μπαίνει σε αντικείμενο που είναι ΠΑΝΤΑ ενεργό - π.χ. στο GameManager.
///
/// Κάθε κουμπί του παιχνιδιού καλεί ShowHint(αριθμός). Μπορείς να συνδέσεις
/// όσα κουμπιά θέλεις στον ίδιο αριθμό - θα εμφανιστεί μόνο στο πρώτο πάτημα.
/// </summary>
public class TutorialHints : MonoBehaviour
{
    [System.Serializable]
    public class Hint
    {
        [Tooltip("Μόνο για να το αναγνωρίζεις εσύ στον Inspector")]
        public string label;

        [Tooltip("Η κάρτα οδηγιών")]
        public GameObject card;

        [Tooltip("Πόσο αργεί να εμφανιστεί, ώστε να προλάβει ο παίκτης να δει τι έγινε")]
        public float delay = 0.6f;

        [HideInInspector] public bool alreadyShown;
    }

    [Header("Οι κάρτες, με τη σειρά")]
    [SerializeField] private Hint[] hints;

    /// <summary>Καλείται από τα On Click των κουμπιών, με τον αριθμό της κάρτας.</summary>
    public void ShowHint(int index)
    {
        if (index < 0 || hints == null || index >= hints.Length)
        {
            Debug.LogWarning($"[TutorialHints] Δεν υπάρχει κάρτα με αριθμό {index}", this);
            return;
        }

        Hint hint = hints[index];

        if (hint.alreadyShown || hint.card == null)
            return;

        hint.alreadyShown = true;
        StartCoroutine(ShowAfterDelay(hint));
    }

    private IEnumerator ShowAfterDelay(Hint hint)
    {
        if (hint.delay > 0f)
            yield return new WaitForSeconds(hint.delay);

        if (hint.card != null)
            hint.card.SetActive(true);
    }

    /// <summary>Για δοκιμές - ξαναεπιτρέπει να εμφανιστούν όλες.</summary>
    public void ResetAllHints()
    {
        if (hints == null)
            return;

        foreach (Hint hint in hints)
            hint.alreadyShown = false;
    }
}

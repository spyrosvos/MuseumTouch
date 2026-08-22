using System.Collections;
using UnityEngine;

public class PuzzlePairManager : MonoBehaviour
{
    [Header("Puzzle pieces")]
    [SerializeField] private GameObject[] objectsToHideWhenComplete;
    [Tooltip("Τα CanvasGroup των κομματιών - για να σβήνουν ομαλά αντί να εξαφανίζονται")]
    [SerializeField] private CanvasGroup[] pieceCanvasGroups;

    [Header("Evidence card")]
    [SerializeField] private RectTransform evidenceCard;
    [SerializeField] private CanvasGroup evidenceCardGroup;

    [Header("Timing")]
    [Tooltip("Πόσο βλέπει ο παίκτης το ενωμένο παζλ πριν αρχίσει η μεταμόρφωση")]
    [SerializeField] private float holdBeforeMorph = 0.8f;
    [SerializeField] private float morphDuration = 0.55f;
    [Tooltip("Μέγεθος της κάρτας τη στιγμή που γεννιέται, ως ποσοστό του τελικού")]
    [SerializeField] private float birthScale = 0.55f;

    private bool completed = false;

    /// <summary>Καλείται από τον PuzzlePieceConnector μόλις κουμπώσουν τα κομμάτια.</summary>
    public void CompletePuzzle(Vector3 joinWorldPosition)
    {
        if (completed)
            return;

        completed = true;
        StartCoroutine(MorphRoutine(joinWorldPosition));
    }

    private IEnumerator MorphRoutine(Vector3 joinWorldPosition)
    {
        // 1. Ο παίκτης απολαμβάνει το ενωμένο παζλ
        yield return new WaitForSeconds(holdBeforeMorph);

        if (evidenceCard == null)
        {
            HidePieces();
            yield break;
        }

        // 2. Η κάρτα γεννιέται πάνω στη ραφή, μικρή και αόρατη - και εκεί μένει
        evidenceCard.position = joinWorldPosition;
        evidenceCard.localScale = Vector3.one * birthScale;

        if (evidenceCardGroup != null)
        {
            evidenceCardGroup.alpha = 0f;
            // ’υλη όσο μεγαλώνει, ώστε να μην την πιάσει ο παίκτης εν κινήσει
            evidenceCardGroup.blocksRaycasts = false;
        }

        evidenceCard.gameObject.SetActive(true);

        // 3. Cross-fade: τα κομμάτια σβήνουν την ώρα που η κάρτα δυναμώνει
        float elapsed = 0f;
        while (elapsed < morphDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / morphDuration);
            float eased = t * t * (3f - 2f * t);   // smoothstep

            foreach (CanvasGroup group in pieceCanvasGroups)
            {
                if (group != null)
                    group.alpha = 1f - eased;
            }

            if (evidenceCardGroup != null)
                evidenceCardGroup.alpha = eased;

            evidenceCard.localScale = Vector3.one * Mathf.Lerp(birthScale, 1f, eased);

            yield return null;
        }

        // 4. Καθάρισμα - τα τελικά νούμερα ακριβώς στη θέση τους
        evidenceCard.localScale = Vector3.one;

        if (evidenceCardGroup != null)
        {
            evidenceCardGroup.alpha = 1f;
            evidenceCardGroup.blocksRaycasts = true;   // τώρα γίνεται πιάσιμη
        }

        HidePieces();
    }

    private void HidePieces()
    {
        foreach (GameObject obj in objectsToHideWhenComplete)
        {
            if (obj != null)
                obj.SetActive(false);
        }
    }
}

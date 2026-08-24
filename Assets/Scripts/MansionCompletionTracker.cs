using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

/// <summary>
/// Παρακολουθεί την πρόοδο ενός αρχοντικού και αλλάζει το εικονίδιό του στον χάρτη.
///
/// Δύο τρόποι να δηλώσεις τι μετράει - μπορείς να χρησιμοποιήσεις όποιον βολεύει
/// σε κάθε εύρημα, ή και τους δύο μαζί:
///
///   itemsToFind        -> πράγματα που ΣΒΗΝΟΥΝ όταν ολοκληρωθούν (hotspots, States)
///   polaroidsToAppear  -> πράγματα που ΑΝΑΒΟΥΝ όταν ολοκληρωθούν (polaroids στο σημειωματάριο)
///
/// Μπαίνει σε αντικείμενο που είναι ΠΑΝΤΑ ενεργό - π.χ. στο GameManager.
/// </summary>
public class MansionCompletionTracker : MonoBehaviour
{
    [Header("Πρέπει να ΣΒΗΣΟΥΝ όλα")]
    [Tooltip("Hotspots, States, κομμάτια παζλ - ό,τι απενεργοποιείται όταν ολοκληρωθεί")]
    [SerializeField] private GameObject[] itemsToFind;

    [Header("Πρέπει να ΑΝΑΨΟΥΝ όλα")]
    [Tooltip("Τα polaroids στο σημειωματάριο - ανάβουν με το κουμπί ΑΠΟΘΗΚΕΥΣΗ")]
    [SerializeField] private GameObject[] polaroidsToAppear;

    [Header("Το εικονίδιο στον χάρτη")]
    [SerializeField] private Image mapIcon;

    [Tooltip("ON: σκουραίνει το χρώμα. OFF: αλλάζει το sprite.")]
    [SerializeField] private bool useColorInsteadOfSprite = true;

    [Tooltip("Το χρώμα όταν ολοκληρωθεί. Χρησιμοποιείται μόνο αν το παραπάνω είναι ON.")]
    [SerializeField] private Color completedColor = new Color(0.45f, 0.45f, 0.45f, 1f);

    [Tooltip("Η σκούρα εικόνα. Χρησιμοποιείται μόνο αν το παραπάνω είναι OFF.")]
    [SerializeField] private Sprite completedSprite;

    [Header("Extra (προαιρετικά)")]
    [Tooltip("Ό,τι άλλο θέλεις να συμβεί - π.χ. ένας ήχος")]
    public UnityEvent onCompleted;

    [Header("Performance")]
    [SerializeField] private float checkInterval = 0.5f;

    private bool completed = false;
    private float timer = 0f;

    private void Update()
    {
        if (completed)
            return;

        timer += Time.deltaTime;
        if (timer < checkInterval)
            return;

        timer = 0f;

        if (IsComplete())
            Complete();
    }

    private bool IsComplete()
    {
        bool hasAnything = false;

        // Όλα αυτά πρέπει να έχουν σβήσει
        if (itemsToFind != null)
        {
            foreach (GameObject item in itemsToFind)
            {
                if (item == null)
                    continue;

                hasAnything = true;

                // activeSelf, όχι activeInHierarchy - το δωμάτιο μπορεί να είναι κλειστό
                if (item.activeSelf)
                    return false;
            }
        }

        // Όλα αυτά πρέπει να έχουν ανάψει
        if (polaroidsToAppear != null)
        {
            foreach (GameObject polaroid in polaroidsToAppear)
            {
                if (polaroid == null)
                    continue;

                hasAnything = true;

                if (!polaroid.activeSelf)
                    return false;
            }
        }

        // Άδειες λίστες δεν σημαίνουν ολοκλήρωση
        return hasAnything;
    }

    private void Complete()
    {
        completed = true;

        if (mapIcon != null)
        {
            if (useColorInsteadOfSprite)
                mapIcon.color = completedColor;
            else if (completedSprite != null)
                mapIcon.sprite = completedSprite;
        }

        onCompleted?.Invoke();
    }
}

using UnityEngine;

public class PotionSelectionManager : MonoBehaviour
{
    public static PotionSelectionManager Instance { get; private set; }

    private Potion currentlySelectedPotion;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    public void SelectPotion(Potion newPotion)
    {
        if (currentlySelectedPotion == newPotion) return;

        // Deselect yang lama
        if (currentlySelectedPotion != null)
            currentlySelectedPotion.SetSelected(false);

        // Select yang baru
        currentlySelectedPotion = newPotion;
        currentlySelectedPotion.SetSelected(true);
    }
}

using UnityEngine;
using UnityEngine.UI;
using BridgePackage;

public class ChangeImage : MonoBehaviour
{
    public Sprite newSprite; // The new sprite to change to
    private Image imageComponent;

    private void Start()
    {
        // Get the Image component on this GameObject
        imageComponent = GetComponent<Image>();

        if (imageComponent == null)
        {
            Debug.LogError("No Image component found on the GameObject. Please attach this script to a UI Image.");
            return;
        }

        // Listen for the BridgeIsComplete event
        Debug.Log("ChangeImage: Subscribing to BridgeIsComplete event.");
        BridgeEvents.BridgeIsComplete += ChangeSprite;
    }
        
    
    private void OnDestroy()
    {
        // Unsubscribe from the event to avoid memory leaks
        Debug.Log("ChangeImage: Unsubscribing from BridgeIsComplete event.");
        BridgeEvents.BridgeIsComplete -= ChangeSprite;
    }

    private void ChangeSprite()
    {
        Debug.Log("ChangeSprite: Event triggered, changing sprite.");
        if (imageComponent != null && newSprite != null)
        {
            imageComponent.sprite = newSprite;
            Debug.Log("ChangeSprite: Sprite changed successfully.");
        }
        else
        {
            Debug.LogWarning("ChangeSprite: Either imageComponent or newSprite is null.");
        }
    }
}
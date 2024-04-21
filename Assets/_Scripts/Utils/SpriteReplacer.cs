using UnityEngine;
using UnityEngine.UI;

public class SpriteReplacer : MonoBehaviour {
    public Sprite newSprite; // The new sprite that you want to use
    [SerializeField] private bool isVertical = true; // To store if the sprite is vertical

    void Start() {
        Sprite currentSprite = GetComponent<SpriteRenderer>().sprite;
        float numOfOriginalPixelPerUnit = currentSprite.pixelsPerUnit;
        float originalWidth = currentSprite.rect.width;
        float originalHeight = currentSprite.rect.height;
        var localScale = transform.localScale;
        Debug.Log("Unity units ratio: " + " Height: " +
                  (originalHeight * localScale.y) / numOfOriginalPixelPerUnit +
                  " Width: " + (originalWidth * localScale.x) / numOfOriginalPixelPerUnit);

        /* Normalize the width and height of the original sprite to unity units*/
        float normalizedWidthPerUnit = (originalWidth * localScale.x) / numOfOriginalPixelPerUnit;
        float normalizedHeightPerUnit = (originalHeight * localScale.y) / numOfOriginalPixelPerUnit;
        // Debug.Log("Normalized Width: " + normalizedWidthPerUnit + " Normalized Height: " + normalizedHeightPerUnit);

        // Get the original rotation of the sprite
        float originalRotationZ = transform.localRotation.eulerAngles.z;

        // Get the new sprite's width and height
        float numOfNewPixelPerUnit = newSprite.pixelsPerUnit;
        var width = newSprite.rect.width;
        var height = newSprite.rect.height;
        // Debug.Log("Width: " + width + " Height: " + height);

        // make the new sprite fill the same space of unity's units in the scene as the original sprite
        float newSpriteWidthSize = normalizedWidthPerUnit * numOfNewPixelPerUnit;
        Debug.Log(newSpriteWidthSize);
        float newSpriteHeightSize = normalizedHeightPerUnit * numOfNewPixelPerUnit;
        Debug.Log(newSpriteHeightSize);

        // Calculate the ratio of the new sprite's width and height to the original sprite's width and height
        float ratioWidth = newSpriteWidthSize / (isVertical ? height : width);
        float ratioHeight = newSpriteHeightSize / (isVertical ? width : height);
        gameObject.GetComponent<SpriteRenderer>().sprite = newSprite;

        /* Set the new sprite's scale and rotation
         If the sprite is vertical, rotate it by 90 degrees*/
        if (isVertical) {
            gameObject.transform.localScale = new Vector3(ratioHeight, ratioWidth, 1);
            transform.rotation = Quaternion.Euler(0, 0, -90 + originalRotationZ); // Rotate the sprite
        }
        else {
            gameObject.transform.localScale = new Vector3(ratioWidth, ratioHeight, 1);
        }
    }
}
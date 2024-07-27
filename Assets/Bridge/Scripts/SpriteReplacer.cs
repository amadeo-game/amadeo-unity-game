using UnityEngine;
using BridgePackage;
public class SpriteReplacer : MonoBehaviour {
    public static void ReplaceSprite(SpriteUnit spriteUnit, GameObject bridgeUnit, bool isMirorred = false) {
        Sprite newSprite = spriteUnit.Sprite; // The new sprite
        bool isVertical = spriteUnit.IsVertical;
        Sprite currentSprite = bridgeUnit.GetComponent<SpriteRenderer>().sprite;
        float numOfOriginalPixelPerUnit = currentSprite.pixelsPerUnit;
        float originalWidth = currentSprite.rect.width;
        float originalHeight = currentSprite.rect.height;
        var localScale = bridgeUnit.transform.localScale;

        /* Normalize the width and height of the original sprite to unity units*/
        float normalizedWidthPerUnit = (originalWidth * localScale.x) / numOfOriginalPixelPerUnit;
        float normalizedHeightPerUnit = (originalHeight * localScale.y) / numOfOriginalPixelPerUnit;

        // Get the original rotation of the sprite
        float originalRotationZ = bridgeUnit.transform.localRotation.eulerAngles.z;

        // Get the new sprite's width and height
        float numOfNewPixelPerUnit = newSprite.pixelsPerUnit;
        var width = newSprite.rect.width;
        var height = newSprite.rect.height;

        // make the new sprite fill the same space of unity's units in the scene as the original sprite
        float newSpriteWidthSize = normalizedWidthPerUnit * numOfNewPixelPerUnit;
        float newSpriteHeightSize = normalizedHeightPerUnit * numOfNewPixelPerUnit;

        // Calculate the ratio of the new sprite's width and height to the original sprite's width and height
        float ratioWidth = newSpriteWidthSize / (isVertical ? height : width);
        float ratioHeight = newSpriteHeightSize / (isVertical ? width : height);
        bridgeUnit.GetComponent<SpriteRenderer>().sprite = newSprite;

        /* Set the new sprite's scale and rotation
         If the sprite is vertical, rotate it by 90 degrees*/
        if (isVertical) {
            bridgeUnit.transform.localScale = new Vector3(ratioHeight, ratioWidth, 1);
            bridgeUnit.transform.rotation =
                Quaternion.Euler(0, 0, 90 + originalRotationZ * (isMirorred ? -1 : 1)); // Rotate the sprite
        }
        else {
            bridgeUnit.transform.localScale = new Vector3(ratioWidth, ratioHeight, 1);
        }
    }

}
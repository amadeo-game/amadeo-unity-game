using UnityEngine;

[System.Serializable]
public class SpriteUnit {
    // if the sprite is vertical or horizontal visually from the sprite sheet, needed for sprite replacement
    public bool IsVertical = true;
    public Sprite Sprite;
}
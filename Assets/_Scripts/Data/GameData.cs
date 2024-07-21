using UnityEngine;
using UnityEngine.UIElements;
using System;


// stores consumable data (resources)
[Serializable]
public class GameData {
    public float musicVolume;
    public float sfxVolume;

    // non-functional, used for saving SettingsScreen values
    public bool isSlideToggled;
    public bool isToggled;
    public string dropdownSelection;
    public int buttonSelection;


    // constructor, starting values
    public GameData() {
        // settings
        this.musicVolume = 80f;
        this.sfxVolume = 80f;
        this.dropdownSelection = "Item1";
        this.buttonSelection = 2;

        this.isSlideToggled = false;
        this.isToggled = false;
    }

    public string ToJson() {
        return JsonUtility.ToJson(this);
    }

    public void LoadJson(string jsonFilepath) {
        JsonUtility.FromJsonOverwrite(jsonFilepath, this);
    }
}
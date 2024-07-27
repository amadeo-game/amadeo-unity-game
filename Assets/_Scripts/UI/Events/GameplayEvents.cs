using System;

public static class GameplayEvents {
    // Triggered after the bridge is complete
    public static Action WinScreenShown;

    // Triggered after the bridge is collapse
    public static Action LoseScreenShown;

    public static Action<GameData> SettingsUpdated;

    // Use the gameData to load the music and sfx volume levels
    public static Action SettingsLoaded;

    // Notify listeners to pause after delay in seconds
    public static Action<float> GamePaused;

    // Resume the game from the PauseScreen
    public static Action GameResumed;

    // Quit the game from the PauseScreen
    public static Action GameQuit;

    // Restart the game from the PauseScreen
    public static Action GameRestarted;

    // Adjust the music volume during gameplay
    public static Action<float> MusicVolumeChanged;

    // Adjust the sound effects volume during gameplay
    public static Action<float> SfxVolumeChanged;
}
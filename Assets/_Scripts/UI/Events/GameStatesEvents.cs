using System;
using UnityEngine.UIElements;


public static class GameStatesEvents {
    // Triggered after the bridge is complete
    public static Action GameSessionInitialized;

    // Triggered after the bridge is collapse
    public static Action GameSessionStarted;
    //
    // // Restart the game from the PauseScreen
    // public static Action GameRestarted;
    
    public static Action GameSessionEnded;
}
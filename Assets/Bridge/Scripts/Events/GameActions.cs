using System;

namespace BridgePackage
{
    public static class GameActions{
        public static Action PlaySession;
        public static Action PlayTrial;
        
        // ---- Mid-Game Interaction Events ----
        
        
        // called when the game is paused  
        public static Action PauseGameAction;
        
        // called when the game is resumed
        public static Action ResumeGameAction;
        
        // Restart the game
        public static Action RestartGameAction;
        
        // Enable the guide units
        public static Action EnableGameInteraction;
        // Initiate collapsing the bridge

        public static Action ForceDestroyBridge;
        
        // Game Over Action
        public static Action GameFinishedAction;
    }
}

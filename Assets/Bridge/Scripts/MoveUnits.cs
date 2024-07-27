using UnityEngine;

namespace BridgePackage
{
    public class MoveUnits : MonoBehaviour
    {
        private GameObject[] playerUnits;

        /// <summary>
        /// Sets the player units for the game.
        /// </summary>
        /// <param name="units">Array of player units.</param>
        public void SetPlayerUnits(GameObject[] units)
        {
            playerUnits = units;
        }

        /// <summary>
        /// Applies the specified forces to the player units.
        /// </summary>
        /// <param name="forces">Array of forces to apply.</param>
        public void ApplyForces(double[] forces)
        {
            if (playerUnits == null || playerUnits.Length == 0) return;

            for (int i = 0; i < playerUnits.Length; i++)
            {
                if (playerUnits[i] != null)
                {
                    // Apply force to the unit
                    // This is where you would add your logic to apply forces to the units.
                }
            }
        }

        /// <summary>
        /// Disables control for the player units.
        /// </summary>
        public void DisableControl()
        {
            if (playerUnits == null) return;

            foreach (var unit in playerUnits)
            {
                if (unit != null)
                {
                    // Disable unit control logic
                }
            }
        }
    }
}
using UnityEngine;

namespace BridgePackage
{
    /// <summary>
    /// Manages player units, applying forces and enabling/disabling control.
    /// </summary>
    public class UnitsControl : MonoBehaviour
    {
        private GameObject[] playerUnits;
        private SessionData sessionData;
        private float[] leftHandHighestY = new float[5];
        private float[] rightHandHighestY = new float[5];
        private bool hasPlayed;
        private bool isLeft;

        /// <summary>
        /// Sets the player units for the game.
        /// </summary>
        /// <param name="units">Array of player units.</param>
        public void SetPlayerUnits(GameObject[] units)
        {
            playerUnits = units;
            AssignUnitControlScripts();
        }

        /// <summary>
        /// Assigns control scripts to player units if not already assigned.
        /// </summary>
        private void AssignUnitControlScripts()
        {
            foreach (var unit in playerUnits)
            {
                if (unit != null && unit.GetComponent<MoveUnit>() == null)
                {
                    unit.AddComponent<MoveUnit>();
                }
            }
        }

        /// <summary>
        /// Applies the specified forces to the player units.
        /// </summary>
        /// <param name="forces">Array of forces to apply.</param>
        public void ApplyForces(double[] forces)
        {
            double[] forcesByHand = new double[5];
            
            int len = forces.Length;
                for (int i = 5; i < len; i++) {
                    if (isLeft) {
                        forcesByHand[len - i] = forces[i];
                        leftHandHighestY[len - i] = Mathf.Max((float)forces[i], leftHandHighestY[len - i]);
                    }
                    else {
                        forcesByHand[i - 5] = forces[i];
                        rightHandHighestY[i - 5] = Mathf.Max((float)forces[i], rightHandHighestY[i - 5]);
                    }
                }

            
            Debug.Log("UnitsControl :: forces: " + string.Join(", ", forces));
            if (playerUnits == null || playerUnits.Length == 0) return;

            for (int i = 0; i < playerUnits.Length; i++)
            {
                if (playerUnits[i] != null)
                {
                    var moveUnit = playerUnits[i].GetComponent<MoveUnit>();
                    if (moveUnit != null)
                    {
                        moveUnit.ApplyForce((float)forcesByHand[i]);
                    }
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
                    var moveUnit = unit.GetComponent<MoveUnit>();
                    if (moveUnit != null)
                    {
                        moveUnit.DisableControl();
                    }
                }
            }
        }
    }


}

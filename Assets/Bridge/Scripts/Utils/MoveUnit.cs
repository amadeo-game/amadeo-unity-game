using UnityEngine;

namespace BridgePackage
{
    /// <summary>
    /// Controls the individual player unit.
    /// </summary>
    public class MoveUnit : MonoBehaviour
    {
        private bool controlEnabled = true;

        /// <summary>
        /// Applies the specified force to the unit.
        /// </summary>
        /// <param name="force">Force to apply.</param>
        public void ApplyForce(float force)
        {
            if (controlEnabled)
            {
                // Apply the force to the unit (implementation depends on your game logic)
                Debug.Log($"Applying force: {force} to {gameObject.name}");
                Vector2 targetPosition = new Vector2(transform.position.x, force);
                GetComponent<Rigidbody2D>().MovePosition(targetPosition);
            }
        }

        /// <summary>
        /// Disables the control for this unit.
        /// </summary>
        public void DisableControl()
        {
            controlEnabled = false;
            Debug.Log($"{gameObject.name} control disabled.");
        }
    }
}

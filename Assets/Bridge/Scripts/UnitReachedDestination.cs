using System;
using UnityEngine;

namespace BridgePackage
{
    // Attached to each playable game unit prefab, serialized in BridgeTypeSO.
    public class UnitReachedDestination : MonoBehaviour
    {
        private BridgeStateMachine bridgeStateMachine;
        internal FingerUnit fingerUnit;

        /// <summary>
        /// Initializes the UnitReachedDestination with the specified state machine.
        /// </summary>
        /// <param name="stateMachine">The state machine to interact with.</param>
        internal void Initialize(BridgeStateMachine stateMachine)
        {
            bridgeStateMachine = stateMachine ? stateMachine : throw new ArgumentNullException(nameof(stateMachine), "BridgeStateMachine cannot be null.");
        }

        internal void SetFingerUnit(FingerUnit unit)
        {
            fingerUnit = unit;
            GetComponent<MoveUnit>().fingerIndex = (int)unit;

        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            bridgeStateMachine?.UnitPlaced(fingerUnit, true);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            bridgeStateMachine?.UnitPlaced(fingerUnit, false);
        }
    }
}
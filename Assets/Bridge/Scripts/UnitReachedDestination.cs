using System;
using UnityEngine;
using BridgePackage;
public class UnitReachedDestination : MonoBehaviour
{
    private BridgeMediator bridgeMediator;
    internal  FingerUnit fingerUnit;

    internal void Initialize(BridgeMediator mediator)
    {
        bridgeMediator = mediator ? mediator : throw new ArgumentNullException(nameof(mediator), "BridgeMediator cannot be null.");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        bridgeMediator?.UnitPlaced(fingerUnit, true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        bridgeMediator?.UnitPlaced(fingerUnit, false);
    }
}
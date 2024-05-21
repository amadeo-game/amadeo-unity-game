using System;
using UnityEngine;

public class UnitReachedDestination : MonoBehaviour
{
    private BridgeMediator bridgeMediator;
    public FingerUnit fingerUnit;

    public void Initialize(BridgeMediator mediator)
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
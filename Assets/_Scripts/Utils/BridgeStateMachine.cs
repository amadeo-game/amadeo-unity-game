using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum BridgeState {
    Idle,
    Building,
    Built,
    Collapsing,
    Collapsed,
    Winning,
    Win
    
}

public class BridgeStateMachine : MonoBehaviour {
    private BridgeMediator bridgeMediator;
    private Dictionary<FingerUnit, bool> dicUnitPlaced;
    private BridgeState currentState = BridgeState.Idle;

    private void Awake()
    {
        dicUnitPlaced = new Dictionary<FingerUnit, bool>
        {
            { FingerUnit.first, false },
            { FingerUnit.second, false },
            { FingerUnit.third, false },
            { FingerUnit.fourth, false },
            { FingerUnit.fifth, false },
        };
    }

    public void Initialize(BridgeMediator mediator)
    {
        bridgeMediator = mediator ? mediator : throw new ArgumentNullException(nameof(mediator), "BridgeMediator cannot be null.");
        bridgeMediator.OnUnitPlaced += OnUnitPlaced;
    }

    private void OnDestroy()
    {
        if (bridgeMediator != null)
        {
            bridgeMediator.OnUnitPlaced -= OnUnitPlaced;
        }
    }

    private void OnUnitPlaced(FingerUnit fingerUnit, bool isPlaced)
    {
        if (dicUnitPlaced.ContainsKey(fingerUnit))
        {
            dicUnitPlaced[fingerUnit] = isPlaced;
        }

        if (!dicUnitPlaced.Values.Contains(false))
        {
            StartSuccess();
        }
    }

    public void StartBuilding() {
        if (currentState == BridgeState.Idle) {
            currentState = BridgeState.Building;
            bridgeMediator?.BuildStart();
        }
    }

    public void FinishBuilding() {
        if (currentState == BridgeState.Building) {
            currentState = BridgeState.Built;
            bridgeMediator?.BuildComplete();
        }
    }

    public void StartCollapsing() {
        if (currentState == BridgeState.Built) {
            currentState = BridgeState.Collapsing;
            bridgeMediator?.CollapseStart();
        }
    }

    public void CompleteCollapse() {
        if (currentState == BridgeState.Collapsing) {
            currentState = BridgeState.Collapsed;
            bridgeMediator?.CollapseComplete();
        }
    }

    public void StartSuccess() {
        if (currentState == BridgeState.Built) {
            currentState = BridgeState.Winning;
            bridgeMediator?.SuccessStart();
        }
    }

    public void FinishSuccess() {
        if (currentState == BridgeState.Winning) {
            currentState = BridgeState.Win;
            bridgeMediator?.SuccessComplete();
        }
    }

    public void ResetState() {
        if (currentState == BridgeState.Collapsed || currentState == BridgeState.Win) {
            currentState = BridgeState.Idle;
        }
    }
}
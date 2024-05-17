using System;
using UnityEngine;

public enum BridgeState {
    Idle,
    Building,
    Built,
    Collapsing,
    Collapsed,
    Win,
}

public class BridgeStateMachine : MonoBehaviour {
    public event Action OnBuildStart;
    public event Action OnBuildComplete;
    public event Action OnSuccess;
    public event Action OnCollapseStart;
    public event Action OnCollapseComplete;

    private BridgeState currentState = BridgeState.Idle;

    public bool CanBuild() {
        return currentState == BridgeState.Idle;
    }

    public void StartBuilding() {
        if (currentState == BridgeState.Idle) {
            currentState = BridgeState.Building;
            OnBuildStart?.Invoke();
        }
    }

    public void FinishBuilding() {
        if (currentState == BridgeState.Building) {
            currentState = BridgeState.Built;
            OnBuildComplete?.Invoke();
        }
    }

    public void StartCollapsing() {
        if (currentState == BridgeState.Built) {
            currentState = BridgeState.Collapsing;
            OnCollapseStart?.Invoke();
        }
    }

    public void StartSuccess() {
        if (currentState == BridgeState.Built) {
            currentState = BridgeState.Win;
            OnSuccess?.Invoke();
        }
    }

    public void CompleteCollapse() {
        if (currentState == BridgeState.Collapsing) {
            currentState = BridgeState.Collapsed;
            OnCollapseComplete?.Invoke();
        }
    }

    public void ResetState() {
        if (currentState == BridgeState.Collapsed || currentState == BridgeState.Win) {
            currentState = BridgeState.Idle;
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BridgePackage {
    [RequireComponent(typeof(BridgeStateMachine))]
    public class BridgeAPI : MonoBehaviour, IBridgeAPI
    {
        public event Action BridgeBuilt;
        public event Action BridgeCollapsed;
        public event Action BridgeIsComplete;
        
        private BridgeStateMachine bridgeStateMachine;
        
        private void Awake() {
            bridgeStateMachine = GetComponent<BridgeStateMachine>();
        }
    
        public void BuildBridge() {
            bridgeStateMachine.StartBuilding();
        }

        public void CollapseBridge() {
            bridgeStateMachine.StartCollapsing();
        }

        public void PauseBridge() {
            throw new NotImplementedException();
        }

        public void ResetBridge() {
            
        }
    }
}


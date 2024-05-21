using System;
using UnityEngine;

namespace BridgePackage {
    internal  class BridgeMediator : MonoBehaviour
    {
        internal event Action<FingerUnit, bool> OnUnitPlaced;
        internal event Action OnBuildStart;
        internal event Action OnBuildComplete;
        internal event Action OnSuccessStart;
        internal event Action OnSuccessComplete;
        internal event Action OnCollapseStart;
        internal event Action OnCollapseComplete;

        internal  void UnitPlaced(FingerUnit fingerUnit, bool isPlaced)
        {
            OnUnitPlaced?.Invoke(fingerUnit, isPlaced);
        }

        internal  void BuildStart()
        {
            OnBuildStart?.Invoke();
        }

        internal  void BuildComplete()
        {
            OnBuildComplete?.Invoke();
        }

        internal  void SuccessStart()
        {
            OnSuccessStart?.Invoke();
        }

        internal  void SuccessComplete()
        {
            OnSuccessComplete?.Invoke();
        }

        internal  void CollapseStart()
        {
            OnCollapseStart?.Invoke();
        }

        internal  void CollapseComplete()
        {
            OnCollapseComplete?.Invoke();
        }
    }
}
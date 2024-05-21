using System;
using UnityEngine;

public class BridgeMediator : MonoBehaviour
{
    public event Action<FingerUnit, bool> OnUnitPlaced;
    public event Action OnBuildStart;
    public event Action OnBuildComplete;
    public event Action OnSuccessStart;
    public event Action OnSuccessComplete;
    public event Action OnCollapseStart;
    public event Action OnCollapseComplete;

    public void UnitPlaced(FingerUnit fingerUnit, bool isPlaced)
    {
        OnUnitPlaced?.Invoke(fingerUnit, isPlaced);
    }

    public void BuildStart()
    {
        OnBuildStart?.Invoke();
    }

    public void BuildComplete()
    {
        OnBuildComplete?.Invoke();
    }

    public void SuccessStart()
    {
        OnSuccessStart?.Invoke();
    }

    public void SuccessComplete()
    {
        OnSuccessComplete?.Invoke();
    }

    public void CollapseStart()
    {
        OnCollapseStart?.Invoke();
    }

    public void CollapseComplete()
    {
        OnCollapseComplete?.Invoke();
    }
}
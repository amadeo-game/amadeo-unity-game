using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "SubjectData", menuName = "Menu/Subject Data", order = 1)]
public class SubjectData : ScriptableObject {
    private Dictionary<string, bool> flags;
    

    private void OnEnable()
    {
        InitializeFlags();
    }

    private void InitializeFlags()
    {
        flags.Clear();
        flags["ZeroF"] = false;
        flags["MVC"] = false;
        flags["Individuation"] = false;
        flags["Control"] = false;
        flags["Synergy"] = false;
    }

    public bool ZeroF
    {
        get => flags["ZeroF"];
        set
        {
            flags["ZeroF"] = value;
            UpdateFlags("ZeroF");
        }
    }

    public bool MVC
    {
        get => flags["MVC"];
        set
        {
            flags["MVC"] = value;
            UpdateFlags("MVC");
        }
    }

    // Similar properties for other flags...

    private void UpdateFlags(string changedFlag)
    {
        foreach (var flag in flags.Keys)
        {
            if (flag != changedFlag)
                flags[flag] = false;
        }
    }
}
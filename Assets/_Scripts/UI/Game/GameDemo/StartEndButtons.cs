using UnityEngine;
using UnityEngine.UI;

public class StartEndButtons : MonoBehaviour {
    [SerializeField] private Button start;
    [SerializeField] private Button end;
    [SerializeField] private Button success;
    [SerializeField] private GameObject guideKeys;

    private void Start() {
        DisableButtons();
    }

    private void OnEnable() {
        BridgePackage.BridgeAPI.BridgeReady += EnableButtons;
        BridgePackage.BridgeAPI.OnGameStart += () => {
            start.interactable = false;
            success.interactable = true;
            guideKeys.SetActive(true);
        };
        BridgePackage.BridgeAPI.BridgeCollapsed += DisableButtons;
        BridgePackage.BridgeAPI.BridgeIsComplete += DisableButtons;
    }

    public void DisableButtons() {
        start.interactable = false;
        end.interactable = false;
        success.interactable = false;
        guideKeys.SetActive(false);

    }

    private void EnableButtons() {
        start.interactable = true;
        end.interactable = true;
        guideKeys.SetActive(false);

    }
}
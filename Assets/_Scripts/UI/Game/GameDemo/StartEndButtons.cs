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
        BridgePackage.BridgeEvents.BridgeReadyState += EnableButtons;
        BridgePackage.BridgeEvents.StartingGameState += () => {
            Debug.Log("UI Buttons got notified that the game has started.");
            start.interactable = false;
            success.interactable = true;
            if (guideKeys != null) {
                guideKeys?.SetActive(true);
            }
        };
        BridgePackage.BridgeEvents.BridgeCollapsingState += DisableButtons;
        BridgePackage.BridgeEvents.BridgeIsCompletedState += DisableButtons;
    }
    
    public void PressedEndGameButton() {
        start.interactable = false;
        end.interactable = false;
        success.interactable = false;
        if (guideKeys != null) {
            guideKeys?.SetActive(false);
        }    }

    public void DisableButtons() {
        start.interactable = false;
        end.interactable = false;
        success.interactable = false;
        if (guideKeys != null) {
            guideKeys?.SetActive(false);
        }
    }

    private void EnableButtons() {
        start.interactable = true;
        end.interactable = true;
        if (guideKeys != null) {
            guideKeys?.SetActive(false);
        }
    }
}
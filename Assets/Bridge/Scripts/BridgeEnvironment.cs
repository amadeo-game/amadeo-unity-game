using UnityEngine;

namespace BridgePackage
{
    public class BridgeEnvironment : MonoBehaviour
    {
        private int _currentLevel = 0;
        private GameObject _background;

        private void Start() {
            _currentLevel = BridgeDataManager.Level;
            _background = Instantiate(BridgeDataManager.BridgeType.BridgeEnvDecoration);
        }

        private void OnEnable() {
            BridgeEvents.BuildingState += OnBuildingState;
        }

        private void OnDisable() {
            BridgeEvents.BuildingState -= OnBuildingState;
        }

        private void OnBuildingState() {
            Debug.Log("BridgeEnvironment :: OnBuildingState() called.");
            // if (_background == null) {
            //     Debug.Log("Background is null. Returning.");
            //     return;
            // }
            SetBackground();
        }
        private void SetBackground() {
            if (_currentLevel != BridgeDataManager.Level) {
                _currentLevel = BridgeDataManager.Level;
                Destroy(_background);
                _background = Instantiate(BridgeDataManager.BridgeType.BridgeEnvDecoration);
            }
        }
    }
}

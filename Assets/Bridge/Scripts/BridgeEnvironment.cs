using UnityEngine;

namespace BridgePackage
{
    public class BridgeEnvironment : MonoBehaviour
    {
        private int _currentLevel = 1;
        private GameObject _background;

        private void Start() {
            _currentLevel = BridgeDataManager.Level;
            Debug.Log("BridgeEnvironment :: Start() called.");
            
            _background = Instantiate(BridgeDataManager.BridgeType.BridgeEnvDecoration);
        }

        private void OnEnable() {
            BridgeEvents.BuildingState += SetBackground;
        }

        private void OnDisable() {
            BridgeEvents.BuildingState -= SetBackground;
        }
        
        private void SetBackground() {
            _currentLevel = BridgeDataManager.Level;
            Destroy(_background);
            _background = Instantiate(BridgeDataManager.BridgeType.BridgeEnvDecoration);
        }
    }
}

using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "BridgeType", menuName = "Bridge/BridgeType")]
public class BridgeTypeSO : ScriptableObject {
    [SerializeField] private GameObject[] _bridgeEnvDecorations;

    // [SerializeField] private BridgeSpriteCollection bridgeSpritesCollections;
    [SerializeField] private PlayableUnit[] _playableUnitPrefab;

    // prefab for height difference 0
    [SerializeField] private BridgeType[] _bridgeEnvUnit0Prefab;


    [SerializeField] private BridgeType[] _bridgeEnvUnit1Prefab;


    [SerializeField] private BridgeType[] _bridgeEnvUnit2Prefab;


    [SerializeField] private BridgeType[] _bridgeEnvUnit3Prefab;


    [SerializeField] private BridgeType[] _bridgeEnvUnit4Prefab;


    [SerializeField] private BridgeType[] _bridgeEnvUnit5Prefab;


    // public BridgeSpriteCollection BridgeSpritesCollections => bridgeSpritesCollections;
    public PlayableUnit GetPlayableUnitPrefab => _playableUnitPrefab[Random.Range(0, _playableUnitPrefab.Length)];
    public GameObject BridgeEnvDecoration => _bridgeEnvDecorations[Random.Range(0, _bridgeEnvDecorations.Length)];


    /* by asking for an environment unit that will fit the gap of the 2 player units' height differences,
     will randomize all the pre-made options in the unity inspector and return one of them.
     */
    public GameObject GetEnvUnitType(int heightDifference) {
        int randomIndex;
        switch (heightDifference) {
            case 0:
                randomIndex = Random.Range(0, _bridgeEnvUnit0Prefab.Length);
                return _bridgeEnvUnit0Prefab[randomIndex].GetRandomVisualBridge;
            case 1:
                randomIndex = Random.Range(0, _bridgeEnvUnit1Prefab.Length);
                return _bridgeEnvUnit1Prefab[randomIndex].GetRandomVisualBridge;
            case 2:
                randomIndex = Random.Range(0, _bridgeEnvUnit2Prefab.Length);
                return _bridgeEnvUnit2Prefab[randomIndex].GetRandomVisualBridge;
            case 3:
                randomIndex = Random.Range(0, _bridgeEnvUnit3Prefab.Length);
                return _bridgeEnvUnit3Prefab[randomIndex].GetRandomVisualBridge;
            case 4:
                randomIndex = Random.Range(0, _bridgeEnvUnit4Prefab.Length);
                return _bridgeEnvUnit4Prefab[randomIndex].GetRandomVisualBridge;
            case 5:
                randomIndex = Random.Range(0, _bridgeEnvUnit5Prefab.Length);
                return _bridgeEnvUnit5Prefab[randomIndex].GetRandomVisualBridge;
            default:
                Debug.LogError($"BridgeEnvUnitPrefab index {heightDifference} is out of range");
                throw new System.ArgumentOutOfRangeException();
        }
    }
    
}

[System.Serializable]
public class BridgeType {
    [SerializeField] private GameObject[] bridgeTypeSprites;
    public GameObject GetRandomVisualBridge => bridgeTypeSprites[Random.Range(0, bridgeTypeSprites.Length)];
}

[System.Serializable]
public struct PlayableUnit {
    public GameObject PlayerUnit;
    public GameObject GuideUnit;
}
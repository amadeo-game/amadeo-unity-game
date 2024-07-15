using UnityEngine;

[CreateAssetMenu(fileName = "BridgeType", menuName = "Bridge/BridgeType")]
public class BridgeTypeSO : ScriptableObject {
    // [SerializeField] private BridgeSpriteCollection bridgeSpritesCollections;
    [SerializeField] private PlayableUnit[] playableUnitPrefab;
    // prefab for height difference 0
    [SerializeField] private BridgeType[] bridgeEnvUnit0Prefab;

    // prefab for height difference 1
    [SerializeField] private BridgeType[] bridgeEnvUnit1Prefab;

    // prefab for height difference 2
    [SerializeField] private BridgeType[] bridgeEnvUnit2Prefab;

    // prefab for height difference 3
    [SerializeField] private BridgeType[] bridgeEnvUnit3Prefab;

    // prefab for height difference 4
    [SerializeField] private BridgeType[] bridgeEnvUnit4Prefab;

    // prefab for height difference 5
    [SerializeField] private BridgeType[] bridgeEnvUnit5Prefab;


    // public BridgeSpriteCollection BridgeSpritesCollections => bridgeSpritesCollections;
    public PlayableUnit GetPlayableUnitPrefab => playableUnitPrefab[Random.Range(0, playableUnitPrefab.Length)];


    /* by asking for an environment unit that will fit the gap of the 2 player units' height differences,
     will randomize all the pre-made options in the unity inspector and return one of them.
     */
    public GameObject GetEnvUnitType(int heightDifference) {
        int randomIndex;
        switch (heightDifference) {
            case 0:
                randomIndex = Random.Range(0, bridgeEnvUnit0Prefab.Length);
                return bridgeEnvUnit0Prefab[randomIndex].GetRandomVisualBridge;
            case 1:
                randomIndex = Random.Range(0, bridgeEnvUnit1Prefab.Length);
                return bridgeEnvUnit1Prefab[randomIndex].GetRandomVisualBridge;
            case 2:
                randomIndex = Random.Range(0, bridgeEnvUnit2Prefab.Length);
                return bridgeEnvUnit2Prefab[randomIndex].GetRandomVisualBridge;
            case 3:
                randomIndex = Random.Range(0, bridgeEnvUnit3Prefab.Length);
                return bridgeEnvUnit3Prefab[randomIndex].GetRandomVisualBridge;
            case 4:
                randomIndex = Random.Range(0, bridgeEnvUnit4Prefab.Length);
                return bridgeEnvUnit4Prefab[randomIndex].GetRandomVisualBridge;
            case 5:
                randomIndex = Random.Range(0, bridgeEnvUnit5Prefab.Length);
                return bridgeEnvUnit5Prefab[randomIndex].GetRandomVisualBridge;
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
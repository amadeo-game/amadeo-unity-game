using UnityEngine;

public class Bridge {
    private GameObject[] bridgeEnvUnits;
    public GameObject[] bridgePlayerUnits { get; private set; }
    private GameObject[] playerBridgeUnitsPlaceHolders;

    private readonly int[] playerUnitsHeights;
    private readonly GameObject envUnitPrefab;
    private readonly GameObject playerUnitPrefab;
    private readonly BridgeSpritesCollection bridgeSpritesCollection;
    private readonly GameObject playerUnitPlaceHolder;

    private readonly int numBridgeUnits;
    private readonly Vector3 prefabLocalScale;

    public Bridge(
        int[] playerUnitsHeights, GameObject playerUnitPrefab, GameObject envUnitPrefab
        , GameObject playerUnitPlaceHolder,
        BridgeSpritesCollection bridgeSpritesCollection) {
        this.playerUnitsHeights = playerUnitsHeights;
        this.playerUnitPrefab = playerUnitPrefab;
        this.playerUnitPlaceHolder = playerUnitPlaceHolder;
        this.envUnitPrefab = envUnitPrefab;
        this.bridgeSpritesCollection = bridgeSpritesCollection;
        prefabLocalScale = envUnitPrefab.transform.localScale;
        numBridgeUnits = playerUnitsHeights.Length;
    }


    public void BuildPlayerUnits() {
        bridgePlayerUnits = new GameObject[numBridgeUnits];
        playerBridgeUnitsPlaceHolders = new GameObject[numBridgeUnits];

        for (int i = 0; i < numBridgeUnits; i++) {
            var position = envUnitPrefab.transform.position;
            var rotation = envUnitPrefab.transform.rotation;
            bridgePlayerUnits[i] = GameObject.Instantiate(playerUnitPrefab,
                new Vector3(position.x + i * 4, playerUnitsHeights[i], 0),
                rotation);
            playerBridgeUnitsPlaceHolders[i] = GameObject.Instantiate(playerUnitPlaceHolder,
                new Vector3(position.x + i * 4, playerUnitsHeights[i], 0),
                rotation);
        }
    }

    public void ReplacePUnitSprite() {
        for (int i = 0; i < numBridgeUnits; i++) {
            SpriteReplacer.ReplaceSprite(bridgeSpritesCollection.PlayerUnitSprite, bridgePlayerUnits[i]);
            SpriteReplacer.ReplaceSprite(bridgeSpritesCollection.PlayerUnitSprite, playerBridgeUnitsPlaceHolders[i]);
        }
    }

    public void GenerateBridgeEnvironment(GameObject[] bridgeUnits) {
        for (int i = 0; i < bridgeUnits.Length - 1; i++) {
            EnvElevationUnit(bridgeUnits[i].transform, bridgeUnits[i + 1].transform);
        }
    }

    private void EnvElevationUnit(Transform leftUnit, Transform rightUnit) {
        var leftUnitY = leftUnit.position.y;
        var rightUnitY = rightUnit.position.y;
        var validator = 0;
        bool isHeightSignificant = Mathf.Abs(leftUnitY - rightUnitY) > 1;
        while (leftUnitY + 1 < rightUnitY) {
            if (validator > 5) {
                break;
            }

            GameObject bridgeEnvUnit = GameObject.Instantiate(envUnitPrefab);
            bridgeEnvUnit.transform.SetLocalPositionAndRotation(leftUnit.position + new Vector3(1.25f, 0.75f, 0f),
                Quaternion.Euler(0, 0, 90));
            SpriteReplacer.ReplaceSprite(bridgeSpritesCollection.EnvironmentSprites[0], bridgeEnvUnit);
            leftUnitY++;
            leftUnit.position += new Vector3(0, 1);

            validator++;
        }

        while (leftUnitY - 1 > rightUnitY) {
            if (validator < -5) {
                break;
            }

            GameObject bridgeEnvUnit = GameObject.Instantiate(envUnitPrefab);
            bridgeEnvUnit.transform.SetLocalPositionAndRotation(leftUnit.position + new Vector3(1.25f - 0.05f, -0.75f, 0f),
                Quaternion.Euler(0, 0, 90));
            SpriteReplacer.ReplaceSprite(bridgeSpritesCollection.EnvironmentSprites[0], bridgeEnvUnit);
            leftUnitY--;
            leftUnit.position += new Vector3(0, -2);

            validator--;
        }

        var newY = (validator < 0 ? 1 : (validator > 0 ? -1 : 0));
        leftUnit.position += new Vector3(0, newY);
        // rightUnit.localPosition += new Vector3(0,rightUnitY-rightUnit.position.y);
        EnvConnectingUnit(leftUnit, rightUnit);
    }


    private void EnvConnectingUnit(Transform leftUnit, Transform rightUnit) {
        GameObject bridgeEnvUnit = GameObject.Instantiate(envUnitPrefab);

        // Calculate the position of the new square
        var position1 = leftUnit.position;
        float square1LeftEdgePos = position1.x + (leftUnit.localScale.x / 2);
        var position2 = rightUnit.position;
        float square2RightEdgePos = position2.x - (rightUnit.localScale.x / 2);

        Vector3 square1EdgePosition =
            new Vector3(square1LeftEdgePos + 0.3f, position1.y, position1.z);
        Vector3 square2EdgePosition =
            new Vector3(square2RightEdgePos - 0.3f, position2.y, position2.z);
        Vector3 position = (square1EdgePosition + square2EdgePosition) / 2;

        // Calculate the rotation of the new square
        float angle =
            Mathf.Atan2(square2EdgePosition.y - square1EdgePosition.y,
                square2EdgePosition.x - square1EdgePosition.x) *
            Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, 0, angle);

        // Calculate the scale of the new square
        float distance = Vector3.Distance(square1EdgePosition, square2EdgePosition);
        Vector3 scale = new Vector3(distance / Mathf.Abs(Mathf.Cos(rotation.eulerAngles.z * Mathf.Deg2Rad)),
            prefabLocalScale.y, prefabLocalScale.z);

        // Set the position, rotation and scale of the new square
        bridgeEnvUnit.transform.position = position;
        bridgeEnvUnit.transform.rotation = rotation;
        bridgeEnvUnit.transform.localScale = scale;
        // bridgeEnvUnit.GetComponent<SpriteReplacer>().ReplaceSprite();
        SpriteReplacer.ReplaceSprite(bridgeSpritesCollection.EnvironmentSprites[0], bridgeEnvUnit);
    }
}
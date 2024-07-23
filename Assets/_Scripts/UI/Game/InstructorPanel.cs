using BridgePackage;
using UnityEngine;
using UnityEngine.UIElements;

public class InstructorPanel : MonoBehaviour
{
    public VisualTreeAsset visualTree;
    public StyleSheet styleSheet;
    private VisualElement root;
    private VisualElement floatingPanel;

    private BridgeDataManager _bridgeDataManager;
    private GameManager gameManager;

    private void OnEnable()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        if (root == null)
        {
            Debug.LogError("UIDocument rootVisualElement is null");
            return;
        }

        visualTree.CloneTree(root);
        root.styleSheets.Add(styleSheet);

        _bridgeDataManager = FindObjectOfType<BridgeDataManager>();
        gameManager = FindObjectOfType<GameManager>();

        if (_bridgeDataManager == null || gameManager == null)
        {
            Debug.LogError("SessionManager or GameManager not found in the scene");
            return;
        }

        floatingPanel = root.Q<VisualElement>("FloatingPanel");

        root.Q<Button>("InitializeGameButton").clicked += InitializeGameSession;
        root.Q<Button>("StartSessionButton").clicked += StartSession;

        SetupInputValidators();
    }

    private void SetupInputValidators()
    {
        // Setup validators for input fields to ensure correct input
        for (int i = 0; i < 5; i++)
        {
            root.Q<TextField>($"MvcValueInput{i}").RegisterValueChangedCallback(evt =>
            {
                if (!int.TryParse(evt.newValue, out int result) || result < -50 || result > 50)
                {
                    root.Q<TextField>($"MvcValueInput{i}").value = "0";
                }
            });

            root.Q<TextField>($"UnitsGraceInput{i}").RegisterValueChangedCallback(evt =>
            {
                if (!float.TryParse(evt.newValue, out float result) || result < 0 || result > 5)
                {
                    root.Q<TextField>($"UnitsGraceInput{i}").value = "0";
                }
            });
        }
    }

    private void InitializeGameSession()
    {
        Debug.Log("InitializeGameSession called.");
        UpdateSessionManagerValues();
        gameManager.InitializeNewGame();
        
    }

    private void StartSession()
    {
        UpdateSessionManagerValues();
    }

    private void UpdateSessionManagerValues()
    {
        int[] heights = new int[5];
        for (int i = 0; i < 5; i++)
        {
            heights[i] = root.Q<SliderInt>($"HeightSlider{i}").value;
        }
        BridgeDataManager.SetHeights(heights);

        float[] mvcValues = new float[5];
        for (int i = 0; i < 5; i++)
        {
            mvcValues[i] = float.Parse(root.Q<TextField>($"MvcValueInput{i}").value);
        }
        BridgeDataManager.SetMvcValues(mvcValues);

        bool[] playableUnits = new bool[5];
        for (int i = 0; i < 5; i++)
        {
            playableUnits[i] = root.Q<Toggle>($"PlayableUnitToggle{i}").value;
        }
        BridgeDataManager.SetPlayableUnits(playableUnits);

        float[] unitsGrace = new float[5];
        for (int i = 0; i < 5; i++)
        {
            unitsGrace[i] = float.Parse(root.Q<TextField>($"UnitsGraceInput{i}").value);
        }
        BridgeDataManager.SetUnitsGrace(unitsGrace);

        BridgeDataManager.SetIsLeftHand(root.Q<Toggle>("IsLeftHandToggle").value);
        BridgeDataManager.SetIsFlexion(root.Q<Toggle>("IsFlexionToggle").value);
        BridgeDataManager.SetAutoPlay(root.Q<Toggle>("AutoPlayToggle").value);
    }

    public void TogglePanelVisibility()
    {
        floatingPanel.style.display = floatingPanel.style.display == DisplayStyle.None ? DisplayStyle.Flex : DisplayStyle.None;
    }
}

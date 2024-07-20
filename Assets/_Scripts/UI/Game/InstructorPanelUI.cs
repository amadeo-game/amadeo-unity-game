using UnityEngine;
using UnityEngine.UI;

public class InstructorPanelUI : MonoBehaviour
{
    public GameObject panelPrefab;
    public GameObject textPrefab;
    public GameObject inputFieldPrefab;
    public GameObject dropdownPrefab;
    public GameObject togglePrefab;
    public GameObject buttonPrefab;

    void Start()
    {
        GameObject canvas = new GameObject("Canvas");
        Canvas c = canvas.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.AddComponent<CanvasScaler>();
        canvas.AddComponent<GraphicRaycaster>();

        GameObject panel = Instantiate(panelPrefab, canvas.transform);

        CreateLabel(panel, "Time Duration", new Vector2(-200, 300));
        GameObject timeDurationInput = CreateInputField(panel, "TimeDurationInput", new Vector2(200, 300));

        CreateLabel(panel, "Heights", new Vector2(-200, 250));
        GameObject[] heightInputs = new GameObject[5];
        for (int i = 0; i < 5; i++)
        {
            heightInputs[i] = CreateInputField(panel, $"HeightInput{i}", new Vector2(200, 250 - (i * 50)));
        }

        CreateLabel(panel, "Bridge Type", new Vector2(-200, 0));
        GameObject bridgeTypeDropdown = CreateDropdown(panel, "BridgeTypeDropdown", new Vector2(200, 0));

        CreateLabel(panel, "Left Hand", new Vector2(-200, -50));
        GameObject isLeftHandToggle = CreateToggle(panel, "IsLeftHandToggle", new Vector2(200, -50));

        CreateLabel(panel, "Flexion", new Vector2(-200, -100));
        GameObject isFlexionToggle = CreateToggle(panel, "IsFlexionToggle", new Vector2(200, -100));

        CreateLabel(panel, "MVC Values", new Vector2(-200, -150));
        GameObject[] mvcValueInputs = new GameObject[5];
        for (int i = 0; i < 5; i++)
        {
            mvcValueInputs[i] = CreateInputField(panel, $"MvcValueInput{i}", new Vector2(200, -150 - (i * 50)));
        }

        CreateLabel(panel, "Playable Units", new Vector2(-200, -400));
        GameObject[] playableUnitToggles = new GameObject[5];
        for (int i = 0; i < 5; i++)
        {
            playableUnitToggles[i] = CreateToggle(panel, $"PlayableUnitToggle{i}", new Vector2(200, -400 - (i * 50)));
        }

        CreateLabel(panel, "Units Grace", new Vector2(-200, -650));
        GameObject[] unitsGraceInputs = new GameObject[5];
        for (int i = 0; i < 5; i++)
        {
            unitsGraceInputs[i] = CreateInputField(panel, $"UnitsGraceInput{i}", new Vector2(200, -650 - (i * 50)));
        }

        CreateLabel(panel, "Auto Play", new Vector2(-200, -900));
        GameObject autoPlayToggle = CreateToggle(panel, "AutoPlayToggle", new Vector2(200, -900));

        GameObject initializeGameButton = CreateButton(panel, "InitializeGameButton", "Initialize Game Session", new Vector2(0, -1000));
        GameObject startSessionButton = CreateButton(panel, "StartSessionButton", "Start Session", new Vector2(0, -1100));
    }

    private GameObject CreateLabel(GameObject parent, string text, Vector2 position)
    {
        GameObject label = Instantiate(textPrefab, parent.transform);
        label.GetComponent<Text>().text = text;
        label.GetComponent<RectTransform>().anchoredPosition = position;
        return label;
    }

    private GameObject CreateInputField(GameObject parent, string name, Vector2 position)
    {
        GameObject inputField = Instantiate(inputFieldPrefab, parent.transform);
        inputField.name = name;
        inputField.GetComponent<RectTransform>().anchoredPosition = position;
        return inputField;
    }

    private GameObject CreateDropdown(GameObject parent, string name, Vector2 position)
    {
        GameObject dropdown = Instantiate(dropdownPrefab, parent.transform);
        dropdown.name = name;
        dropdown.GetComponent<RectTransform>().anchoredPosition = position;
        return dropdown;
    }

    private GameObject CreateToggle(GameObject parent, string name, Vector2 position)
    {
        GameObject toggle = Instantiate(togglePrefab, parent.transform);
        toggle.name = name;
        toggle.GetComponent<RectTransform>().anchoredPosition = position;
        return toggle;
    }

    private GameObject CreateButton(GameObject parent, string name, string buttonText, Vector2 position)
    {
        GameObject button = Instantiate(buttonPrefab, parent.transform);
        button.name = name;
        button.GetComponentInChildren<Text>().text = buttonText;
        button.GetComponent<RectTransform>().anchoredPosition = position;
        return button;
    }
}

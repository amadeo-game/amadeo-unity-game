using UnityEngine;
using UnityEngine.UI;

public class TogglePanelButton : MonoBehaviour
{
    public Button toggleButton;
    private InstructorPanel instructorPanel;

    private void Start()
    {
        instructorPanel = FindObjectOfType<InstructorPanel>();
        if (instructorPanel == null)
        {
            Debug.LogError("InstructorPanel not found in the scene.");
            return;
        }

        toggleButton.onClick.AddListener(() => instructorPanel.TogglePanelVisibility());
    }
}
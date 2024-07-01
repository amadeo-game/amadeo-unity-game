using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGame : MonoBehaviour {
    // Method to change the scene, accepting the name of the scene as a parameter
    public void ChangeScene() {
        // Find the main menu canvas using its tag or name and destroy it
        GameObject mainMenuCanvas = GameObject.Find("Canvas");
        if (mainMenuCanvas != null) {
            Destroy(mainMenuCanvas);
        }

        // SceneManager.LoadScene("LevelVarient");
        SceneManager.LoadScene(1);
    }
}
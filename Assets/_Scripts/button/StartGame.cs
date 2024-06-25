using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGame : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    // Method to change the scene, accepting the name of the scene as a parameter
    public void ChangeScene()
    {
        
        // Find the main menu canvas using its tag or name and destroy it
        GameObject mainMenuCanvas = GameObject.Find("Canvas");
        if (mainMenuCanvas != null)
        {
            Destroy(mainMenuCanvas);
        }
        SceneManager.LoadScene("LevelVarient");
    }
}

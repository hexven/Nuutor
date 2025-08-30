using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneChanger : MonoBehaviour
{
    // Reference to the button in the Unity Inspector
    public Button changeSceneButton;

    void Start()
    {
        // Ensure the button is assigned and add a listener for the click event
        if (changeSceneButton != null)
        {
            changeSceneButton.onClick.AddListener(ChangeToOpeningScene);
        }
        else
        {
            Debug.LogError("Button not assigned in the Inspector!");
        }
    }

    // Function to change to the OpeningScene
    void ChangeToOpeningScene()
    {
        SceneManager.LoadScene("CutScene"); // Use the scene name or index (e.g., 0)
        // Alternatively: SceneManager.LoadScene(0); if OpeningScene is at index 0
    }
}
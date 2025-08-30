using UnityEngine;
using UnityEngine.SceneManagement;

public class HideCursorOnSceneLoad : MonoBehaviour
{
    [SerializeField] private string sceneName = "YourSceneName"; // Set this in the Inspector

    void Start()
    {
        // Check if the current scene matches the specified scene
        if (SceneManager.GetActiveScene().name == sceneName)
        {
            // Hide the cursor and lock it
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}
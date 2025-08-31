using UnityEngine;
using UnityEngine.SceneManagement;

public class HideCursorOnSceneLoad : MonoBehaviour
{
    void Awake()
    {
        // Ensure only one instance exists to avoid duplicates across scenes
        HideCursorOnSceneLoad[] managers = FindObjectsOfType<HideCursorOnSceneLoad>();
        if (managers.Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject); // Persist across scenes
    }

    void zeros()
    {
        // Ensure cursor is visible and unlocked in all scenes
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // Listen to scene changes to ensure cursor remains visible
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Ensure cursor is visible and unlocked when any scene loads
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}
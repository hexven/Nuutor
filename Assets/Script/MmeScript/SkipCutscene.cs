using UnityEngine;
using UnityEngine.Playables;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class SkipCutscene : MonoBehaviour
{
    public PlayableDirector timelineDirector;
    public List<double> sceneStartTimes = new List<double> { 1.0, 12.5, 24.0, 35.5, 47, 58.5 };
    private int currentSceneIndex = 0;
    public string nextSceneName;


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            currentSceneIndex++;

            if (currentSceneIndex < sceneStartTimes.Count)
            {
                double targetTime = sceneStartTimes[currentSceneIndex];
                timelineDirector.time = targetTime;
                timelineDirector.Evaluate();
            }
            else
            {
                timelineDirector.time = timelineDirector.duration;
                timelineDirector.Evaluate();
                timelineDirector.Stop();

                SceneManager.LoadScene(nextSceneName);
            }
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}

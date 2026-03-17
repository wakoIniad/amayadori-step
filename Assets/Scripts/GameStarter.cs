using UnityEngine;
using UnityEngine.SceneManagement;

class GameStarter: MonoBehaviour
{
    public string targetSceneName;
    #if UNITY_EDITOR

    public void OnButtonPushed()
    {
        SceneManager.LoadScene(targetSceneName, LoadSceneMode.Single);
    }
    #else
    
    public void OnButtonPushed()
    {
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(targetSceneName));
        SceneManager.UnloadSceneAsync(title);
    }
    Scene title;
    void Start()
    {
        title = SceneManager.GetActiveScene();
        SceneManager.LoadSceneAsync(targetSceneName, LoadSceneMode.Additive);
        SceneManager.SetActiveScene(title);
    }
    #endif
}
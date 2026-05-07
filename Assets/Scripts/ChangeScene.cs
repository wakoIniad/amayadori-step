using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public string targetSceneName;

    // Update is called once per frame
    public void Handler()
    {
        SceneManager.LoadScene(targetSceneName);
    }
}

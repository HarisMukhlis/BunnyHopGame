using UnityEngine;
using UnityEngine.SceneManagement;

public class StaticManager : MonoBehaviour
{
    public static StaticManager Instance { get; private set; }

    [Header("Scenes")]
    [SerializeField] private int gameplayScene = 0;
    [SerializeField] private int mainMenuScene = 1;
    [SerializeField] private int throwScene = 2;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else if (Instance != this)
        {
            Destroy(this.gameObject);
        }
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void LoadScene(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }

    public void RestartGameScene()
    {
        SceneManager.sceneLoaded += RestartGameSceneCallback;
        LoadScene(throwScene);
    }

    private void RestartGameSceneCallback(Scene scene, LoadSceneMode mode) //called when throwScene is loaded
    {
        SceneManager.sceneLoaded -= RestartGameSceneCallback;

        LoadScene(gameplayScene);
    }

    public void QuitApplication()
    {
        Application.Quit();
    }
}
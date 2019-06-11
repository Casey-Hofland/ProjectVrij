using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerController : MonoBehaviour
{
    public static SceneManagerController instance;

    [SerializeField] private Animator animator;

    private int sceneIndexOnFadeOut;

    private void Awake()
    {
        instance = this;
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void LoadScene(int sceneIndex)
    {
        if (SceneManager.GetActiveScene().buildIndex == sceneIndex)
            throw new System.Exception("Trying to load the current scene.");

        sceneIndexOnFadeOut = sceneIndex;
        animator.SetTrigger("fadeOut");
    }

    public void LoadScene(string sceneName)
    {
        int sceneIndex = SceneManager.GetSceneByName(sceneName).buildIndex;
        LoadScene(sceneIndex);
    }

    public void LoadNextScene()
    {
        int sceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        if (sceneIndex >= SceneManager.sceneCountInBuildSettings)
            throw new System.IndexOutOfRangeException("Trying to load the next scene when there is none.");

        LoadScene(sceneIndex);
    }

    // Event for the Fade Out Animation
    public void LoadSceneOnFadeOut()
    {
        SceneManager.LoadScene(sceneIndexOnFadeOut);
    }
}

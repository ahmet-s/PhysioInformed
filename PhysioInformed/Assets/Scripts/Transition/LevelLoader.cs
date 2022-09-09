using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;


public class LevelLoader : MonoBehaviour
{
    private static LevelLoader instance;

    [SerializeField] Animator sceneFade;
    float transitionTime = 1f;

    private void Awake()
    {
        instance = this;
    }

    public static LevelLoader GetInstance()
    {
        return instance;
    }

    public void LoadNextLevel(string levelName)
    {
        StartCoroutine(LoadLevel(levelName));
    }

    IEnumerator LoadLevel(string levelName)
    {
        sceneFade.SetTrigger("Start");

        yield return new WaitForSeconds(transitionTime);

        SceneManager.LoadScene(levelName);
    }
}

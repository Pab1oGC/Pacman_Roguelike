using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class MenuController : MonoBehaviour
{
    [Header("Fade")]
    [SerializeField] private Image fade;
    [SerializeField] private float fadeSpeed = 1.6f;

    [Header("Scenes")]
    [SerializeField] private string arSceneName = "ARGame";
    [SerializeField] private string multiplayerSceneName = "Multiplayer";

    [Header("SFX (opcional)")]
    [SerializeField] private AudioSource uiClick;

    void Start()
    {
        if (fade != null)
        {
            Color c = fade.color;
            c.a = 1f;
            fade.color = c;
            StartCoroutine(FadeTo(0f));
        }
    }

    public void OnPlay()
    {
        if (uiClick != null) uiClick.Play();
        StartCoroutine(LoadSceneWithFade(arSceneName));
    }

    public void OnMultiplayer()
    {
        if (uiClick != null) uiClick.Play();
        StartCoroutine(LoadSceneWithFade(multiplayerSceneName));
    }

    public void OnExit()
    {
        if (uiClick != null) uiClick.Play();
        StartCoroutine(QuitWithFade());
    }

    IEnumerator LoadSceneWithFade(string scene)
    {
        yield return StartCoroutine(FadeTo(1f));
        SceneManager.LoadScene(scene, LoadSceneMode.Single);
    }

    IEnumerator QuitWithFade()
    {
        yield return StartCoroutine(FadeTo(1f));
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    IEnumerator FadeTo(float targetAlpha)
    {
        if (fade == null) yield break;
        Color c = fade.color;
        while (!Mathf.Approximately(c.a, targetAlpha))
        {
            c.a = Mathf.MoveTowards(c.a, targetAlpha, Time.deltaTime * fadeSpeed);
            fade.color = c;
            yield return null;
        }
    }
}

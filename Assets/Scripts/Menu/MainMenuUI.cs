using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] string gameSceneName = "SampleScene"; // pon aqu� el nombre exacto de tu escena de juego

    public void Play()
    {
        SceneManager.LoadScene(gameSceneName);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PanelWhellControl : MonoBehaviour
{
    [SerializeField] private GameObject panel;     // puedes arrastrarlo si quieres
    [SerializeField] private string panelTag = "Panel";
    [SerializeField] private bool openOnPlayerCollision = true;

    private void Awake()
    {
        // Si no está asignado, intenta encontrarlo INCLUYENDO inactivos
        if (panel == null)
            panel = FindInactiveByTagAcrossLoadedScenes(panelTag);
    }

    private void OnEnable()
    {
        // Si aún no existe (p.ej. lo carga otra escena después), espera hasta que aparezca
        if (panel == null)
            StartCoroutine(WaitAndBindPanel());
    }

    private void Start()
    {
        if (panel != null) panel.SetActive(false);
        else Debug.LogWarning("[PanelWhellControl] Panel no encontrado. ¿Tiene el tag correcto y está en una escena cargada?");
    }

    public void ClosePopUp()
    {
        if (panel != null) panel.SetActive(false);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!openOnPlayerCollision) return;
        if (collision.gameObject.CompareTag("Player") && panel != null)
            panel.SetActive(true);
    }

    // --- Helpers ---

    // Busca en TODAS las escenas cargadas, incluyendo hijos inactivos
    private static GameObject FindInactiveByTagAcrossLoadedScenes(string tag)
    {
        for (int si = 0; si < SceneManager.sceneCount; si++)
        {
            var scene = SceneManager.GetSceneAt(si);
            if (!scene.isLoaded) continue;

            var roots = scene.GetRootGameObjects();
            for (int r = 0; r < roots.Length; r++)
            {
                var trs = roots[r].GetComponentsInChildren<Transform>(true); // true = incluye inactivos
                for (int i = 0; i < trs.Length; i++)
                    if (trs[i].CompareTag(tag))
                        return trs[i].gameObject;
            }
        }
        return null;
    }

    private IEnumerator WaitAndBindPanel()
    {
        while (panel == null)
        {
            panel = FindInactiveByTagAcrossLoadedScenes(panelTag);
            if (panel != null)
            {
                // opcional: inicializa estado
                panel.SetActive(false);
                yield break;
            }
            yield return null; // espera al siguiente frame
        }
    }
}

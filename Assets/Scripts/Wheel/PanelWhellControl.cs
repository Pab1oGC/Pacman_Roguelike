using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PanelWhellControl : MonoBehaviour
{
    [SerializeField] private GameObject panel;     // puedes arrastrarlo si quieres
    [SerializeField] private string panelTag = "Panel";
    [SerializeField] private bool openOnPlayerCollision = true;
    [SerializeField] private GameObject joystickMovement;
    [SerializeField] private GameObject joystickAttack;
    [Header("Refs")]
    public Camera arCamera;                // Asigna tu ARCamera (Vuforia/AR Foundation)
    [Tooltip("Opcional: solo detectar estos layers")]
    public LayerMask hitMask = ~0;

    private void Awake()
    {
        // Si no está asignado, intenta encontrarlo INCLUYENDO inactivos
        if (panel == null)
            panel = FindInactiveByTagAcrossLoadedScenes(panelTag);

        joystickMovement = GameObject.FindGameObjectWithTag("JoystickMove");
        joystickAttack = GameObject.FindGameObjectWithTag("JoystickAttack");
    }

    private void Update()
    {
        // Bloquear si tocaste UI
        if (IsPointerOverUI()) return;

        // Mouse en Editor
        if (Input.GetMouseButtonDown(0))
            TryRaycast(Input.mousePosition);

        // Touch en móvil
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            TryRaycast(Input.GetTouch(0).position);
    }

    bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;
#if UNITY_EDITOR
        return EventSystem.current.IsPointerOverGameObject();
#else
        // En móvil hay que pasar el fingerId
        return EventSystem.current.IsPointerOverGameObject(Input.touchCount > 0 ? Input.GetTouch(0).fingerId : -1);
#endif
    }

    void TryRaycast(Vector2 screenPos)
    {
        if (arCamera == null) arCamera = Camera.main;
        Ray ray = arCamera.ScreenPointToRay(screenPos);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, hitMask, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.CompareTag("Buffon"))
            {
                if (!openOnPlayerCollision) return;
                if (panel != null)
                {
                    panel.SetActive(true);
                    joystickMovement.SetActive(false);
                    joystickAttack.SetActive(false);
                }
            }
        }
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
        if (panel != null)
        {
            panel.SetActive(false);
            joystickMovement.SetActive(true);
            joystickAttack.SetActive(true);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        /*if (!openOnPlayerCollision) return;
        if (collision.gameObject.CompareTag("Player") && panel != null)
            panel.SetActive(true);*/
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

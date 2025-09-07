using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DashButton : MonoBehaviour
{
    [Tooltip("Opcional: Tag del Player para encontrar primero por tag.")]
    [SerializeField] private string playerTag = "Player";

    private Button _button;
    private Image _img;
    private IDashController _dash; // tu DashComponent implementa esto

    private void Awake()
    {
        _button = GetComponent<Button>();
        _img = GetComponent<Image>();
        _img.raycastTarget = true; // ¡que reciba taps!

        _button.onClick.AddListener(() => _dash?.TryDash());
    }

    private void Start()
    {
        // si el player aparece después (prefab), lo buscamos hasta hallarlo
        StartCoroutine(CoFindDash());
    }

    private IEnumerator CoFindDash()
    {
        while (_dash == null)
        {
            // 1) por tag
            if (!string.IsNullOrEmpty(playerTag))
            {
                var go = GameObject.FindWithTag(playerTag);
                if (go)
                {
                    var dc = go.GetComponent<Dash>();
                    if (dc) _dash = dc; // como IDashController
                }
            }
            // 2) por toda la escena
            if (_dash == null)
            {
#if UNITY_2022_2_OR_NEWER
                var dc = FindFirstObjectByType<DashComponent>(FindObjectsInactive.Include);
#else
                var dc = FindObjectOfType<Dash>(true);
#endif
                if (dc) _dash = dc;
            }

            if (_dash == null) yield return new WaitForSeconds(0.25f);
        }
    }

    private void Update()
    {
        // deshabilita el botón si no hay dash o está en cooldown
        _button.interactable = _dash != null && !_dash.IsOnCooldown && !_dash.IsDashing;
    }
}

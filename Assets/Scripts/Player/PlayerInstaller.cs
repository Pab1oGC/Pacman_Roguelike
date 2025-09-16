using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-100)] // se ejecuta temprano
public sealed class PlayerInstaller : MonoBehaviour
{
    [Header("Contexto")]
    [SerializeField] private PlayerContext context;

    [Header("Componentes a cablear")]
    [SerializeField] private Movement movement;
    [SerializeField] private Dash dash;
    [SerializeField] private Health health;

    [SerializeField] public AttackController attackController;

    void OnValidate()
    {
        if (!context) context = GetComponent<PlayerContext>();
        if (!movement) movement = GetComponent<Movement>();
        if (!dash) dash = GetComponent<Dash>();
        if (!health) health = GetComponent<Health>();
    }

    void Awake()
    {

        attackController = GetComponentInChildren<AttackController>();
        if (!context) { Debug.LogError("[PlayerInstaller] Falta PlayerContext", this); return; }

        var dpad = GetComponentInChildren<DPadInputSource>(true);
        movement.SetInputSource(dpad);

        if (movement)
        {
            if (context.SpeedProvider != null) movement.SetSpeedProvider(context.SpeedProvider);
            if (dash) movement.SetDashController(dash);
        }

        if (dash)
        {
            dash.Inject(context.Rb, context.Invulnerability, movement);
        }

        if (health)
        {
            health.OnDied += GameOver;
        }


    }

    private void GameOver()
    {
        SceneManager.LoadScene("SampleScene");
    }

    


}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToastServiceMB : MonoBehaviour, IToastService
{
    [Header("Setup")]
    [SerializeField] private RectTransform toastRoot;   // contenedor en Canvas
    [SerializeField] private ToastView toastPrefab;     // prefab ToastItem
    [SerializeField] private int maxVisible = 2;
    [SerializeField] private float defaultDuration = 2f;
    [SerializeField] private float fadeTime = 0.18f;

    private IToastQueue _queue;
    private IToastFactory _factory;
    private IToastAnimator _anim;

    private readonly Queue<(string, float, ToastType)> _pending = new Queue<(string, float, ToastType)>();
    private bool _runner;

    void Awake()
    {
        // Inyección de dependencias
        _queue = new SimpleToastQueue(maxVisible);
        _factory = new UguiToastFactory(toastRoot, toastPrefab, new DefaultToastPalette());
        _anim = new FadeToastAnimator(fadeTime);
    }

    public void Enqueue(string message, float duration = -1f, ToastType type = ToastType.Info)
    {
        if (string.IsNullOrWhiteSpace(message)) return;
        float dur = duration > 0f ? duration : defaultDuration;
        _pending.Enqueue((message, dur, type));
        if (!_runner) StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        _runner = true;
        while (_pending.Count > 0)
        {
            while (!_queue.HasCapacity) yield return null;

            var (msg, dur, type) = _pending.Dequeue();
            var view = _factory.Create(msg, type);

            _queue.OnToastStarted();
            yield return StartCoroutine(_anim.Show(view, dur));
            Destroy(view.gameObject);
            _queue.OnToastFinished();
        }
        _runner = false;
    }
}

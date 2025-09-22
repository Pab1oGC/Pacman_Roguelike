using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeToastAnimator : IToastAnimator
{
    private readonly float _fade;
    public FadeToastAnimator(float fadeSeconds = 0.18f) { _fade = Mathf.Max(0.01f, fadeSeconds); }

    public IEnumerator Show(ToastView view, float duration)
    {
        yield return Fade(view.canvasGroup, 0f, 1f, _fade);
        yield return new WaitForSeconds(duration);
        yield return Fade(view.canvasGroup, 1f, 0f, _fade);
    }

    private IEnumerator Fade(CanvasGroup cg, float a, float b, float t)
    {
        float el = 0f;
        while (el < t)
        {
            el += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(a, b, el / t);
            yield return null;
        }
        cg.alpha = b;
    }
}

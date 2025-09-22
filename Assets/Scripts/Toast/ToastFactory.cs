using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultToastPalette : IToastPalette
{
    public Color info = new Color(0.16f, 0.16f, 0.16f, 0.95f);
    public Color success = new Color(0.10f, 0.50f, 0.22f, 0.95f);
    public Color warning = new Color(0.60f, 0.45f, 0.10f, 0.95f);
    public Color error = new Color(0.60f, 0.10f, 0.10f, 0.95f);

    public Color ColorOf(ToastType t) => t switch
    {
        ToastType.Success => success,
        ToastType.Warning => warning,
        ToastType.Error => error,
        _ => info
    };
}

public class UguiToastFactory : IToastFactory
{
    private readonly RectTransform _root;
    private readonly ToastView _prefab;
    private readonly IToastPalette _palette;

    public UguiToastFactory(RectTransform root, ToastView prefab, IToastPalette palette)
    {
        _root = root; _prefab = prefab; _palette = palette;
    }

    public ToastView Create(string msg, ToastType type)
    {
        var v = Object.Instantiate(_prefab, _root);
        if (v.canvasGroup) v.canvasGroup.alpha = 0f;
        v.SetText(msg);
        v.SetColor(_palette.ColorOf(type));
        return v;
    }
}

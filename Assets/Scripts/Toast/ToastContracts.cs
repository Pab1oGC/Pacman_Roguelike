using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ToastType { Info, Success, Warning, Error }

public interface IToastService
{
    void Enqueue(string message, float duration = 2f, ToastType type = ToastType.Info);
}

public interface IToastQueue
{
    bool HasCapacity { get; }
    void OnToastStarted();
    void OnToastFinished();
}

public interface IToastFactory
{
    ToastView Create(string msg, ToastType type);
}

public interface IToastAnimator
{
    System.Collections.IEnumerator Show(ToastView view, float duration);
}

public interface IToastPalette
{
    UnityEngine.Color ColorOf(ToastType t);
}

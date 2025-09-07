using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedService : MonoBehaviour, ISpeedProvider
{
    [Min(0f)][SerializeField] private float baseSpeed = 0.5f;

    private readonly Dictionary<string, float> _flat = new Dictionary<string, float> ();
    private readonly Dictionary<string, float> _mul = new Dictionary<string, float>();

    public float CurrentSpeed
    {
        get
        {
            float flatSum = 0f;
            foreach (var kv in _flat) flatSum += kv.Value;
            float product = 1f;
            foreach (var kv in _mul) product *= kv.Value;
            return Mathf.Max(0f, (baseSpeed + flatSum) * product);
        }
    }

    // API para buffs
    public void SetBase(float v) => baseSpeed = Mathf.Max(0f, v);
    public void SetFlat(string key, float value) => _flat[key] = value;
    public void RemoveFlat(string key) => _flat.Remove(key);
    public void SetMul(string key, float factor) => _mul[key] = Mathf.Max(0f, factor);
    public void RemoveMul(string key) => _mul.Remove(key);
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CoinPickup : MonoBehaviour
{
    [SerializeField] private int value = 1;
    [SerializeField] private float lifetime = 30f;
    [SerializeField] private AudioClip pickupSfx;
    [SerializeField] private GameObject pickupVfx;
    [SerializeField] private float vfxLifetime = 1f;

    private void Awake()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;

        if (lifetime > 0f) Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (other.TryGetComponent<PlayerWallet>(out var wallet))
        {
            wallet.Add(value);

            if (pickupVfx)
            {
                var fx = Instantiate(pickupVfx, transform.position, Quaternion.identity);
                if (vfxLifetime > 0f) Destroy(fx, vfxLifetime);
            }

            if (pickupSfx)
                AudioSource.PlayClipAtPoint(pickupSfx, transform.position);

            Destroy(gameObject);
        }
    }
}

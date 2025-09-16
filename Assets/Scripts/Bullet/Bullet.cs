using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float damage = 1f;
    public bool isBulletPlayer;
    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject,1f);
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.forward * 4f * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") && isBulletPlayer)
        {
            Enemy enemy = other.GetComponent<Enemy>();
            enemy.TakeDamage(damage);
            Destroy(gameObject); 
        }
        else if (other.CompareTag("Player") && !isBulletPlayer)
        {
            Health player = other.GetComponent<Health>();
            player.ApplyDamage(damage);
            Destroy(gameObject);
        }
    }
}

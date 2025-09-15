using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float health = 3f;

    protected virtual void Start()
    {

    }

    public void TakeDamage(float damage)
    {
        health -= damage;

        if (health <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        Destroy(gameObject);
    }
}

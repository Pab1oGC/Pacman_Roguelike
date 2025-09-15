using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KamikazeEnemy : Enemy
{
    public float speed = 3f;
    private Transform player;

    protected override void Start()
    {
        base.Start();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (player == null) return;

        transform.position = Vector3.MoveTowards(transform.position, player.position, speed * Time.deltaTime);

        Vector3 dir = (player.position - transform.position).normalized;
        transform.forward = dir;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }


}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hit_Effect : MonoBehaviour
{
    private Monster _monster;

    private float _damagePushForceMultiplier_sett = 10;    

    void OnCollisionEnter2D(Collision2D col)
    {
        var monster = col.gameObject.GetComponent<Monster>();
        if (monster == null || monster.UsingMove == -1) return;
        var move = monster.Moves[monster.UsingMove];
        var rb = _monster.GetComponent<Rigidbody2D>();

        if (_monster.UsingMove != -1)
        {
            if (_monster.Moves[_monster.UsingMove].Damage < move.Damage)
            {
                var direction = _monster.Moves[_monster.UsingMove].MoveDirection.normalized;
                var enemyDirection = monster.Moves[monster.UsingMove].MoveDirection.normalized;
                var forceChanger = (float) _monster.Moves[_monster.UsingMove].Damage / (float) move.Damage;
                rb.AddForce(enemyDirection + (direction * forceChanger) * _damagePushForceMultiplier_sett);
                monster.GetComponent<Rigidbody2D>().velocity = Vector3.zero;
            } else 
            {
                rb.velocity = Vector3.zero;
            }
        } else 
        {
            var direction = monster.Moves[monster.UsingMove].MoveDirection.normalized * _damagePushForceMultiplier_sett;
            rb.velocity = direction;
            monster.GetComponent<Rigidbody2D>().velocity = Vector3.zero;
        }

        _monster.ReceiveDamage(move, monster);
    }
}

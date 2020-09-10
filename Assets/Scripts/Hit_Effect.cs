using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hit_Effect : MonoBehaviour
{
    public Monster _monster;

    private float _damagePushForceMultiplier_sett = 10;    

    void OnCollisionEnter2D(Collision2D col)
    {
        var monster = col.gameObject.GetComponent<Monster>();
        if (monster == null) return;
        var move = _monster.Moves[_monster.UsingMove];
        var rb = monster.GetComponent<Rigidbody2D>();

        if (monster.UsingMove != -1)
        {
            if (monster.Moves[monster.UsingMove].Damage < move.Damage)
            {
                var direction = monster.Moves[monster.UsingMove].MoveDirection.normalized;
                var enemyDirection = _monster.Moves[_monster.UsingMove].MoveDirection.normalized;
                var forceChanger = (float) monster.Moves[monster.UsingMove].Damage / (float) move.Damage;
                rb.AddForce(enemyDirection + (direction * forceChanger) * _damagePushForceMultiplier_sett);
                _monster.GetComponent<Rigidbody2D>().velocity = Vector3.zero;
            } else 
            {
                rb.velocity = Vector3.zero;
            }
        } else 
        {
            var direction = _monster.Moves[_monster.UsingMove].MoveDirection.normalized * _damagePushForceMultiplier_sett;
            rb.velocity = direction;
            _monster.GetComponent<Rigidbody2D>().velocity = Vector3.zero;
        }

        monster.ReceiveDamage(move, monster);
        gameObject.SetActive(false);
    }
}

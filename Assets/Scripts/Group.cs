using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Group : MonoBehaviour
{
    public List<Monster> Monsters;

    void OnTriggerEnter2D(Collider2D col)
    {
        var monster = col.gameObject.GetComponent<Monster>();
        var player = col.gameObject.GetComponent<Player>();
        if ((player == null && monster == null) || (monster != null && !monster.Captured)) return;
        foreach (var m in Monsters)
        {
            m.Target = col.gameObject;
        }
    }
}

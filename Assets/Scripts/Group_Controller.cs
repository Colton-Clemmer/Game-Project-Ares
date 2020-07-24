using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Group_Controller : MonoBehaviour
{
    public List<Monster> Monsters;

    public void StopChase()
    {
        foreach (var monster in Monsters)
        {
            monster.Target = null;
        }
    }

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

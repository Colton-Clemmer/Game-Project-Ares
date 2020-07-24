using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chase_Boundry : MonoBehaviour
{
    public Group_Controller Monsters;

    void OnTriggerExit2D(Collider2D col)
    {
        var monster = col.gameObject.GetComponent<Monster>();
        var player = col.gameObject.GetComponent<Player>();
        if ((player == null && monster == null) || (monster != null && !monster.Captured)) return;
        Monsters.StopChase();
    }
}

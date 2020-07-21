using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PolyNav;

public class Item : MonoBehaviour
{
    public Utils.Item Type;
    public GameObject MonsterInside;

    private PolyNavAgent _nav
    { get { return GetComponent<PolyNavAgent>(); } }

    void Start()
    {
        _nav.map = Utils.NavGrid;
    }


    void Update()
    {
        if (MonsterInside)
        { 
            _nav.SetDestination(Utils.Util.Player.transform.position);
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (Type == Utils.Item.Capture_Ball)
        {
            var monster = col.gameObject.GetComponent<Monster>();
            if (monster != null)
            {
                MonsterInside = col.gameObject;
                MonsterInside.transform.SetParent(transform);
                MonsterInside.transform.localPosition = Vector3.zero;
                monster.Captured = true;
                MonsterInside.SetActive(false);
                GetComponent<Rigidbody2D>().velocity = Vector3.zero;
            }
            var player = col.gameObject.GetComponent<Player>();
            if (player != null)
            {
                player.MonstersCaptured.Add(MonsterInside.GetComponent<Monster>());
                MonsterInside.transform.SetParent(player.transform);
                MonsterInside.transform.localPosition = Vector3.zero;
                Destroy(gameObject);
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using PolyNav;

public class Group_Controller : MonoBehaviour
{
    public GameObject MonsterPrefab;
    [SerializeField] private PolyNav2D _nav;

    public GameObject Target;

    public List<Monster> Monsters;

    void Update()
    {
        if (Monsters.Any(m => m == null))
        {
            Monsters.RemoveAll(m => m == null);
        }
        if (Monsters.Count < 1)
        {
            var monster = Instantiate(MonsterPrefab);
            monster.transform.SetParent(transform);
            monster.transform.localPosition = Vector3.zero;
            var nav = monster.GetComponent<PolyNavAgent>();
            nav.map = Utils.NavGrid;
            var m = monster.GetComponent<Monster>();
            m.Home = gameObject;
            m.Group = this;
            m.Level = 1;
            if (Target != null)
            {
                m.Target = Target;
            }
            Monsters.Add(m);
        }
    }

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
        Target = col.gameObject;
    }

    void OnTriggerExit2D(Collider2D col)
    {
        var monster = col.gameObject.GetComponent<Monster>();
        var player = col.gameObject.GetComponent<Player>();
        if ((player == null && monster == null) || (monster != null && !monster.Captured)) return;
        Target = null;
    }
}

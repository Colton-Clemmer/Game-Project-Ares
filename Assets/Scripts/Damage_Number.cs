using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Damage_Number : MonoBehaviour
{
    public int _lifeMillis = 500;

    public int _damageNumberForce_sett = 300;

    IEnumerator _despawnCorountine;

    IEnumerator _despawnFn()
    {
        yield return new WaitForSeconds((float) _damageNumberForce_sett / 1000f);
        Destroy(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Rigidbody2D>().AddForce(new Vector2(0, _damageNumberForce_sett));
        _despawnCorountine = _despawnFn();
        StartCoroutine(_despawnCorountine);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour
{
    public string Name;
    public int ID;
    public int Rarity;
    public Utils.Type MainType;
    public Utils.Type SubType;

    public int Damage;
    public int Stamina;
    public int Accuracy;

    public float _leapForce_sett;
    public float _telegraphTime_sett;
    public float _moveTimeLength_sett;
    public float _recoverTime_sett;


    private GameObject _parent;
    private Vector3 _moveDirection;

    IEnumerator MoveCoroutine;
    IEnumerator MoveFn()
    {
        yield return new WaitForSeconds(_telegraphTime_sett / 1000f);
        _parent.GetComponent<Animator>().SetTrigger("Stop_Telegraph");
        var rb = _parent.GetComponent<Rigidbody2D>();
        rb.AddForce(_moveDirection * _leapForce_sett);
        yield return new WaitForSeconds(_moveTimeLength_sett / 1000f);
        rb.velocity = Vector3.zero;
        yield return new WaitForSeconds(_recoverTime_sett / 1000f);
        _parent.GetComponent<Monster>().UsingMove = -1;
        var mon = _parent.GetComponent<Monster>();
        if (mon != null)
        {
            mon.EndMove();
        }
    }

    public void Execute(Vector3 direction, GameObject parent)
    {
        _parent = parent;
        _moveDirection = direction;
        parent.GetComponent<Animator>().SetTrigger("Telegraph");
        MoveCoroutine = MoveFn();
        StartCoroutine(MoveCoroutine);
    }
}

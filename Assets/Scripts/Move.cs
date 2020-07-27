using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeuralNetworks;

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
    public float _stunTime_sett;

    private bool _cancelled;

    private GameObject _parent;
    private Vector3 _moveDirection;

    /*
    Inputs:
        distance to target
        distance to home
    Outputs:
        Confidence in successful attack
        Distance needed
    */

    public NeuralNetwork MoveConfidence;
    public string ConfidencePath;
    public double[] ConfidenceInput;
    public double[] ConfidenceDecision;

    IEnumerator MoveCoroutine;
    IEnumerator MoveFn()
    {
        yield return new WaitForSeconds(_telegraphTime_sett / 1000f);
        if (!_cancelled)
        {
            _parent.GetComponent<Animator>().SetTrigger("Mv_Stomp");
            var rb = _parent.GetComponent<Rigidbody2D>();
            rb.AddForce(_moveDirection * _leapForce_sett);
            yield return new WaitForSeconds(_moveTimeLength_sett / 1000f);
            if (!_cancelled)
            {
                _parent.GetComponent<Animator>().SetTrigger("Recover");
                rb.velocity = Vector3.zero;
                yield return new WaitForSeconds(_recoverTime_sett / 1000f);
                _parent.GetComponent<Animator>().SetTrigger("Stop_Recover");
                if (!_cancelled)
                {
                    _parent.GetComponent<Monster>().UsingMove = -1;
                    var mon = _parent.GetComponent<Monster>();
                    if (mon != null)
                    {
                        mon.EndMove();
                    }
                }
            } else 
            {
                _parent.GetComponent<Animator>().SetTrigger("Recover");
                _parent.GetComponent<Animator>().SetTrigger("Stop_Recover");
            }
        } else 
        {
            _parent.GetComponent<Animator>().SetTrigger("Mv_Stomp");
            _parent.GetComponent<Animator>().SetTrigger("Recover");
            _parent.GetComponent<Animator>().SetTrigger("Stop_Recover");
        }
    }

    public void CancelMove()
    {
        _cancelled = true;
    }

    public void Execute(Vector3 direction, GameObject parent)
    {
        parent.GetComponent<Rigidbody2D>().velocity = Vector3.zero;
        var animator = parent.GetComponent<Animator>();
        animator.ResetTrigger("Telegraph");
        animator.ResetTrigger("Mv_Stomp");
        animator.ResetTrigger("Recover");
        animator.ResetTrigger("Stop_Recover");
        _cancelled = false;
        _parent = parent;
        _moveDirection = direction;
        parent.GetComponent<Animator>().SetTrigger("Telegraph");
        MoveCoroutine = MoveFn();
        StartCoroutine(MoveCoroutine);
    }
}

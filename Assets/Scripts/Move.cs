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
    public Vector3 MoveDirection;

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
        var animator = _parent.GetComponent<Animator>();
        yield return new WaitForSeconds(_telegraphTime_sett / 1000f);
        if (!_cancelled)
        {
            animator.SetTrigger("Mv_Stomp");
            var rb = _parent.GetComponent<Rigidbody2D>();
            var direction = MoveDirection * _leapForce_sett;
            Debug.Log(direction);
            rb.AddForce(direction);
            yield return new WaitForSeconds(_moveTimeLength_sett / 1000f);
            if (!_cancelled)
            {
                animator.SetTrigger("Recover");
                rb.velocity = Vector3.zero;
                yield return new WaitForSeconds(_recoverTime_sett / 1000f);
                animator.SetTrigger("Stop_Recover");
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
                animator.Play("Idle");
                animator.ResetTrigger("Telegraph");
                animator.ResetTrigger("Mv_Stomp");
                animator.ResetTrigger("Recover"); 
                animator.ResetTrigger("Stop_Recover");
            }
        } else 
        {
            animator.Play("Idle");
            animator.ResetTrigger("Telegraph");
            animator.ResetTrigger("Mv_Stomp");
            animator.ResetTrigger("Recover"); 
            animator.ResetTrigger("Stop_Recover");
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
        MoveDirection = direction;
        parent.GetComponent<Animator>().SetTrigger("Telegraph");
        MoveCoroutine = MoveFn();
        StartCoroutine(MoveCoroutine);
    }
}

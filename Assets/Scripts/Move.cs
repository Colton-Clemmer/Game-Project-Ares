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
    public int Moves;
    public int Accuracy;

    public float _leapDistance_sett;
    public float _telegraphTime_sett;
    public float _moveTimeLength_sett;
    public float _recoverTime_sett;



    public void Execute(Vector3 direction)
    {
        
    }
}

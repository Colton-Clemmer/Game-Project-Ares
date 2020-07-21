using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PolyNav;

public class Utils : MonoBehaviour
{
    public List<GameObject> MoveList;

    public enum Type
    {
        Normal,
        Fire,
        Fighting,
        Water,
        Flying,
        Grass,
        Poison,
        Electric,
        Ground,
        Psychic,
        Rock,
        Ice,
        Bug,
        Dragon,
        Ghost
    }

    public static string GetStringFromType(Type type)
    {
        switch (type)
        {
            case Type.Bug:
                return "Bug";
            case Type.Dragon:
                return "Dragon";
            case Type.Electric:
                return "Electric";
            case Type.Fighting:
                return "Fighting";
            case Type.Fire:
                return "Fire";
            case Type.Flying:
                return "Flying";
            case Type.Ghost:
                return "Ghost";
            case Type.Grass:
                return "Grass";
            case Type.Ground:
                return "Ground";
            case Type.Ice:
                return "Ice";
            case Type.Normal:
                return "Normal";
            case Type.Poison:
                return "Poison";
            case Type.Psychic:
                return "Psychic";
            case Type.Rock:
                return "Rock";
            case Type.Water:
                return "Water";
        }
        return null;
    }

    public enum Item
    {
        Capture_Ball
    }

    public GameObject CaptureBall;

    public static GameObject Camera
    { get { return GameObject.FindWithTag("Camera_Container"); } }
    
    public static Utils Util
    { get { return GameObject.FindWithTag("Game_Manager").GetComponent<Utils>(); } }

    public static PolyNav2D NavGrid
    { get { return GameObject.FindWithTag("Nav_Grid").GetComponent<PolyNav2D>(); } }

    public Text ItemValue;

    public GameObject Player;

    public List<GameObject> ItemIndicators;

    public List<GameObject> MonsterIndicators;

    // Stats UI
    public GameObject StatsBoard;
    public GameObject MoveBoard;
    public Text TypeValue;
    public Text SubTypeValue;
    public Text LevelValue;
    public Text AttackValue;
    public Text SpeedValue;
    public Text DefenceValue;
    public Text SpAttackValue;
    public Text SpDefenceValue;

    // Moves UI

    public Text Move1Name;
    public Text Move1Damage;
    public Text Move1Stamina;
    public Text Move1Accuracy;
    public Text Move2Name;
    public Text Move2Damage;
    public Text Move2Stamina;
    public Text Move2Accuracy;
    public Text Move3Name;
    public Text Move3Damage;
    public Text Move3Stamina;
    public Text Move3Accuracy;
    public Text Move4Name;
    public Text Move4Damage;
    public Text Move4Stamina;
    public Text Move4Accuracy;

    // Targeting UI
    public Text TargetLvlValue;
    public Text TargetTypeValue;
}

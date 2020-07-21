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
}

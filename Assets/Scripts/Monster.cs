﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class Monster : MonoBehaviour
{
    public List<Move> Moves;
    public Utils.Type MainType;
    public Utils.Type SubType;
    public int MaxHealth;
    private int _healthStat;
    public int CurrentHealth;
    public int Level;
    public int Speed;
    public int Attack;
    public int Defence;
    public int SpAttack;
    public int SpDefence;

    public bool Generated;
    public bool Captured;
    public bool Stunned;
    public int UsingMove = -1;

    [SerializeField] private float _attackWeight_sett = .5f;
    [SerializeField] private float _damageNumberForce_sett = 100;
    private int _maxStartingStat_sett = 5;
    private int _maxMoves_sett = 4;
    private int _minStartHealth_sett = 20;
    private int _startingPoints_sett = 100;
    private int _maxStatIncreasePerLevel_sett = 4;
    private float _singleTypeChance_sett = .75f;
    private float _baseMainTypeMoveChance_sett = .4f;
    private float _baseSubTypeMoveChance_sett = .3f;

    [SerializeField] private TextMeshPro _levelValue;
    [SerializeField] private TextMeshPro _typeValue;
    [SerializeField] private TextMeshPro _subTypeValue;

    [SerializeField] private GameObject _moveUseBackGround;
    [SerializeField] private TextMeshPro _useMoveValue;
    [SerializeField] private GameObject _healthBar;
    [SerializeField] private GameObject _damageNumber;

    void Start()
    {
        // TODO: Add moves as monsters level up
        // Add move level requirements
        Generate(1);
        _updateText();
        UsingMove = -1;
    }

    void Update()
    {
        if (Captured)
        {
            if (UsingMove == -1 && !Stunned)
            {
                Utils.Util.Player.GetComponent<Player>().UpdateFn();
            }
            else
            {
                Utils.Camera.transform.position = transform.position;
            }
        }
    }

    public void Generate(int lvl)
    {
        Level = lvl;
        _increaseStats(_startingPoints_sett);
        _pickType();
        MaxHealth = _healthStat + _minStartHealth_sett;
        for (var i = 0; i < _maxMoves_sett; i++)
        {
            GenerateMove();
        }
        for (var i = 0; i < lvl; i++)
        {
            LevelUp();
        }
        Generated = true;
    }

    public void GenerateMove(int attempt = 0)
    {
        if (attempt > 1000) return;
        // TODO: Ensure that moves cannot be generated for monsters that will make them invalid
        var allMoves = Utils.Util.MoveList.Select(m => m.GetComponent<Move>()).ToList();
        var mainTypeMoves = allMoves.Where(m => m.MainType == MainType || m.SubType == SubType).ToList();
        var subTypeMoves = allMoves.Where(m => m.MainType == SubType || m.SubType == SubType).ToList();

        var useMainType = UnityEngine.Random.value > _baseMainTypeMoveChance_sett;
        var useSubType = !useMainType && UnityEngine.Random.value > _baseSubTypeMoveChance_sett;

        var moveList = useMainType ? mainTypeMoves : useSubType ? subTypeMoves : allMoves;

        var moveIndex = (int)(UnityEngine.Random.value * moveList.Count());
        if (moveIndex == moveList.Count())
        {
            moveIndex = moveList.Count() - 1;
        }
        if (Moves.Any(m => m.ID == moveIndex) || !moveList.Any())
        {
            GenerateMove(attempt + 1);
            return;
        }
        if (Moves.Any(m => moveList[moveIndex].ID == m.ID))
        {
            GenerateMove(attempt + 1);
            return;
        }
        var newMove = Instantiate(moveList[moveIndex]);
        newMove.transform.SetParent(transform);
        newMove.transform.position = Vector3.zero;
        Moves.Add(newMove.GetComponent<Move>());
    }

    private void _updateText()
    {
        _levelValue.text = Level.ToString();
        _typeValue.text = Utils.GetStringFromType(MainType);
        _subTypeValue.text = MainType == SubType ? "" : Utils.GetStringFromType(SubType);
    }

    private void _updateHealthBar()
    {
        _healthBar.SetActive(true);
        var healthScale = _healthBar.transform.localScale;
        healthScale.x = CurrentHealth / MaxHealth;
        _healthBar.transform.localScale = healthScale;
    }

    private void _increaseStats(int remainingPoints, List<int> statsDone = null)
    {
        if (statsDone == null) statsDone = new List<int>();
        if (remainingPoints <= 0) return;
        var stat = (int)(UnityEngine.Random.value * 6);
        if (stat == 6) stat = 5;
        if (statsDone.Contains(stat))
        {
            _increaseStats(remainingPoints, statsDone);
            return;
        }
        statsDone.Add(stat);
        var amount = (int)(UnityEngine.Random.value * _maxStartingStat_sett);
        remainingPoints -= amount;
        switch (stat)
        {
            case 0:
                _healthStat += amount;
                break;
            case 1:
                Speed += amount;
                break;
            case 2:
                Attack += amount;
                break;
            case 3:
                Defence += amount;
                break;
            case 4:
                SpAttack += amount;
                break;
            case 5:
                SpDefence += amount;
                break;
        }
        if (statsDone.Count() == 5) statsDone.RemoveAll(s => true);
        if (remainingPoints > 0) _increaseStats(remainingPoints, statsDone);
        CurrentHealth = MaxHealth;
    }

    private void _pickType()
    {
        var hasSubType = UnityEngine.Random.value > _singleTypeChance_sett;
        var typeIndex = (int)(UnityEngine.Random.value * 15);
        if (typeIndex == 15) typeIndex = 15;
        MainType = (Utils.Type)typeIndex;
        // TODO: Add weights depending upon the initialized stats to determine type
        if (hasSubType)
        {
            typeIndex = (int)(UnityEngine.Random.value * 15);
            SubType = (Utils.Type)typeIndex;
        }
        else
        {
            SubType = MainType;
        }
    }

    public void LevelUp()
    {
        var highestStatBestChance = UnityEngine.Random.value > .5f;
        MaxHealth += (int)(_healthStat * .25f);
        if (highestStatBestChance)
        {
            var totalStats = Attack + Speed + Defence + SpAttack + SpDefence;
            var levelUpIndex = (int)(UnityEngine.Random.value * totalStats);
            var increase = (int)(UnityEngine.Random.value * _maxStatIncreasePerLevel_sett) + 1;
            if (levelUpIndex < Attack)
            {
                Attack += increase;
            }
            if (levelUpIndex > Attack && levelUpIndex < Attack + Speed)
            {
                Speed += increase;
            }
            if (levelUpIndex > Attack + Speed && levelUpIndex < Attack + Speed + Defence)
            {
                Defence += increase;
            }
            if (levelUpIndex > Attack + Speed + Defence && levelUpIndex < Attack + Speed + Defence + SpAttack)
            {
                SpAttack += increase;
            }
            if (levelUpIndex > Attack + Speed + Defence + SpAttack)
            {
                SpDefence += increase;
            }
        }
    }

    public void UseMove(int moveIndex, Vector3 direction)
    {
        if (UsingMove >= 0) return;
        if (moveIndex > Moves.Count() - 1) return;
        UsingMove = moveIndex;
        _moveUseBackGround.SetActive(true);
        _useMoveValue.text = "Using " + Moves[moveIndex].Name;
        Moves[moveIndex].Execute(direction, gameObject);
    }

    public void EndMove()
    {
        _moveUseBackGround.SetActive(false);
        if (UsingMove != -1)
        {
            Moves[UsingMove].CancelMove();
        }
        GetComponent<Animator>().SetTrigger("Stop_Telegraph");
        UsingMove = -1;
    }

    IEnumerator _stunCoroutine;
    
    IEnumerator _stunFn(Move move)
    {
        Stunned = true;
        GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
        yield return new WaitForSeconds((float) move._stunTime_sett / 1000f);
        GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
        Stunned = false;
    }

    private void _receiveDamage(Move move, Monster attackingMonster)
    {
        attackingMonster.EndMove();
        var damage = (float)move.Damage * Utils.GetTypeRelation(move.MainType, MainType);
        float attack =  move.MainType == attackingMonster.MainType ? attackingMonster.SpAttack : attackingMonster.Attack;
        float defence = move.MainType == MainType ? SpDefence : Defence;
        var amount = (int)(damage * (attack / defence) * _attackWeight_sett);
        CurrentHealth -= amount;
        Instantiate(_damageNumber);
        _damageNumber.transform.position = transform.position + ((transform.position - attackingMonster.transform.position).normalized * .5f);
        _damageNumber.GetComponent<TextMeshPro>().text = amount.ToString();
        _updateHealthBar();
        if (move._stunTime_sett > 0)
        {
            _stunCoroutine = _stunFn(move);
            StartCoroutine(_stunCoroutine);
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        var monster = col.gameObject.GetComponent<Monster>();
        if (monster == null || monster.UsingMove == -1) return;
        var move = monster.Moves[monster.UsingMove];
        _receiveDamage(move, monster);
    }
}

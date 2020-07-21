using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Monster : MonoBehaviour
{
    public bool Generated;
    public List<Move> Moves;
    public Utils.Type MainType;
    public Utils.Type SubType;
    public int MaxHealth;
    private int _healthStat;
    public int Level;
    public int Speed;
    public int Attack;
    public int Defence;
    public int SpAttack;
    public int SpDefence;

    public bool Captured;
    public bool UsingMove;

    private int _maxStartingStat_sett = 5;
    private int _maxMoves_sett = 4;
    private int _minStartHealth_sett = 20;
    private int _startingPoints_sett = 100;
    private int _maxStatIncreasePerLevel_sett = 4;
    private float _singleTypeChance_sett = .75f;
    private float _baseMainTypeMoveChance_sett = .4f;
    private float _baseSubTypeMoveChance_sett = .3f;

    void Start()
    {
        // TODO: Add moves as monsters level up
        // Add move level requirements
        Generate(1);
    }

    void Update()
    {
        if (Captured)
        {
            if (!UsingMove)
            {
                Utils.Util.Player.GetComponent<Player>().UpdateFn();
            } else 
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
        for (var i = 0; i < lvl;i++)
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

        var moveIndex = (int) (UnityEngine.Random.value * moveList.Count());
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

    private void _increaseStats(int remainingPoints, List<int> statsDone = null)
    {
        if (statsDone == null) statsDone = new List<int>();
        if (remainingPoints <= 0) return;
        var stat = (int) (UnityEngine.Random.value * 6);
        if (stat == 6) stat = 5;
        if (statsDone.Contains(stat)) 
        {
            _increaseStats(remainingPoints, statsDone);
            return;
        }
        statsDone.Add(stat);
        var amount = (int) (UnityEngine.Random.value * _maxStartingStat_sett);
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
    }

    private void _pickType()
    {
        var hasSubType = UnityEngine.Random.value > _singleTypeChance_sett;
        var typeIndex = (int) (UnityEngine.Random.value * 15);
        if (typeIndex == 15) typeIndex = 15;
        MainType = (Utils.Type) typeIndex;
        // TODO: Add weights depending upon the initialized stats to determine type
        if (hasSubType)
        {
            typeIndex = (int) (UnityEngine.Random.value * 15);
            SubType = (Utils.Type) typeIndex;
        } else 
        {
            SubType = MainType;
        }
    }

    public void LevelUp()
    {
        var highestStatBestChance = UnityEngine.Random.value > .5f;
        MaxHealth += (int) (_healthStat * .25f);
        if (highestStatBestChance)
        {
            var totalStats = Attack + Speed + Defence + SpAttack + SpDefence;
            var levelUpIndex = (int) (UnityEngine.Random.value * totalStats);
            var increase = (int) (UnityEngine.Random.value * _maxStatIncreasePerLevel_sett) + 1;
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
        if (UsingMove) return;
        if (moveIndex > Moves.Count() - 1) return;
        UsingMove = true;
        Moves[moveIndex].Execute(direction, gameObject);
    }
}

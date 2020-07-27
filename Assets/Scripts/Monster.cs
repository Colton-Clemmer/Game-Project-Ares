using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;
using UnityEngine;
using PolyNav;
using TMPro;
using NeuralNetworks;

public class Monster : MonoBehaviour
{
    public List<Move> Moves;
    public Utils.Type MainType;
    public Utils.Type SubType;
    public int MaxHealth;
    private int _healthStat;
    public int CurrentHealth;
    public int Stamina = 100;
    public int CurrentStamina = 100;
    public int CurrentEndurance = 100;
    public int Experience;
    public int Level = 1;
    public int Speed;
    public int Attack;
    public int Defence;
    public int SpAttack;
    public int SpDefence;

    public bool Generated;
    public bool Captured;
    public bool Stunned;
    public int UsingMove = -1;

    [SerializeField] private float _staminaRegenTime_sett = 200;
    [SerializeField] private float _staminaRegenDelay_sett = 600;
    [SerializeField] private float _attackWeight_sett = .5f;
    [SerializeField] private float _damageNumberForce_sett = 100;
    [SerializeField] private float _enduranceDivider_sett = 4;
    [SerializeField] private float _experienceMultiplier = 1;
    private int _maxStartingStat_sett = 5;
    private int _maxMoves_sett = 4;
    private int _minStartHealth_sett = 20;
    private int _startingPoints_sett = 100;
    private int _maxStatIncreasePerLevel_sett = 4;
    private float _singleTypeChance_sett = .75f;
    private float _baseMainTypeMoveChance_sett = .4f;
    private float _baseSubTypeMoveChance_sett = .3f;
    private float _experienceNotificationTime_sett = 1500;
    private float _useMoveThreshold = .7f;

    public bool Dead
    { get { return CurrentHealth <= 0; } }

    public GameObject Target;
    public GameObject Home;

    [SerializeField] private TextMeshPro _levelValue;
    [SerializeField] private TextMeshPro _typeValue;
    [SerializeField] private TextMeshPro _subTypeValue;

    [SerializeField] private GameObject _moveUseBackGround;
    [SerializeField] private TextMeshPro _useMoveValue;
    [SerializeField] private GameObject _healthBar;
    [SerializeField] private GameObject _damageNumber;
    [SerializeField] private GameObject _experienceText;

    private float _homeCloseDistance_sett = .5f;
    private float _distanceToHome
    { get { return (transform.position - Home.transform.position).magnitude; } }

    private float _distanceToTarget
    { get { return Target == null ? 200f : (transform.position - Target.transform.position).magnitude; } }

    private NeuralNetwork _brain;
    private string _brainPath = "../Neural_Data/monster.json";
    private double[] _brainInput;
    private double[] _brainDecision;

    /*
    Inputs:
        confidence from move 0 - 3 networks
        average distance needed
        distance to player
        distance to home
    Outputs:
        use move 0 - 3 (four outputs)
        idle 0 - 1
        follow 0 - 1
        retreat 0 - 1
    */

    void Start()
    {
        // TODO: Add moves as monsters level up
        // Add move level requirements
        if (File.Exists(_brainPath))
        {
            _brain = NeuralNetwork.Load(_brainPath);
        } else {
            _brain = new NeuralNetwork(6, 30, 7);
        }
        Generate(Level);
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
        } else
        {
            _wildUpdate();
        }
    }

    private void _wildUpdate()
    {
        var confidenceInput = new double[] { (double) _distanceToTarget , (double) _distanceToHome };
        var moveConfidences = Moves.Select(m => m.MoveConfidence.FeedForward(confidenceInput)).ToList();
        double averageDistanceNeeded = moveConfidences.Average(c => c[1]);
        var inp = new double[7] { 
            (double) moveConfidences[0][0], 
            (double) moveConfidences[1][0], 
            Moves.Count() < 3 ? 0 :(double) moveConfidences[2][0], 
            Moves.Count() < 4 ? 0 : (double) moveConfidences[3][0],
            averageDistanceNeeded,
            (double) _distanceToTarget,
            (double) _distanceToHome
        };
        var output = _brain.FeedForward(inp);

        if (Target != null && UsingMove == -1)
        {
            _brainInput = inp;
            _brainDecision = output.Select(o => (double) o).ToArray();
            for (var i = 0; i < Moves.Count();i++)
            {
                Moves[i].ConfidenceInput = confidenceInput;
                Moves[i].ConfidenceDecision = moveConfidences[i].Select(c => (double) c).ToArray();
                if (output[i] > _useMoveThreshold)
                {
                    UseMove(i, (transform.position - Target.transform.position).normalized);
                    break;
                }
            }
        }
        Debug.Log(output[0] + " " + output[1] + " " + output[2] + " " + output[3] + " " + output[4] + " " + output[5] + " " + output[6]);

        var nav = GetComponent<PolyNavAgent>();
        if (output[4] > _useMoveThreshold)
        {
            nav.Stop();
        }

        if (output[5] > _useMoveThreshold && Target != null)
        {
            nav.SetDestination(Target.transform.position);
        }

        if (output[6] > _useMoveThreshold)
        {
            nav.SetDestination(Home.transform.position);
        }

        if (output[5] < _useMoveThreshold && output[6] < _useMoveThreshold)
        {
            nav.Stop();
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
        Experience = (int) Mathf.Pow((float) Math.E, (float) Level);
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
        newMove.ConfidencePath = "../Neural_Data/Mv_" + newMove.Name + ".json";
        if (File.Exists(newMove.ConfidencePath))
        {
            newMove.MoveConfidence = NeuralNetwork.Load(newMove.ConfidencePath);
        } else
        {
            newMove.MoveConfidence = new NeuralNetwork(2, 10, 2);
        }
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
        var move = Moves[moveIndex];
        if (CurrentStamina < move.Stamina || CurrentEndurance < move.Stamina / _enduranceDivider_sett) return;
        CurrentStamina -= move.Stamina;
        CurrentEndurance -= (int) ((float) move.Stamina / (float) _enduranceDivider_sett);
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

    public void PraiseAttack(int times = 1)
    {
        if (times == 0)
        {
            _brain.Save(_brainPath);
            Moves[UsingMove].MoveConfidence.Save(Moves[UsingMove].ConfidencePath);
            return;
        }
        _brain.Backpropagate(_brainInput, _brainDecision);
        Moves[UsingMove].MoveConfidence.Backpropagate(Moves[UsingMove].ConfidenceInput, Moves[UsingMove].ConfidenceDecision);
        times--;
        PraiseAttack(times);
    }

    public void Shame(int times = 1)
    {
        if (times == 0)
        {
            _brain.Save(_brainPath);
            Moves[UsingMove].MoveConfidence.Save(Moves[UsingMove].ConfidencePath);
            return;
        }
        _brain.Backpropagate(_brainInput, new double[] {
            _brainDecision[0], 
            _brainDecision[1], 
            _brainDecision[2], 
            _brainDecision[3], 
            UnityEngine.Random.value,
            UnityEngine.Random.value,
            UnityEngine.Random.value
        });
        times--;
        Shame(times);
    }

    private void _receiveDamage(Move move, Monster attackingMonster)
    {
        if (!attackingMonster.Captured)
        {
            attackingMonster.PraiseAttack();
        }
        if (!Captured)
        {
            Shame();
        }
        attackingMonster.EndMove();
        var damage = (float)move.Damage * Utils.GetTypeRelation(move.MainType, MainType);
        float attack =  move.MainType == attackingMonster.MainType ? attackingMonster.SpAttack : attackingMonster.Attack;
        float defence = move.MainType == MainType ? SpDefence : Defence;
        var amount = (int)(damage * (attack / defence) * _attackWeight_sett);
        CurrentHealth -= amount;
        Instantiate(_damageNumber);
        _damageNumber.transform.position = transform.position + ((transform.position - attackingMonster.transform.position).normalized * .5f);
        _damageNumber.GetComponent<TextMeshPro>().text = amount.ToString();
        if (Dead)
        {
            _kill(attackingMonster);
            return;
        }
        _updateHealthBar();
        if (move._stunTime_sett > 0)
        {
            _stunCoroutine = _stunFn(move);
            StartCoroutine(_stunCoroutine);
        }
    }

    private void _kill(Monster attackingMonster)
    {
        if (!Dead) return;
        var experience = Mathf.Pow((float) Math.E, (float) Level) * _experienceMultiplier;
        attackingMonster.AddExperience((int) experience);
        Destroy(gameObject);
    }

    public void StartStaminaRegen()
    {
        _staminaRegenCoroutine = _staminaRegenFn();
        StartCoroutine(_staminaRegenCoroutine);
    }

    public void AddExperience(int amount)
    {
        Experience += amount;
        if (Experience >= Mathf.Pow((float) Math.E, (float) Level + 1f))
        {
            LevelUp();
        }
        _experienceAddCoroutine = _experienceAddFn(amount);
        StartCoroutine(_experienceAddCoroutine);
    }

    IEnumerator _experienceAddCoroutine;

    IEnumerator _experienceAddFn(int amount)
    {
        var notification = Utils.Util.ExperienceNotification;
        notification.gameObject.SetActive(true);
        notification.text = "Gained " + amount + " experience";
        yield return new WaitForSeconds(_experienceNotificationTime_sett / 1000f);
        notification.gameObject.SetActive(false);
        Utils.Util.Player.GetComponent<Player>().UpdateExperienceUi();
    }

    IEnumerator _staminaRegenCoroutine;
    private bool _usedMove;

    IEnumerator _staminaRegenFn()
    {
        yield return new WaitForSeconds((_usedMove ? _staminaRegenDelay_sett : _staminaRegenTime_sett) / 1000f);
        if (UsingMove != -1)
        {
            _usedMove = true;
        }
        if (CurrentStamina < Stamina && UsingMove == -1)
        {
            CurrentStamina++;
            Utils.Util.Player.GetComponent<Player>().UpdateStaminaUi();
            _usedMove = false;
        }
        _staminaRegenCoroutine = _staminaRegenFn();
        StartCoroutine(_staminaRegenCoroutine);
        
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        var monster = col.gameObject.GetComponent<Monster>();
        if (monster == null || monster.UsingMove == -1) return;
        var move = monster.Moves[monster.UsingMove];
        _receiveDamage(move, monster);
    }
}

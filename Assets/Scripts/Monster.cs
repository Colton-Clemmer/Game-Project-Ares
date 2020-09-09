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
    private float _maxTargetingDistance_sett = 20;
    private float _defaultStrikeDistance_sett = 2;
    private float _damagePushForceMultiplier_sett = 10;
    private float _homeStopDistance = .5f;

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
    [SerializeField] private Group_Controller Group;

    private float _distanceToHome
    { get { return (transform.position - Home.transform.position).magnitude; } }

    private float _distanceToTarget
    { get { return Target == null ? 200f : (transform.position - Target.transform.position).magnitude; } }

    void Start()
    {
        Generate(Level);
        _updateText();
        UsingMove = -1;
        _wildUpdateCoroutine = _wildUpdate();
        StartCoroutine(_wildUpdateCoroutine);
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

    private GameObject _getClosestCapturedMonster(float minDistance)
    {
        var monsters = Utils.GetCapturedMonsters();
        return monsters
            .Where(m => (m.transform.position - transform.position).magnitude < minDistance)
            .OrderBy(m => (m.transform.position - transform.position).magnitude).FirstOrDefault();
    }

    IEnumerator _wildUpdateCoroutine;

    IEnumerator _wildUpdate()
    {
        yield return new WaitForSeconds(.2f);
        
        var stopped = false;
        var nav = GetComponent<PolyNavAgent>();

        // Player has entered boundary and monster does not have target
        if (Group.Target != null && Target == null)
        {
            var closestMonster = _getClosestCapturedMonster(_maxTargetingDistance_sett);
            if (closestMonster != null)
            {
                Target = closestMonster;
            }
        }

        // Player has left boundary and target is aquired
        if (Group.Target == null && Target != null)
        {
            Target = null;
        }

        // Go to target if it exists
        if (Target != null && !Stunned)
        {
            nav.SetDestination(Target.transform.position);
        }

        if (_distanceToTarget < _defaultStrikeDistance_sett && UsingMove == -1 && !Stunned)
        {
            UseMove((int) (UnityEngine.Random.value * Moves.Count()), (Target.transform.position - transform.position).normalized);
        }

        // Go to home if no target
        if (Target == null)
        {
            nav.SetDestination(Home.transform.position);
        }

        // Stop at home when close
        if (_distanceToHome < _homeStopDistance && Target == null)
        {
            nav.Stop();
            stopped = true;
        }

        if (stopped && UsingMove == -1)
        {
            GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        }
        if (!Captured)
        {
            _wildUpdateCoroutine = _wildUpdate();
            StartCoroutine(_wildUpdateCoroutine);
        }
    }

    public void Generate(int lvl)
    {
        Level = lvl;
        _increaseStats(_startingPoints_sett);
        _pickType();
        MaxHealth = _healthStat + _minStartHealth_sett;
        CurrentHealth = MaxHealth;
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
        newMove.ConfidencePath = "Assets/Neural_Data/Mv_" + newMove.Name + ".json";
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
        healthScale.x = (float) CurrentHealth / (float) MaxHealth;
        _healthBar.transform.localScale = healthScale;
    }

    private void _increaseStats(int remainingPoints, List<int> statsDone = null)
    {
        if (statsDone == null) statsDone = new List<int>();
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
    
    IEnumerator _stunFn(float stunTime, bool stop = true)
    {
        Stunned = true;
        if (stop)
        {
            GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
        }
        yield return new WaitForSeconds((float) stunTime / 1000f);
        if (stop)
        {
            GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
        }
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
        Debug.Log(amount);
        if (Dead)
        {
            _kill(attackingMonster);
            return;
        }
        _updateHealthBar();
    }

    private void _kill(Monster attackingMonster)
    {
        if (!Dead) return;
        var experience = Mathf.Pow((float) Math.E, (float) Level) * _experienceMultiplier;
        attackingMonster.AddExperience((int) experience);
        if (Captured)
        {
            var player = Utils.Util.Player.GetComponent<Player>();
            var monster = player.MonstersCaptured[player.MonsterIndex];
            if (monster == this)
            {
                player.RemoveMonster(this);
            }
        }
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
        var rb = GetComponent<Rigidbody2D>();

        if (UsingMove != -1)
        {
            if (Moves[UsingMove].Damage < move.Damage)
            {
                var direction = Moves[UsingMove].MoveDirection.normalized;
                var enemyDirection = monster.Moves[monster.UsingMove].MoveDirection.normalized;
                var forceChanger = (float) Moves[UsingMove].Damage / (float) move.Damage;
                rb.AddForce(enemyDirection + (direction * forceChanger) * _damagePushForceMultiplier_sett);
                monster.GetComponent<Rigidbody2D>().velocity = Vector3.zero;
            } else 
            {
                rb.velocity = Vector3.zero;
            }
        } else 
        {
            var direction = monster.Moves[monster.UsingMove].MoveDirection.normalized * _damagePushForceMultiplier_sett;
            rb.velocity = direction;
            monster.GetComponent<Rigidbody2D>().velocity = Vector3.zero;
        }

        _receiveDamage(move, monster);
    }
}

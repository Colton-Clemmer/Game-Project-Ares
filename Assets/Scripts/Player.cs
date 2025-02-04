﻿/*
Stamina will reduce every time a move is used, it is used up quickly but will regenerate quickly
Endurance will reduce every time a move is used also, but less so. Endurance does not regenerate and must be restored by other methods,a
*/


using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

public class Player : MonoBehaviour
{
    #region Data
    [SerializeField] private GameObject StartingMonster;

    public List<Monster> MonstersCaptured;
    public List<Utils.Item> Items;

    #endregion

    #region Settings

    [SerializeField] private float _idleStopForce_sett = 10;
    [SerializeField] private float _stopForce_sett = 3;
    [SerializeField] private float _moveForce_sett = 5;
    [SerializeField] private float _maxSpeet_sett = 5;
    [SerializeField] private float _ballStartDistance_sett = 2;
    [SerializeField] private float _ballThrowForce = 10;
    [SerializeField] private float _monsterSwitchMillis = 500;
    [SerializeField] private float _itemUseMillis = 500;
    
    #endregion

    #region State

    public int ItemUseIndex;
    public int MonsterIndex = -1;

    private DateTime _lastMonsterSwitch;

    private DateTime _lastItemUsed;
    private bool _zoomedOut;

    #endregion

    #region Computed

    private Vector3 mouseDirection
    { get { return (transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition)).normalized * -1; } }

    public Monster CurrentMonster
    { get { return MonsterIndex == -1 ? null : MonstersCaptured[MonsterIndex]; } }

    private Rigidbody2D _rb
    { get { return CurrentMonster == null ? GetComponent<Rigidbody2D>() : CurrentMonster.GetComponent<Rigidbody2D>(); } }

    #endregion

    #region Mono

    void Start()
    {
        _lastMonsterIndex = MonsterIndex;
        CaptureMonster(StartingMonster);
        _updateItemText();        
        _updateMonsterUi();
    }
 
    void Update()
    {
        UpdateFn();
    }

    public void UpdateFn()
    {
        Utils.Camera.transform.position = transform.position;
        _getMovement();
        _getZoom();
        _selectMonster();
        _selectItem();
        _useMove();
        _checkForInfo();
        if (Input.GetKey(KeyCode.Mouse1) && Input.GetKeyDown(KeyCode.Mouse0) && MonsterIndex == -1)
        {
            _useItem();
        }
    }

    #endregion

    #region UI

    private void _updateMonsterUi()
    {
        var indicators = Utils.Util.MonsterIndicators;
        foreach(var indicator in indicators)
        {
            indicator.SetActive(false);
        }
        for (var i = 0; i < MonstersCaptured.Count();i++)
        {
            indicators[i].SetActive(true);
        }
    }

    private void _updateMonsterInfoUi()
    {
        if (CurrentMonster == null) return;
        var util = Utils.Util;
        util.LevelValue.text = CurrentMonster.Level.ToString();
        util.SpeedValue.text = CurrentMonster.Speed.ToString();
        util.AttackValue.text = CurrentMonster.Attack.ToString();
        util.DefenceValue.text = CurrentMonster.Defence.ToString();
        util.SpAttackValue.text = CurrentMonster.SpAttack.ToString();
        util.SpDefenceValue.text = CurrentMonster.SpDefence.ToString();
        util.TypeValue.text = Utils.GetStringFromType(CurrentMonster.MainType);
        util.SubTypeValue.text = CurrentMonster.MainType == CurrentMonster.SubType ? "None" : Utils.GetStringFromType(CurrentMonster.SubType);

        for(var i = 0; i < CurrentMonster.Moves.Count(); i++)
        {   
            var move = CurrentMonster.Moves[i];
            if (i == 0)
            {
                util.Move1Name.text = move.Name;
                util.Move1Damage.text = move.Damage.ToString();
                util.Move1Stamina.text = move.Stamina.ToString();
                util.Move1Accuracy.text = move.Accuracy.ToString();
            }
            if (i == 1)
            {
                util.Move2Name.text = move.Name;
                util.Move2Damage.text = move.Damage.ToString();
                util.Move2Stamina.text = move.Stamina.ToString();
                util.Move2Accuracy.text = move.Accuracy.ToString();
            }
            if (i == 2)
            {
                util.Move3Name.text = move.Name;
                util.Move3Damage.text = move.Damage.ToString();
                util.Move3Stamina.text = move.Stamina.ToString();
                util.Move3Accuracy.text = move.Accuracy.ToString();
            }
            if (i == 3)
            {
                util.Move4Name.text = move.Name;
                util.Move4Damage.text = move.Damage.ToString();
                util.Move4Stamina.text = move.Stamina.ToString();
                util.Move4Accuracy.text = move.Accuracy.ToString();
            }
        }
    }

    public void UpdateStaminaUi()
    {
        var staminaScale = Utils.Util.StaminaBar.transform.localScale;
        staminaScale.x = (float) CurrentMonster.CurrentStamina / (float) CurrentMonster.Stamina;
        Utils.Util.StaminaBar.transform.localScale = staminaScale;
        var enduranceScale = Utils.Util.EnduranceBar.transform.localScale;
        enduranceScale.x = (float) CurrentMonster.CurrentEndurance / (float) CurrentMonster.Stamina;
        Utils.Util.EnduranceBar.transform.localScale = enduranceScale;
    }

    public void UpdateExperienceUi()
    {
        if (CurrentMonster == null) return;
        var experienceScale = Utils.Util.ExperienceBar.transform.localScale;
        experienceScale.x = Math.Abs((CurrentMonster.Experience - Mathf.Pow((float) Math.E, (float) CurrentMonster.Level)) / (Mathf.Pow((float) Math.E, (float) CurrentMonster.Level + 1f) - Mathf.Pow((float) Math.E, (float) CurrentMonster.Level)));
        Utils.Util.ExperienceBar.transform.localScale = experienceScale;
    }

    private void _updateItemText()
    {
        if (ItemUseIndex != -1)
        {
            switch (Items[ItemUseIndex])
            {
                case Utils.Item.Capture_Ball:
                    Utils.Util.ItemValue.text = "Capture Ball";
                    break;
            }
        } else 
        {
            Utils.Util.ItemValue.text = "None";
        }
        var indicators = Utils.Util.ItemIndicators;
        foreach(var indicator in indicators)
        {
            indicator.SetActive(false);
        }
        if (ItemUseIndex != -1)
        {
            for (var i = 0; i < Items.Count(item => item == Items[ItemUseIndex]);i++)
            {
                indicators[i].SetActive(true);
            }
        }
    }

    #endregion

    #region Monsters

    private int _lastMonsterIndex;

    public void ChangeMonster(int monsterIndex)
    {
        MonsterIndex = monsterIndex;
        if (MonsterIndex >= 0)
        {
            Utils.Util.StaminaGauge.SetActive(true);
            Utils.Util.EnduranceGauge.SetActive(true);
            Utils.Util.ExperienceGauge.SetActive(true);
            gameObject.SetActive(false);
            MonstersCaptured[MonsterIndex].transform.SetParent(null);
            transform.SetParent(MonstersCaptured[MonsterIndex].transform);
            for (var i = 0;i < MonstersCaptured.Count();i++)
            {
                MonstersCaptured[i].gameObject.SetActive(i == MonsterIndex);
                if (i != MonsterIndex) MonstersCaptured[i].transform.SetParent(transform);
            }
            MonstersCaptured[MonsterIndex].StartStaminaRegen();
            UpdateExperienceUi();
        } else 
        {
            Utils.Util.StaminaGauge.SetActive(false);
            Utils.Util.EnduranceGauge.SetActive(false);
            Utils.Util.ExperienceGauge.SetActive(false);
            gameObject.SetActive(true);
            transform.SetParent(null);
            foreach(var monster in MonstersCaptured)
            {
                monster.gameObject.SetActive(false);
                monster.transform.SetParent(transform);
            }
        }
    }

    public void RemoveMonster(Monster m)
    {
        if (CurrentMonster == m)
        {
            ChangeMonster(-1);
        }
        MonstersCaptured.Remove(m);
        _updateMonsterUi();
    }

    private void _selectMonster()
    {
        if (_lastMonsterSwitch.AddMilliseconds(_monsterSwitchMillis) > DateTime.Now) return;
        if (Input.GetKeyDown(KeyCode.Q) && MonstersCaptured.Count() > 0)
        {
            _lastMonsterIndex = MonsterIndex;
            _lastMonsterSwitch = DateTime.Now;
            MonsterIndex--;
            if (MonsterIndex < -1) MonsterIndex = MonstersCaptured.Count() - 1;
        }
        if (Input.GetKeyDown(KeyCode.E) && MonstersCaptured.Count() > 0)
        {
            _lastMonsterIndex = MonsterIndex;
            _lastMonsterSwitch = DateTime.Now;
            MonsterIndex++;
            if (MonsterIndex > MonstersCaptured.Count() - 1)
            {
                MonsterIndex = -1;
            }
        }
        if (_lastMonsterIndex != MonsterIndex)
        {
            _lastMonsterIndex = MonsterIndex;
            ChangeMonster(MonsterIndex);
        }
    }

    public void CaptureMonster(GameObject monster)
    {
        monster.GetComponent<Monster>().Captured = true;
        monster.SetActive(false);
        MonstersCaptured.Add(monster.GetComponent<Monster>());
        monster.transform.SetParent(transform);
        monster.transform.localPosition = Vector3.zero;
        _updateMonsterUi();
    }

    #endregion

    #region Items

    private void _selectItem()
    {
        var mouseScroll = Input.mouseScrollDelta.y;
        if  (mouseScroll > 0)
        {
            ItemUseIndex++;
            if (ItemUseIndex == Items.Count)
            {
                ItemUseIndex = 0;
            }
        } else if (mouseScroll < 0) {
            ItemUseIndex--;
            if (ItemUseIndex < 0)
            {
                ItemUseIndex = Items.Count - 1;
            }
        }
    }

    private void _useItem()
    {
        if (_lastItemUsed.AddMilliseconds(_itemUseMillis) > DateTime.Now) return;
        if (!Items.Any()) return;
        _lastItemUsed = DateTime.Now;
        switch (Items[ItemUseIndex])
        {
            case Utils.Item.Capture_Ball:
                var ball = Instantiate(Utils.Util.CaptureBall);
                ball.transform.position = transform.position + (mouseDirection * _ballStartDistance_sett);
                ball.GetComponent<Rigidbody2D>().AddForce(mouseDirection * _ballThrowForce);
                break;
        }
        var currentType = Items[ItemUseIndex];
        Items.Remove(Items[ItemUseIndex]);
        var itemFound = false;
        for (var i = 0; i < Items.Count();i++)
        {
            if (Items[i] == currentType)
            {
                ItemUseIndex = i;
                itemFound = true;
                break;
            }
        }
        if (!itemFound)
        {
            ItemUseIndex = -1;
        }
        _updateItemText();
    }

    #endregion

    #region Input

    private void _getZoom()
    {
        var zoom = Input.mouseScrollDelta.y;
        if (zoom > 0 || zoom < 0)
        {
            _zoomedOut = !_zoomedOut;
            GameObject.FindWithTag("MainCamera").GetComponent<Camera>().orthographicSize = _zoomedOut ? 10 : 5;
        }
    }

    private void _getMovement()
    {
        var holdingShift = Input.GetKey(KeyCode.LeftShift);
        if (MonsterIndex == -1) holdingShift = false;
        var inputHorizontal = Input.GetAxis("Horizontal");
        var inputVertical = Input.GetAxis("Vertical");
        var moveHorizontal = inputHorizontal * _moveForce_sett;
        var moveVertical = inputVertical * _moveForce_sett;
        var rigidBody = _rb;

        if (Mathf.Abs(rigidBody.velocity.x) > (holdingShift ? _maxSpeet_sett * 2 : _maxSpeet_sett))
        {
            moveHorizontal = 0;
        }
        if (Mathf.Abs(rigidBody.velocity.y) > (holdingShift ? _maxSpeet_sett * 2 : _maxSpeet_sett))
        {
            moveVertical = 0;
        }
        
        if (moveHorizontal == 0 && Mathf.Abs(rigidBody.velocity.x) > 0)
        {
            moveHorizontal = _idleStopForce_sett * (rigidBody.velocity.x > 0 ? -1 : 1);
        }
        if (moveVertical == 0 && Mathf.Abs(rigidBody.velocity.y) > 0)
        {
            moveVertical = _idleStopForce_sett * (rigidBody.velocity.y > 0 ? -1 : 1);
        }

        if ((inputHorizontal < 0 && rigidBody.velocity.x > 0) || (inputHorizontal > 0 && rigidBody.velocity.x < 0))
        {
            moveHorizontal *= _stopForce_sett;
        }

        if ((inputVertical < 0 && rigidBody.velocity.y > 0) || (inputVertical > 0 && rigidBody.velocity.y < 0))
        {
            moveVertical *= _stopForce_sett;
        }

        rigidBody.AddForce(new Vector2(moveHorizontal, moveVertical));

        if (inputHorizontal == 0 && Mathf.Abs(rigidBody.velocity.x) > 0 && Mathf.Abs(rigidBody.velocity.x) < .1f)
        {
            rigidBody.velocity = new Vector2(0, rigidBody.velocity.y);
        }

        if (inputVertical == 0 && Mathf.Abs(rigidBody.velocity.y) > 0 && Mathf.Abs(rigidBody.velocity.y) < .1f)
        {
            rigidBody.velocity = new Vector2(rigidBody.velocity.x, 0);
        }
    }

    private void _useMove()
    {
        if (!MonstersCaptured.Any() || MonsterIndex == -1) return;
        var leftClick = Input.GetKeyDown(KeyCode.Mouse0);
        var rightClick = Input.GetKeyDown(KeyCode.Mouse1);
        if (leftClick)
        {
            MonstersCaptured[MonsterIndex].UseMove(Input.GetKey(KeyCode.R) ? 2 : 0, mouseDirection);
            UpdateStaminaUi();
        }
        if (rightClick)
        {
            MonstersCaptured[MonsterIndex].UseMove(Input.GetKey(KeyCode.R) ? 3 : 1, mouseDirection);
            UpdateStaminaUi();
        }
    }

    // Open/Close info pane
    private void _checkForInfo()
    {
        if (CurrentMonster == null) return;
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Utils.Util.StatsBoard.SetActive(true);
            Utils.Util.MoveBoard.SetActive(true);
            _updateMonsterInfoUi();
        }
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            Utils.Util.StatsBoard.SetActive(false);
            Utils.Util.MoveBoard.SetActive(false);
        }
    }

    #endregion
}

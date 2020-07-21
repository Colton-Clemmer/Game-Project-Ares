using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

public class Player : MonoBehaviour
{
    public List<Monster> MonstersCaptured;
    public List<Utils.Item> Items;
    public int ItemUseIndex;
    public int MonsterIndex = -1;
    public Monster CurrentMonster
    { get { return MonsterIndex == -1 ? null : MonstersCaptured[MonsterIndex]; } }

    private Rigidbody2D _rb
    { get { return CurrentMonster == null ? GetComponent<Rigidbody2D>() : CurrentMonster.GetComponent<Rigidbody2D>(); } }

    [SerializeField] private float _stopForce_sett = 10;
    [SerializeField] private float _moveForce_sett = 5;
    [SerializeField] private float _maxSpeet_sett = 5;
    [SerializeField] private float _ballStartDistance_sett = 2;
    [SerializeField] private float _ballThrowForce = 10;
    [SerializeField] private float _monsterSwitchMillis = 500;
    private DateTime _lastMonsterSwitch;

    [SerializeField] private float _itemUseMillis = 500;
    private DateTime _lastItemUsed;

    private Vector3 mouseDirection
    { get { return (transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition)).normalized * -1; } }
 
    void FixedUpdate()
    {
        UpdateFn();
    }

    public void UpdateFn()
    {
        Utils.Camera.transform.position = transform.position;
        _getMovement();
        _selectMonster();
        _selectItem();
        _useMove();
        if (Input.GetKeyDown(KeyCode.Mouse0) && MonsterIndex == -1)
        {
            _useItem();
        }
    }

    private void _useMove()
    {
        if (!MonstersCaptured.Any() || MonsterIndex == -1) return;
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            MonstersCaptured[MonsterIndex].UseMove(Input.GetKeyDown(KeyCode.R) ? 0 : 2, mouseDirection);
        } else if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            MonstersCaptured[MonsterIndex].UseMove(Input.GetKeyDown(KeyCode.R) ? 1 : 3, mouseDirection);
        }
    }

    private void _selectMonster()
    {
        if (_lastMonsterSwitch.AddMilliseconds(_monsterSwitchMillis) > DateTime.Now) return;
        if (Input.GetKeyDown(KeyCode.Q) && MonstersCaptured.Count() > 0)
        {
            _lastMonsterSwitch = DateTime.Now;
            MonsterIndex--;
            if (MonsterIndex < -1) MonsterIndex = MonstersCaptured.Count() - 1;
        }
        if (Input.GetKeyDown(KeyCode.E) && MonstersCaptured.Count() > 0)
        {
            _lastMonsterSwitch = DateTime.Now;
            MonsterIndex++;
            if (MonsterIndex > MonstersCaptured.Count() - 1)
            {
                MonsterIndex = -1;
            }
        }
        if (MonsterIndex >= 0)
        {
            gameObject.SetActive(false);
            MonstersCaptured[MonsterIndex].transform.SetParent(null);
            transform.SetParent(MonstersCaptured[MonsterIndex].transform);
            for (var i = 0;i < MonstersCaptured.Count();i++)
            {
                MonstersCaptured[i].gameObject.SetActive(i == MonsterIndex);
                if (i != MonsterIndex) MonstersCaptured[i].transform.SetParent(transform);
            }
        } else 
        {
            gameObject.SetActive(true);
            transform.SetParent(null);
            foreach(var monster in MonstersCaptured)
            {
                monster.gameObject.SetActive(false);
                monster.transform.SetParent(transform);
            }
        }
    }

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
        Items.Remove(Items[ItemUseIndex]);
    }

    private void _getMovement()
    {
        var inputHorizontal = Input.GetAxis("Horizontal");
        var inputVertical = Input.GetAxis("Vertical");
        var moveHorizontal = inputHorizontal * _moveForce_sett;
        var moveVertical = inputVertical * _moveForce_sett;
        var rigidBody = _rb;

        if (Mathf.Abs(rigidBody.velocity.x) > _maxSpeet_sett)
        {
            moveHorizontal = 0;
        }
        if (Mathf.Abs(rigidBody.velocity.y) > _maxSpeet_sett)
        {
            moveVertical = 0;
        }
        
        if (moveHorizontal == 0 && Mathf.Abs(rigidBody.velocity.x) > 0)
        {
            moveHorizontal = _stopForce_sett * (rigidBody.velocity.x > 0 ? -1 : 1);
        }
        if (moveVertical == 0 && Mathf.Abs(rigidBody.velocity.y) > 0)
        {
            moveVertical = _stopForce_sett * (rigidBody.velocity.y > 0 ? -1 : 1);
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
}

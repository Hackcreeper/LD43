using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Arena : MonoBehaviour
{
    [SerializeField]
    private GameObject[] _fields;

    [SerializeField]
    private int _fieldWidth;

    [SerializeField]
    private int _fieldHeight;

    [SerializeField]
    private Text _fightText;

    private Human[,] _board;

    private List<Human> _playerUnits = new List<Human>();

    private bool _yourTurn;

    public void StartFight(string[] enemies)
    {
        _playerUnits = new List<Human>();
        _board = new Human[_fieldWidth, _fieldHeight];

        _fightText.gameObject.SetActive(true);

        SpawnPlayerUnits();
        SpawnEnemyUnits(enemies);

        _yourTurn = Random.Range(0, 100) <= 50;
        _fightText.text = _yourTurn ? "Your turn!" : "Enemies turn!";

        StartRound();
    }

    private void SpawnPlayerUnits()
    {
        int playerMin = 0;
        int playerMax = _fieldWidth / 3;

        foreach(var human in Game.Instance.GetHumans())
        {
            var x = 0;
            var y = 0;

            do
            {
                x = Random.Range(playerMin, playerMax);
                y = Random.Range(0, _fieldHeight);
            } while (_board[x,y] != null);

            var fighter = Instantiate(Resources.Load<GameObject>(human.GetPrefabName()));
            fighter.GetComponent<NavMeshAgent>().enabled = false;
            fighter.transform.position = FindField(x+1, y+1).position;

            _board[x, y] = fighter.GetComponent<Human>();
            _playerUnits.Add(fighter.GetComponent<Human>());
        }
    }

    private void SpawnEnemyUnits(string[] enemies)
    {
        int enemyMin = _fieldWidth - (_fieldWidth / 3);
        int enemyMax = _fieldWidth - 1;

        foreach(var enemy in enemies)
        {
            var x = 0;
            var y = 0;

            do
            {
                x = Random.Range(enemyMin, enemyMax);
                y = Random.Range(0, _fieldHeight);
            } while (_board[x, y] != null);

            var fighter = Instantiate(Resources.Load<GameObject>($"Enemies/{enemy}"));
            fighter.transform.position = FindField(x + 1, y + 1).position;
            fighter.transform.rotation = Quaternion.Euler(0, 180, 0);
            _board[x, y] = fighter.GetComponent<Human>();
        }
    }

    private Transform FindField(int x, int y)
    {
        foreach(var field in _fields)
        {
            if (field.name == $"Field_{x}_{y}")
            {
                return field.transform;
            }
        }

        return null;
    }

    public bool IsPlayerTurn() => _yourTurn;

    private void StartRound()
    {
        if (_yourTurn)
        {
            foreach(var unit in _playerUnits)
            {
                var info = Instantiate(Resources.Load<GameObject>("Info"));
                info.transform.position = unit.transform.position + new Vector3(0, 3.6f, 0);
            }
        }
    }
}
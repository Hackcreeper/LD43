using UnityEngine;
using UnityEngine.AI;

public class Arena : MonoBehaviour
{
    [SerializeField]
    private GameObject[] _fields;

    [SerializeField]
    private int _fieldWidth;

    [SerializeField]
    private int _fieldHeight;

    [SerializeField]
    private GameObject _fightText;

    private Human[,] _board;

    public void StartFight(string[] enemies)
    {
        _board = new Human[_fieldWidth, _fieldHeight];

        _fightText.SetActive(true);

        SpawnPlayerUnits();
        SpawnEnemyUnits(enemies);
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
}
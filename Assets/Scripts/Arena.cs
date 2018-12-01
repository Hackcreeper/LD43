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

    [SerializeField]
    private Material _defaultFieldMaterial;

    [SerializeField]
    private Material _moveableFieldMaterial;

    [SerializeField]
    private GameObject _nextButton;

    private Human[,] _board;

    private List<Human> _playerUnits = new List<Human>();

    private List<GameObject> _infoSprites = new List<GameObject>();

    private bool _yourTurn;

    private BattleAction _currentAction = BattleAction.None;

    private Human _actionTarget;

    private bool _isPanelOpen;

    public void StartFight(string[] enemies)
    {
        _playerUnits = new List<Human>();
        _infoSprites = new List<GameObject>();
        _nextButton.SetActive(true);
        _board = new Human[_fieldWidth, _fieldHeight];

        _fightText.gameObject.SetActive(true);

        SpawnPlayerUnits();
        SpawnEnemyUnits(enemies);

        _yourTurn = Random.Range(0, 100) <= 5 || true;

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

            var comp = fighter.GetComponent<Human>();
            comp.SetBoardPosition(x, y);

            _board[x, y] = comp;
            _playerUnits.Add(comp);
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

            var comp = fighter.GetComponent<Human>();
            comp.SetBoardPosition(x, y);

            _board[x, y] = comp;
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
        _fightText.text = _yourTurn ? "Your turn!" : "Enemies turn!";

        if (_yourTurn)
        {
            SpawnInfos();
        }
    }

    public bool IsActionRunning() => _currentAction != BattleAction.None;

    public void StartAction(Human target, BattleAction action)
    {
        _currentAction = action;
        _actionTarget = target;

        _isPanelOpen = false;

        _infoSprites.ForEach(sprite => Destroy(sprite));
        _infoSprites.Clear();

        for (var x = target.GetX() - 1; x <= target.GetX() + 1; x++)
        {
            for (var y = target.GetY() - 1; y <= target.GetY() + 1; y++)
            {
                if (FindField(x+1, y+1) && _board[x,y] == null)
                {
                    var field = FindField(x + 1, y + 1);
                    field.GetComponent<MeshRenderer>().material = _moveableFieldMaterial;
                    field.GetComponent<Field>().Activate();
                }
            }
        }

        // and now... 
        // 1.) hide all "info" marks √
        // 3.) hide the panel √
        // 3.) no new players should be selectable √
        // 4.) end the action sequence when pressing "esc" √
        // 5.) Mark all fields which are moveable √
        // 6.) Handle click on field -> move unit √
        // 7.) Reset the field colors √
        // 8.) Remove info mark for this specific unit √
        // 9.) Show all other info marks √
        // 10.) End the action √
        // 11.) Switch rounds √
    }

    private void Update()
    {
        if (IsActionRunning() && Input.GetKeyDown(KeyCode.Escape))
        {
            EndAction();
        }
    }

    public void ClickedField(Field target)
    {
        _actionTarget.transform.position = target.transform.position;
        _actionTarget.ExecutedAction();

        _board[_actionTarget.GetX(), _actionTarget.GetY()] = null;

        var coordinates = target.name.Replace("Field_", "").Split('_');
        _board[int.Parse(coordinates[0])-1, int.Parse(coordinates[1])-1] = _actionTarget;

        EndAction();
    }

    private void EndAction()
    {
        foreach(var field in _fields)
        {
            field.GetComponent<MeshRenderer>().material = _defaultFieldMaterial;
            field.GetComponent<Field>().Reset();
        }

        SpawnInfos();

        _currentAction = BattleAction.None;
        _actionTarget = null;
    }

    private void SpawnInfos()
    {
        _playerUnits.ForEach(human =>
        {
            if (!human.CanRunAction())
            {
                return;
            }

            var info = Instantiate(Resources.Load<GameObject>("Info"));
            info.transform.position = human.transform.position + new Vector3(0, 3.6f, 0);

            _infoSprites.Add(info);
        });
    }

    public void OpenedPanel() => _isPanelOpen = true;

    public bool IsPanelOpen() => _isPanelOpen;

    public void NextTurn()
    {
        if (!_yourTurn)
        {
            return;
        }

        _yourTurn = false;
        _isPanelOpen = false;

        _infoSprites.ForEach(sprite => Destroy(sprite));
        _infoSprites.Clear();

        foreach (var field in _fields)
        {
            field.GetComponent<MeshRenderer>().material = _defaultFieldMaterial;
            field.GetComponent<Field>().Reset();
        }

        _currentAction = BattleAction.None;
        _actionTarget = null;

        StartRound();
    }
}
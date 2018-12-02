using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Arena : MonoBehaviour
{
    public static Arena Instance { get; private set; }

    private List<Unit> _playerUnits;
    private List<GameObject> _infoSprites = new List<GameObject>();

    private List<Unit> _enemyUnits = new List<Unit>();
    private List<Unit> _enemyUnitsTodo = new List<Unit>();

    private Stage _activeStage;

    [SerializeField]
    private string[] _classes;

    [SerializeField]
    private GameObject[] _fields;

    [SerializeField]
    private Transform _canvas;

    [SerializeField]
    private GameObject _nextButton;

    [SerializeField]
    private Text _turnText;

    public const int FIELD_WIDTH = 9;
    public const int FIELD_HEIGHT = 6;

    private FightAction _currentAction = FightAction.None;
    private Unit _actionUnit;

    private GameObject _actionPanel;

    private string[][] _stages;

    private bool _playersTurn = true;

    public void Start()
    {
        Instance = this;
        _stages = new string[1][];
        _stages[0] = new string[] { "Swordsman", "Archer", "Swordsman" };

        InitPlayerUnits();
        LoadStage(new Stage(this, _stages[0]));
    }

    private void InitPlayerUnits()
    {
        _playerUnits = new List<Unit>();

        for (int i = 0; i < 5; i++)
        {
            var randomClass = _classes[Random.Range(0, _classes.Length)];
            var unit = Instantiate(Resources.Load<GameObject>(randomClass));

            var component = unit.GetComponent<Unit>();
            _playerUnits.Add(component);
            component.SetArena(this);
        }
    }

    private void LoadStage(Stage stage)
    {
        _activeStage = stage;
        _activeStage.Start();

        StartTurn();
    }

    public Unit[] GetPlayerUnits() => _playerUnits.ToArray();

    public Transform FindField(int x, int y)
    {
        foreach (var field in _fields)
        {
            if (field.name == $"Field_{x}_{y}")
            {
                return field.transform;
            }
        }

        return null;
    }

    public FightAction GetCurrentAction() => _currentAction;

    public void OpenActionPanel(Unit unit)
    {
        _actionPanel = Instantiate(Resources.Load<GameObject>("ActionsPanel"));
        _actionPanel.transform.SetParent(_canvas);
        _actionPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);

        _actionPanel.GetComponent<ActionPanel>().SetUnit(unit);
        _nextButton.SetActive(false);
    }

    public bool IsPanelOpen() => _actionPanel != null;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && IsPanelOpen())
        {
            Destroy(_actionPanel);
            _nextButton.SetActive(true);
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && _currentAction != FightAction.None)
        {
            EndAction();
        }
    }

    private void ShowInfos()
    {
        _infoSprites = new List<GameObject>();

        _playerUnits.ForEach(unit =>
        {
            if (!unit.CanMakeAction())
            {
                return;
            }

            var info = Instantiate(Resources.Load<GameObject>("Info"));
            info.transform.position = unit.transform.position + new Vector3(0, 3.6f, 0);
            _infoSprites.Add(info);
        });
    }

    public void StartMoveAction(Unit unit)
    {
        _currentAction = FightAction.Move;
        _actionUnit = unit;

        StartAction();

        if (unit.IsEnemy())
        {
            return;
        }

        for (var x = unit.GetX() - 1; x <= unit.GetX() + 1; x++)
        {
            for (var y = unit.GetY() - 1; y <= unit.GetY() + 1; y++)
            {
                if (FindField(x + 1, y + 1) && _activeStage.GetBoard()[x, y] == null)
                {
                    var field = FindField(x + 1, y + 1);
                    field.GetComponent<Field>().Activate(FightAction.Move);
                }
            }
        }
    }

    private void StartAction()
    {
        _nextButton.SetActive(false);

        _infoSprites.ForEach(sprite => Destroy(sprite));
        _infoSprites.Clear();

        if (_actionPanel)
        {
            Destroy(_actionPanel);
        }
    }

    private void EndAction()
    {
        if (_playersTurn)
        {
            _nextButton.SetActive(true);
        }

        foreach (var field in _fields)
        {
            field.GetComponent<Field>().Deactivate();
        }

        if (_playersTurn)
        {
            ShowInfos();
        }

        var unit = _actionUnit;

        _currentAction = FightAction.None;
        _actionUnit = null;

        if (unit.IsEnemy())
        {
            HandleEnemyTurn();
        }
    }

    public void ClickedField(Field field)
    {
        _actionUnit.MoveTo(field.transform.position);

        _activeStage.Set(_actionUnit.GetX(), _actionUnit.GetY(), null);

        var coordinates = field.name.Replace("Field_", "").Split('_');
        var newX = int.Parse(coordinates[0]) - 1;
        var newY = int.Parse(coordinates[1]) - 1;

        _activeStage.Set(newX, newY, _actionUnit);

        _actionUnit.SetBoardPosition(newX, newY);
        _actionUnit.ActionMade();
    }

    public void DestinationReached() => EndAction();

    public void NextTurn()
    {
        _playersTurn = !_playersTurn;

        StartTurn();
    }

    private void StartTurn()
    {
        _nextButton.SetActive(false);
        _turnText.text = _playersTurn ? "Your turn" : "Enemies turn";

        _infoSprites.ForEach(sprite => Destroy(sprite));
        _infoSprites.Clear();

        if (_actionPanel)
        {
            Destroy(_actionPanel);
        }

        if (_playersTurn)
        {
            _playerUnits.ForEach(unit => unit.Reset());
            _nextButton.SetActive(true);
            ShowInfos();

            return;
        }

        _enemyUnitsTodo.AddRange(_enemyUnits);
        HandleEnemyTurn();
    }

    private void HandleEnemyTurn()
    {
        if (_enemyUnitsTodo.Count == 0)
        {
            NextTurn();
            return;
        }

        var enemy = _enemyUnitsTodo[Random.Range(0, _enemyUnitsTodo.Count-1)];

        StartMoveAction(enemy);

        List<Field> _possibleFields = new List<Field>();
        for (var x = enemy.GetX() - 1; x <= enemy.GetX() + 1; x++)
        {
            for (var y = enemy.GetY() - 1; y <= enemy.GetY() + 1; y++)
            {
                if (FindField(x + 1, y + 1) && _activeStage.GetBoard()[x, y] == null)
                {
                    _possibleFields.Add(FindField(x + 1, y + 1).GetComponent<Field>());
                }
            }
        }

        var targetField = _possibleFields[Random.Range(0, _possibleFields.Count - 1)];
        enemy.MoveTo(targetField.transform.position);

        _activeStage.Set(enemy.GetX(), enemy.GetY(), null);

        var coordinates = targetField.name.Replace("Field_", "").Split('_');
        var newX = int.Parse(coordinates[0]) - 1;
        var newY = int.Parse(coordinates[1]) - 1;

        _activeStage.Set(newX, newY, enemy);

        enemy.SetBoardPosition(newX, newY);
        _enemyUnitsTodo.Remove(enemy);
    }

    public bool IsPlayersTurn() => _playersTurn;

    public void AddEnemy(Unit enemy)
    {
        _enemyUnits.Add(enemy);
    }
}

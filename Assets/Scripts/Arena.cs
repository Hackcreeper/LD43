using System.Collections.Generic;
using UnityEngine;

public class Arena : MonoBehaviour 
{
    public static Arena Instance { get; private set; }

    private List<Unit> _playerUnits;
    private List<GameObject> _infoSprites;

    private Stage _activeStage;

    [SerializeField]
    private string[] _classes;

    [SerializeField]
    private GameObject[] _fields;

    [SerializeField]
    private Transform _canvas;

    public const int FIELD_WIDTH = 9;
    public const int FIELD_HEIGHT = 6;

    private FightAction _currentAction = FightAction.None;
    private Unit _actionUnit;

    private GameObject _actionPanel;

    public void Start()
    {
        Instance = this;

        InitPlayerUnits();
        LoadStage(new Stage(this));
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

        ShowInfos();
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
    }

    public bool IsPanelOpen() => _actionPanel != null;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && IsPanelOpen())
        {
            Destroy(_actionPanel);
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
        _infoSprites.ForEach(sprite => Destroy(sprite));
        _infoSprites.Clear();

        if (_actionPanel)
        {
            Destroy(_actionPanel);
        }
    }

    private void EndAction()
    {
        foreach(var field in _fields)
        {
            field.GetComponent<Field>().Deactivate();
        }

        ShowInfos();

        _currentAction = FightAction.None;
        _actionUnit = null;
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
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.PostProcessing;
using UnityEngine.SceneManagement;
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

    [SerializeField]
    private PostProcessingBehaviour _cameraShader;

    [SerializeField]
    private PostProcessingProfile[] _shaderProfiles;

    [SerializeField]
    private GameObject _sacrificeCanvas;

    [SerializeField]
    private Transform _sacrificeCamera;

    [SerializeField]
    private AudioClip _healSound;

    [SerializeField]
    private AudioClip _shootSound;

    [SerializeField]
    private AudioClip _swordSound;

    [SerializeField]
    private AudioClip _smashSound;

    [SerializeField]
    private GameObject _escapeText;

    public const int FIELD_WIDTH = 9;
    public const int FIELD_HEIGHT = 6;

    private FightAction _currentAction = FightAction.None;
    private Unit _actionUnit;

    private GameObject _actionPanel;

    private string[][] _stages;

    private bool _playersTurn = true;

    private bool _arrowShouldSpawn = false;
    private float _arrowTimer = 0f;
    private Field _clickedField;
    private int _arrowSpawnAmount = 0;

    private float? _endActionTimer = null;

    public void Start()
    {
        Instance = this;
        _stages = new string[4][];
        _stages[0] = new string[] { "Healer", "Swordsman", "Archer" };
        _stages[1] = new string[] { "Archer", "Archer", "Swordsman", "Swordsman" };
        _stages[2] = new string[] { "Healer", "Swordsman", "Swordsman" };
        _stages[3] = new string[] { "Boss", "Archer" };

        InitPlayerUnits();
        LoadStage(new Stage(this, _stages[0], 0));
    }

    private void InitPlayerUnits()
    {
        _playerUnits = new List<Unit>();

        var selectedClasses = new string[]
        {
            "Swordsman", "Archer", "Healer", "Archer", "Healer"
        };

        for (int i = 0; i < selectedClasses.Length; i++)
        {
            var randomClass = selectedClasses[i];
            var unit = Instantiate(Resources.Load<GameObject>(randomClass));

            var component = unit.GetComponent<Unit>();
            _playerUnits.Add(component);
            component.SetArena(this);
            component.RegisterOnDie((deadUnit) =>
            {
                _infoSprites.ForEach(info => Destroy(info));
                _infoSprites.Clear();

                if (_playersTurn)
                {
                    ShowInfos();
                }

                _playerUnits.Remove(deadUnit);

                if (_playerUnits.Count <= 0)
                {
                    SceneManager.LoadScene("GameOver");
                }
            });
        }

        _playerUnits[0].UnlockSkill();
    }

    private void LoadStage(Stage stage)
    {
        _activeStage = stage;
        _activeStage.Start();

        _cameraShader.profile = _shaderProfiles[_activeStage.GetIndex()];

        foreach (var field in _fields)
        {
            field.GetComponent<Field>().SetIndex(stage.GetIndex());
        }

        if (Music.Instance)
        {
            Music.Instance.SetStage(stage.GetIndex());
        }

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

        _escapeText.SetActive(true);
    }

    public bool IsPanelOpen() => _actionPanel != null;

    private void Update()
    {
        if (_endActionTimer.HasValue)
        {
            _endActionTimer -= Time.deltaTime;

            if (_endActionTimer <= 0)
            {
                _endActionTimer = null;
                EndAction();
            }
        }

        if (_arrowShouldSpawn)
        {
            _arrowTimer -= Time.deltaTime;
            if (_arrowTimer <= 0f)
            {
                var coordinates = _clickedField.name.Replace("Field_", "").Split('_');
                var newX = int.Parse(coordinates[0]) - 1;
                var newY = int.Parse(coordinates[1]) - 1;

                var unit = _actionUnit;
                var shouldEnd = _arrowSpawnAmount == 1;

                GetComponent<AudioSource>().clip = _shootSound;
                GetComponent<AudioSource>().Play();

                var arrow = Instantiate(Resources.Load<GameObject>("Arrow"));
                arrow.transform.position = _actionUnit.transform.position + new Vector3(0, 1.3f, 0);
                arrow.transform.rotation = _actionUnit.transform.rotation;
                arrow.transform.Rotate(0, 180, 0);
                arrow.transform.Translate(0, 0, -1);
                arrow.GetComponent<Arrow>().SetDestination(_clickedField.transform.position);
                arrow.GetComponent<Arrow>().RegisterOnHit(() =>
                {
                    if (_activeStage.Get(newX, newY))
                    {
                        _activeStage.Get(newX, newY).SubHealth(15, false);
                    }

                    unit.ActionMade();

                    if (shouldEnd)
                    {
                        EndAction();
                    }
                });

                _arrowSpawnAmount--;
                if (_arrowSpawnAmount > 0)
                {
                    _arrowTimer = 0.1f;
                    return;
                }

                _arrowShouldSpawn = false;
                _clickedField = null;
            }
        }

        if ((Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Backspace)) && IsPanelOpen())
        {
            Destroy(_actionPanel);
            _nextButton.SetActive(true);
            _escapeText.SetActive(false);
        }
        else if ((Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Backspace)) && _currentAction != FightAction.None)
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

        _escapeText.SetActive(true);

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

    public void StartAttackAction(Unit unit)
    {
        _currentAction = FightAction.Attack;
        _actionUnit = unit;

        StartAction();

        if (unit.IsEnemy())
        {
            return;
        }

        _escapeText.SetActive(true);

        if (unit.GetClass() == Class.Swordsman)
        {
            for (var x = unit.GetX() - 1; x <= unit.GetX() + 1; x++)
            {
                for (var y = unit.GetY() - 1; y <= unit.GetY() + 1; y++)
                {
                    if (FindField(x + 1, y + 1) && _activeStage.GetBoard()[x, y] != null && _activeStage.GetBoard()[x, y].IsEnemy())
                    {
                        var field = FindField(x + 1, y + 1);
                        field.GetComponent<Field>().Activate(FightAction.Attack);
                    }
                }
            }

            return;
        }

        if (unit.GetClass() == Class.Archer)
        {
            const int range = 4;

            for (var x = unit.GetX() - range; x <= unit.GetX() + range; x++)
            {
                for (var y = unit.GetY() - range; y <= unit.GetY() + range; y++)
                {
                    if (FindField(x + 1, y + 1) && _activeStage.GetBoard()[x, y] != null && _activeStage.GetBoard()[x, y].IsEnemy())
                    {
                        var field = FindField(x + 1, y + 1);
                        field.GetComponent<Field>().Activate(FightAction.Attack);
                    }
                }
            }

            return;
        }
    }

    public void StartSmashAttackAction(Unit unit)
    {
        _currentAction = FightAction.SmashAttack;
        _actionUnit = unit;

        StartAction();

        if (unit.IsEnemy())
        {
            return;
        }

        _escapeText.SetActive(true);

        for (var x = unit.GetX() - 1; x <= unit.GetX() + 1; x++)
        {
            for (var y = unit.GetY() - 1; y <= unit.GetY() + 1; y++)
            {
                if (FindField(x + 1, y + 1) && _activeStage.GetBoard()[x, y] != null && _activeStage.GetBoard()[x, y].IsEnemy())
                {
                    if (x != unit.GetX() && y != unit.GetY())
                    {
                        continue;
                    }

                    var field = FindField(x + 1, y + 1);
                    field.GetComponent<Field>().Activate(FightAction.Attack);
                }
            }
        }
    }

    public void StartRapidFireAction(Unit unit)
    {
        _currentAction = FightAction.RapidFire;
        _actionUnit = unit;

        StartAction();

        if (unit.IsEnemy())
        {
            return;
        }

        _escapeText.SetActive(true);

        const int range = 5;

        for (var x = unit.GetX() - range; x <= unit.GetX() + range; x++)
        {
            for (var y = unit.GetY() - range; y <= unit.GetY() + range; y++)
            {
                if (FindField(x + 1, y + 1) && _activeStage.GetBoard()[x, y] != null && _activeStage.GetBoard()[x, y].IsEnemy())
                {
                    var field = FindField(x + 1, y + 1);
                    field.GetComponent<Field>().Activate(FightAction.RapidFire);
                }
            }
        }
    }

    public void StartHealAction(Unit unit)
    {
        _currentAction = FightAction.Heal;
        _actionUnit = unit;

        StartAction();

        if (unit.IsEnemy())
        {
            return;
        }

        _escapeText.SetActive(true);

        const int range = 4;

        for (var x = unit.GetX() - range; x <= unit.GetX() + range; x++)
        {
            for (var y = unit.GetY() - range; y <= unit.GetY() + range; y++)
            {
                if (FindField(x + 1, y + 1) && _activeStage.GetBoard()[x, y] != null && !_activeStage.GetBoard()[x, y].IsEnemy())
                {
                    var field = FindField(x + 1, y + 1);
                    field.GetComponent<Field>().Activate(FightAction.Heal);
                }
            }
        }

        _playerUnits.ForEach(player =>
        {
            player.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        });
    }

    public void StartHealAllAction(Unit unit)
    {
        _currentAction = FightAction.HealAll;
        _actionUnit = unit;

        StartAction();

        GetComponent<AudioSource>().clip = _healSound;
        GetComponent<AudioSource>().Play();

        unit.GetComponentInChildren<Animator>().Play("Healer_HealAll");

        _playerUnits.ForEach(player =>
        {
            player.SubHealth(-25, true);
        });

        _actionUnit.SkillUsed();
        _actionUnit.ActionMade();

        EndAction();
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
        _escapeText.SetActive(false);

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

        _playerUnits.ForEach(player =>
        {
            player.gameObject.layer = LayerMask.NameToLayer("Default");
        });

        var unit = _actionUnit;

        _currentAction = FightAction.None;
        _actionUnit = null;

        if (unit && unit.IsEnemy())
        {
            HandleEnemyTurn();
        }
    }

    public void ClickedField(Field field)
    {
        var coordinates = field.name.Replace("Field_", "").Split('_');
        var newX = int.Parse(coordinates[0]) - 1;
        var newY = int.Parse(coordinates[1]) - 1;

        if (_currentAction == FightAction.Move)
        {
            _actionUnit.MoveTo(field.transform.position);

            _activeStage.Set(_actionUnit.GetX(), _actionUnit.GetY(), null);
            _activeStage.Set(newX, newY, _actionUnit);

            _actionUnit.SetBoardPosition(newX, newY);
            _actionUnit.ActionMade();
            return;
        }

        if (_currentAction == FightAction.Attack)
        {
            if (_actionUnit.GetClass() == Class.Swordsman)
            {
                _actionUnit.MoveToHalfWay(field.transform.position);
                _activeStage.GetBoard()[newX, newY].SubHealth(25, true);

                return;
            }

            if (_actionUnit.GetClass() == Class.Archer)
            {
                _actionUnit.transform.LookAt(field.transform.position);

                _actionUnit.GetComponentInChildren<Animator>().Play("Archer_Shot");

                _arrowShouldSpawn = true;
                _arrowTimer = .60f;
                _clickedField = field;
                _arrowSpawnAmount = 1;

                return;
            }

            return;
        }

        if (_currentAction == FightAction.SmashAttack)
        {
            _actionUnit.transform.LookAt(field.transform.position);

            _actionUnit.GetComponentInChildren<Animator>().Play("Sword_Smash");
            _actionUnit.ActionMade();

            GetComponent<AudioSource>().clip = _smashSound;
            GetComponent<AudioSource>().Play();

            if (newX == _actionUnit.GetX() && newY > _actionUnit.GetY())
            {
                _activeStage.Get(_actionUnit.GetX() - 1, _actionUnit.GetY() + 1)?.SubHealth(40, true);
                _activeStage.Get(_actionUnit.GetX(), _actionUnit.GetY() + 1)?.SubHealth(40, true);
                _activeStage.Get(_actionUnit.GetX() + 1, _actionUnit.GetY() + 1)?.SubHealth(40, true);

                _activeStage.Get(_actionUnit.GetX() - 1, _actionUnit.GetY() + 2)?.SubHealth(40, true);
                _activeStage.Get(_actionUnit.GetX(), _actionUnit.GetY() + 2)?.SubHealth(40, true);
                _activeStage.Get(_actionUnit.GetX() + 1, _actionUnit.GetY() + 2)?.SubHealth(40, true);

            }
            else if (newX == _actionUnit.GetX() && newY < _actionUnit.GetY())
            {
                _activeStage.Get(_actionUnit.GetX() - 1, _actionUnit.GetY() - 1)?.SubHealth(40, true);
                _activeStage.Get(_actionUnit.GetX(), _actionUnit.GetY() - 1)?.SubHealth(40, true);
                _activeStage.Get(_actionUnit.GetX() + 1, _actionUnit.GetY() - 1)?.SubHealth(40, true);

                _activeStage.Get(_actionUnit.GetX() - 1, _actionUnit.GetY() - 2)?.SubHealth(40, true);
                _activeStage.Get(_actionUnit.GetX(), _actionUnit.GetY() - 2)?.SubHealth(40, true);
                _activeStage.Get(_actionUnit.GetX() + 1, _actionUnit.GetY() - 2)?.SubHealth(40, true);
            }
            else if (newY == _actionUnit.GetY() && newX > _actionUnit.GetX())
            {
                _activeStage.Get(_actionUnit.GetX() + 1, _actionUnit.GetY() - 1)?.SubHealth(40, true);
                _activeStage.Get(_actionUnit.GetX() + 1, _actionUnit.GetY())?.SubHealth(40, true);
                _activeStage.Get(_actionUnit.GetX() + 1, _actionUnit.GetY() + 1)?.SubHealth(40, true);

                _activeStage.Get(_actionUnit.GetX() + 2, _actionUnit.GetY() - 1)?.SubHealth(40, true);
                _activeStage.Get(_actionUnit.GetX() + 2, _actionUnit.GetY())?.SubHealth(40, true);
                _activeStage.Get(_actionUnit.GetX() + 2, _actionUnit.GetY() + 1)?.SubHealth(40, true);
            }
            else if (newY == _actionUnit.GetY() && newX < _actionUnit.GetX())
            {
                _activeStage.Get(_actionUnit.GetX() - 1, _actionUnit.GetY() - 1)?.SubHealth(40, true);
                _activeStage.Get(_actionUnit.GetX() - 1, _actionUnit.GetY())?.SubHealth(40, true);
                _activeStage.Get(_actionUnit.GetX() - 1, _actionUnit.GetY() + 1)?.SubHealth(40, true);

                _activeStage.Get(_actionUnit.GetX() - 2, _actionUnit.GetY() - 1)?.SubHealth(40, true);
                _activeStage.Get(_actionUnit.GetX() - 2, _actionUnit.GetY())?.SubHealth(40, true);
                _activeStage.Get(_actionUnit.GetX() - 2, _actionUnit.GetY() + 1)?.SubHealth(40, true);
            }

            _actionUnit.SkillUsed();

            EndAction();

            return;
        }

        if (_currentAction == FightAction.RapidFire)
        {
            _actionUnit.transform.LookAt(field.transform.position);

            _actionUnit.GetComponentInChildren<Animator>().Play("Archer_Shot");

            _actionUnit.SkillUsed();

            _arrowShouldSpawn = true;
            _arrowTimer = .60f;
            _clickedField = field;
            _arrowSpawnAmount = 4;

            return;
        }

        if (_currentAction == FightAction.Heal)
        {
            _actionUnit.transform.LookAt(field.transform.position);

            _actionUnit.GetComponentInChildren<Animator>().Play("Sword_Hit");
            GetComponent<AudioSource>().clip = _healSound;
            GetComponent<AudioSource>().Play();

            _activeStage.Get(newX, newY).SubHealth(-20, false);

            _actionUnit.ActionMade();
            EndAction();
        }
    }

    public void DestinationReached() => EndAction();

    public void HalfReached()
    {
        _actionUnit.GetComponentInChildren<Animator>().Play("Sword_Hit");
        _actionUnit.ActionMade();
    }

    public void NextTurn()
    {
        if (_playersTurn) { 
            _playerUnits.ForEach(unit => unit.TurnEnded());
        }

        _playersTurn = !_playersTurn;

        StartTurn();
    }

    private void StartTurn()
    {
        _escapeText.SetActive(false);
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

        var target = GetAttackTarget(enemy);
        if (target)
        {
            if (enemy.GetClass() == Class.Swordsman)
            {
                StartAttackAction(enemy);

                _actionUnit.MoveToHalfWay(target.transform.position);

                target.SubHealth(20, true);

                _enemyUnitsTodo.Remove(enemy);

                return;
            }

            if (enemy.GetClass() == Class.Archer)
            {
                StartAttackAction(enemy);
                _enemyUnitsTodo.Remove(enemy);

                _actionUnit.transform.LookAt(target.transform.position);

                _actionUnit.GetComponentInChildren<Animator>().Play("Archer_Shot");

                _arrowShouldSpawn = true;
                _arrowTimer = .60f;
                _clickedField = FindField(target.GetX() + 1, target.GetY() + 1).GetComponent<Field>();
                _arrowSpawnAmount = 1;

                return;
            }

            if (enemy.GetClass() == Class.Healer)
            {
                StartHealAction(enemy);
                _enemyUnitsTodo.Remove(enemy);

                _actionUnit.transform.LookAt(target.transform.position);

                _actionUnit.GetComponentInChildren<Animator>().Play("Sword_Hit");

                _activeStage.Get(target.GetX(), target.GetY()).SubHealth(-15, false);

                _endActionTimer = 2f;

                return;
            }
        }

        StartMoveAction(enemy);

        var lastDistance = float.MaxValue;
        Unit targetUnit = null;

        var lastHealth = int.MaxValue;

        if (enemy.GetClass() == Class.Healer)
        {
            foreach (var unit in _enemyUnits)
            {
                if (unit.GetHealth() < lastHealth)
                {
                    lastHealth = unit.GetHealth();
                    targetUnit = unit;
                }
            }
        }
        else
        {
            foreach (var unit in _playerUnits)
            {
                var distance = Vector3.Distance(enemy.transform.position, unit.transform.position);
                if (distance < lastDistance)
                {
                    lastDistance = distance;
                    targetUnit = unit;
                }
            }
        }

        var targetX = enemy.GetX();
        var targetY = enemy.GetY();

        if (targetUnit.GetX() < enemy.GetX())
        {
            targetX--;
        }
        else if (targetUnit.GetX() > enemy.GetX())
        {
            targetX++;
        }

        if (targetUnit.GetY() < enemy.GetY())
        {
            targetY--;
        }
        else if (targetUnit.GetY() > enemy.GetY())
        {
            targetY++;
        }

        if (_activeStage.Get(targetX, targetY))
        {
            _enemyUnitsTodo.Remove(enemy);
            EndAction();
            return;
        }

        enemy.MoveTo(FindField(targetX + 1, targetY + 1).transform.position);
        _activeStage.Set(enemy.GetX(), enemy.GetY(), null);

        var coordinates = FindField(targetX + 1, targetY + 1).name.Replace("Field_", "").Split('_');
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
        enemy.RegisterOnDie(unit =>
        {
            _enemyUnits.Remove(unit);

            if (_enemyUnits.Count <= 0)
            {
                if (_activeStage.GetIndex() == 3)
                {
                    SceneManager.LoadScene("Win");
                    return;
                }

                _sacrificeCanvas.SetActive(true);
                Camera.main.transform.position = _sacrificeCamera.position;
                _nextButton.SetActive(false);
                _turnText.gameObject.SetActive(false);

                _playersTurn = false;
                _sacrificeCanvas.GetComponent<Sacrifice>().GoOn();
            }
        });
    }

    private Unit GetAttackTarget(Unit source)
    {
        Unit target = null;
        int lastHealth = int.MaxValue;

        if (source.GetClass() == Class.Swordsman) { 
            for (var x = source.GetX() - 1; x <= source.GetX() + 1; x++)
            {
                for (var y = source.GetY() - 1; y <= source.GetY() + 1; y++)
                {
                    if (FindField(x + 1, y + 1) && _activeStage.GetBoard()[x, y] != null && !_activeStage.GetBoard()[x, y].IsEnemy())
                    {
                        if (_activeStage.Get(x, y).GetHealth() < lastHealth)
                        {
                            target = _activeStage.GetBoard()[x, y];
                            lastHealth = _activeStage.Get(x, y).GetHealth();
                        }
                    }
                }
            }
        }
        else if (source.GetClass() == Class.Archer)
        {
            const int range = 4;

            for (var x = source.GetX() - range; x <= source.GetX() + range; x++)
            {
                for (var y = source.GetY() - range; y <= source.GetY() + range; y++)
                {
                    if (FindField(x + 1, y + 1) && _activeStage.GetBoard()[x, y] != null && !_activeStage.GetBoard()[x, y].IsEnemy())
                    {
                        if (_activeStage.Get(x, y).GetHealth() < lastHealth)
                        {
                            target = _activeStage.GetBoard()[x, y];
                            lastHealth = _activeStage.Get(x, y).GetHealth();
                        }
                    }
                }
            }
        }
        else if (source.GetClass() == Class.Healer)
        {
            const int range = 4;

            for (var x = source.GetX() - range; x <= source.GetX() + range; x++)
            {
                for (var y = source.GetY() - range; y <= source.GetY() + range; y++)
                {
                    if (FindField(x + 1, y + 1) && _activeStage.GetBoard()[x, y] != null && _activeStage.GetBoard()[x, y].IsEnemy())
                    {
                        if (_activeStage.Get(x, y).GetHealth() >= 100)
                        {
                            continue;
                        }

                        if (_activeStage.Get(x, y).GetHealth() < lastHealth)
                        {
                            target = _activeStage.GetBoard()[x, y];
                            lastHealth = _activeStage.Get(x, y).GetHealth();
                        }
                    }
                }
            }
        }

        return target;
    }

    public void NextStage()
    {
        _playersTurn = true;
        LoadStage(new Stage(this, _stages[_activeStage.GetIndex() + 1], _activeStage.GetIndex() + 1));
    }
}

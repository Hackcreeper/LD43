using System;
using UnityEngine;
using UnityEngine.AI;

public class Unit : MonoBehaviour
{
    private int _boardX;
    private int _boardY;

    private Arena _arena;

    [SerializeField]
    private Class _class;

    [SerializeField]
    private bool _enemy;

    [SerializeField]
    private int _skillReload;

    [SerializeField]
    private Sprite _icon;

    [SerializeField]
    private Transform _healthbar;

    [SerializeField]
    private AudioClip[] _damageSounds;

    [SerializeField]
    private AudioClip _swordSound;

    private Action<Unit> _onDie;

    private bool _skillReady;

    private bool _canMakeAction = true;

    private int _skillReloadCounter;

    private bool _skillUnlocked = true;

    private int _health = 21;

    private int _healthToSub = 0;

    private float _subTimer = 0f;

    private float? _soundTimer;

    public void SetBoardPosition(int x, int y)
    {
        _boardX = x;
        _boardY = y;
    }

    public int GetX() => _boardX;
    public int GetY() => _boardY;

    private Vector3? _destination;

    private Vector3? _halfDestination;
    private Vector3? _originalPosition;

    private float _moveBackTimer = 0f;

    private void Start()
    {
        _skillReloadCounter = _skillReload;
    }

    public void SetArena(Arena arena)
    {
        _arena = arena;
    }

    private void OnMouseDown()
    {
        if (_enemy)
        {
            return;
        }

        if (!_canMakeAction)
        {
            return;
        }

        if (_arena.GetCurrentAction() != FightAction.None)
        {
            return;
        }

        if (_arena.IsPanelOpen())
        {
            return;
        }

        if (!_arena.IsPlayersTurn())
        {
            return;
        }

        _arena.OpenActionPanel(this);
    }

    public string GetClassLabel()
    {
        switch(_class)
        {
            case Class.Archer:
                return "Archer";

            case Class.Swordsman:
                return "Swordsman";

            case Class.Healer:
                return "Healer";

            default:
                return "-";
        }
    }

    public void ActionMade()
    {
        _canMakeAction = false;
    }

    public bool CanMakeAction() => _canMakeAction;

    public void MoveTo(Vector3 destination)
    {
        GetComponent<NavMeshAgent>().destination = destination;
        GetComponent<NavMeshAgent>().isStopped = false;

        GetComponentInChildren<Animator>().SetBool("walking", true);

        _destination = destination;
    }

    public void SkillUsed()
    {
        _skillReloadCounter = _skillReload;
        _skillReady = false;
    }

    public void MoveToHalfWay(Vector3 destination)
    {
        GetComponent<NavMeshAgent>().isStopped = false;
        GetComponent<NavMeshAgent>().destination = destination;

        GetComponentInChildren<Animator>().SetBool("walking", true);

        _originalPosition = transform.position;
        _halfDestination = destination;
    }

    private void Update()
    {
        if (_soundTimer.HasValue)
        {
            _soundTimer -= Time.deltaTime;
            if (_soundTimer <= 0f)
            {
                GetComponent<AudioSource>().clip = _swordSound;
                GetComponent<AudioSource>().Play();

                _soundTimer = null;
            }
        }

        _healthbar.localScale = new Vector3(
            _health / 100f,
            _healthbar.localScale.y,
            _healthbar.localScale.z
        );

        if (_healthToSub != 0)
        {
            _subTimer -= Time.deltaTime;
            if (_subTimer <= 0f)
            {
                if (_healthToSub >= 0)
                {
                    GetComponent<AudioSource>().clip = _damageSounds[UnityEngine.Random.Range(0, _damageSounds.Length - 1)];
                    GetComponent<AudioSource>().Play();
                }

                _health = Mathf.Clamp(_health - _healthToSub, 0, 100);
                _healthToSub = 0;
            }
        }

        if (_health <= 0)
        {
            _onDie?.Invoke(this);
            Destroy(gameObject);
            return;
        }

        if (_originalPosition.HasValue && !_halfDestination.HasValue)
        {
            _moveBackTimer -= Time.deltaTime;

            if(_moveBackTimer <= 0f)
            {
                MoveTo(_originalPosition.Value);
                _originalPosition = null;
            }

            return;
        }

        if (_destination.HasValue)
        {
            if (Vector3.Distance(transform.position, _destination.Value) > .5f)
            {
                return;
            }

            _destination = null;
            GetComponent<NavMeshAgent>().isStopped = true;

            GetComponentInChildren<Animator>().SetBool("walking", false);

            Arena.Instance.DestinationReached();
            return;
        }

        if (_halfDestination.HasValue)
        {
            var half = Vector3.Distance(_originalPosition.Value, _halfDestination.Value) / 1.4f;
            if (Vector3.Distance(transform.position, _halfDestination.Value) > half)
            {
                return;
            }

            _halfDestination = null;
            GetComponent<NavMeshAgent>().isStopped = true;

            GetComponentInChildren<Animator>().SetBool("walking", false);

            Arena.Instance.HalfReached();

            _soundTimer = .5f;
            _moveBackTimer = 1.5f;

            return;
        }
    }

    public bool IsEnemy() => _enemy;

    public void Reset()
    {
        _canMakeAction = true;
    }

    public bool IsSkillReady() => _skillReady;

    public int SkillReadyIn() => _skillReloadCounter;

    public Class GetClass() => _class;

    public void TurnEnded()
    {
        _skillReloadCounter--;
        _skillReady = _skillReloadCounter <= 0;
    }

    public void UnlockSkill() => _skillUnlocked = true;

    public bool IsSkillUnlocked() => _skillUnlocked;

    public Sprite GetIcon() => _icon;

    public void SubHealth(int damage, bool delayed, bool playSound = true)
    {
        if (!delayed)
        {
            _health = Mathf.Clamp(_health - damage, 0, 100);

            if (playSound && damage >= 0)
            {
                GetComponent<AudioSource>().clip = _damageSounds[UnityEngine.Random.Range(0, _damageSounds.Length - 1)];
                GetComponent<AudioSource>().Play();
            }

            return;
        }

        _healthToSub = damage;
        _subTimer = 1.5f;
    }

    public void RegisterOnDie(Action<Unit> action) => _onDie = action;

    public int GetHealth() => _health;
}

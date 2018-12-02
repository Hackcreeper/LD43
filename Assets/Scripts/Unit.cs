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

    private bool _skillReady;

    private bool _canMakeAction = true;

    private int _skillReloadCounter;

    private bool _skillUnlocked;

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
}

using UnityEngine;
using UnityEngine.AI;

public class Human : MonoBehaviour
{
    private NavMeshAgent _agent;

    [SerializeField]
    private Animator _animator;

    [SerializeField]
    private string _prefabName;

    [SerializeField]
    private bool _enemy;

    [SerializeField]
    private Classes _class;

    [SerializeField]
    private bool _canRunAction = true;

    private int boardX;

    private int boardY;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    public void MoveTo(Vector3 targetPosition)
    {
        _agent.isStopped = false;
        _agent.destination = targetPosition;
        _animator.SetBool("walking", true);
    }

    private void Update()
    {
        if (Game.Instance.IsOngoingBattle())
        {
            if (_agent && _agent.enabled) _agent.isStopped = true;

            _animator.SetBool("walking", false);

            return;
        }

        if (!_agent)
        {
            return;
        }

        if (Vector3.Distance(transform.position, _agent.pathEndPosition) < 1.5f)
        {
            _agent.isStopped = true;
            _animator.SetBool("walking", false);
        }
    }

    public string GetPrefabName() => _prefabName;

    private void OnMouseDown()
    {
        if (_enemy || !Game.Instance.IsOngoingBattle() || !Game.Instance.GetArena().IsPlayerTurn() || Game.Instance.GetArena().IsActionRunning() || !_canRunAction || Game.Instance.GetArena().IsPanelOpen())
        {
            return;
        }

        Game.Instance.GetArena().OpenPanel(this);
    }

    public Classes GetClass() => _class;

    public bool CanRunAction() => _canRunAction;

    public void SetBoardPosition(int x, int y)
    {
        boardX = x;
        boardY = y;
    }

    public int GetX() => boardX;
    public int GetY() => boardY;

    public void ExecutedAction() => _canRunAction = false;
}

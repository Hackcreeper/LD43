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

    private bool _canMakeAction = true;

    public void SetBoardPosition(int x, int y)
    {
        _boardX = x;
        _boardY = y;
    }

    public int GetX() => _boardX;
    public int GetY() => _boardY;

    private Vector3? _destination;

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
        GetComponent<NavMeshAgent>().isStopped = false;
        GetComponent<NavMeshAgent>().destination = destination;

        GetComponentInChildren<Animator>().SetBool("walking", true);

        _destination = destination;
    }

    private void Update()
    {
        if (!_destination.HasValue)
        {
            return;
        }

        if (Vector3.Distance(transform.position, _destination.Value) > 1f)
        {
            return;
        }

        _destination = null;
        GetComponent<NavMeshAgent>().isStopped = true;

        GetComponentInChildren<Animator>().SetBool("walking", false);

        Arena.Instance.DestinationReached();
    }

    public bool IsEnemy() => _enemy;

    public void Reset()
    {
        _canMakeAction = true;
    }
}

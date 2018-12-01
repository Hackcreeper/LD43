using UnityEngine;
using UnityEngine.AI;

public class Human : MonoBehaviour
{
    private NavMeshAgent _agent;

    [SerializeField]
    private Animator _animator;

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
        if (Vector3.Distance(transform.position, _agent.pathEndPosition) < 1.5f)
        {
            _agent.isStopped = true;
            _animator.SetBool("walking", false);
        }
    }
}

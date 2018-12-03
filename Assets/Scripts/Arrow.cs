using System;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    private const float Speed = 10f;

    private Vector3 _destination;

    private Action _onHit;

    public void SetDestination(Vector3 destination) => _destination = destination;

    private void Update()
    {
        transform.Translate(0, 0, -Speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, _destination) < 1.5f) {
            _onHit?.Invoke();
            Destroy(gameObject);
        }
    }

    public void RegisterOnHit(Action onHit) => _onHit = onHit;
}

using System.Collections.Generic;
using UnityEngine;

public class Dungeon : MonoBehaviour 
{
    private static readonly List<Human> _humans = new List<Human>();

    private const int CREW_COUNT = 5;

    private readonly string[] _classes = {
        "Swordsman",
        "Archer"
    };

    [SerializeField]
    private Transform[] _spawnPoints;

    private Transform _camera;

    private Vector3? _targetPosition = null;

    private const float SPEED = 2f;

    private void Start()
    {
        _camera = Camera.main.transform;

        if (_humans.Count == 0)
        {
            GenerateCrew();
        }
    }

    private void GenerateCrew()
    {
        for(int i = 0; i < CREW_COUNT; i++)
        {
            var randomClass = _classes[Random.Range(0, _classes.Length)];
            var human = Instantiate(Resources.Load<GameObject>(randomClass));

            _humans.Add(human.GetComponent<Human>());

            human.transform.position = _spawnPoints[i].position + new Vector3(0, 1, 0);
        }

        var average = GetAveragePosition();

        _humans.ForEach(human =>
        {
            human.Offset = new Vector3(
                average.x - human.transform.position.x,
                0,
                average.z - human.transform.position.z
            );
        });
    }

    private void Update()
    {
        MoveCamera();
        MoveHumans();
    }

    private void MoveHumans()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, 1000f))
            {
                _targetPosition = hit.point + new Vector3(0, 1, 0);
            }
        }

        if (_targetPosition.HasValue)
        {
            var original = GetAveragePosition();
            var average = original;

            if (average.x < _targetPosition.Value.x)
            {
                average.x += SPEED * Time.deltaTime;
            }
            else if (average.x > _targetPosition.Value.x)
            {
                average.x -= SPEED * Time.deltaTime;
            }

            if (average.z < _targetPosition.Value.z)
            {
                average.z += SPEED * Time.deltaTime;
            }
            else if (average.z > _targetPosition.Value.z)
            {
                average.z -= SPEED * Time.deltaTime;
            }

            if (Mathf.Abs(average.x - _targetPosition.Value.x) <= 1f)
            {
                average.x = original.x;
            }

            if (Mathf.Abs(average.z - _targetPosition.Value.z) <= 1f)
            {
                average.z = original.z;
            }

            _humans.ForEach(human =>
            {
                human.transform.position = new Vector3(
                    average.x - human.Offset.x,
                    human.transform.position.y,
                    average.z - human.Offset.z
                );
            });
        }
    }

    private void MoveCamera()
    {
        var average = GetAveragePosition();

        _camera.transform.position = new Vector3(
            average.x,
            6,
            average.z - 5.5f
        );
    }

    private Vector3 GetAveragePosition()
    {
        var totalX = 0f;
        var totalZ = 0f;

        _humans.ForEach(human =>
        {
            totalX += human.transform.position.x;
            totalZ += human.transform.position.z;
        });

        return new Vector3(
            totalX / _humans.Count,
            0,
            totalZ / _humans.Count
        );
    }

    public Human[] GetHumans()
    {
        return _humans.ToArray();
    }
}

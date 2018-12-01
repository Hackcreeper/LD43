using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    public static Game Instance { get; private set; }

    private static readonly List<Human> _humans = new List<Human>();

    private const int CREW_COUNT = 5;

    private readonly string[] _classes = {
        "Swordsman",
        "Archer"
    };

    [SerializeField]
    private Dungeon[] _dungeons;

    [SerializeField]
    private Arena _arena;

    [SerializeField]
    private Transform _canvas;

    private int _activeDungeon;

    private bool _ongoingBattle;

    private void Awake()
    {
        Instance = this;

        GenerateCrew();
    }

    private void GenerateCrew()
    {
        var spawnPoints = GetDungeon().GetSpawnPoints();

        for (int i = 0; i < CREW_COUNT; i++)
        {
            var randomClass = _classes[Random.Range(0, _classes.Length)];
            var human = Instantiate(Resources.Load<GameObject>(randomClass));

            _humans.Add(human.GetComponent<Human>());

            human.transform.position = spawnPoints[i].position + new Vector3(0, 1, 0);
        }
    }

    public Dungeon GetDungeon()
    {
        return _dungeons[_activeDungeon];
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !_ongoingBattle)
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, 1000f))
            {
                var position = hit.point + new Vector3(0, 1, 0);
                _humans.ForEach(human =>
                {
                    human.MoveTo(position);
                });
            }
        }
    }

    public Human[] GetHumans() => _humans.ToArray();

    public bool IsOngoingBattle() => _ongoingBattle;

    public Transform GetCanvas() => _canvas;

    public Arena GetArena() => _arena;

    public void StartBattle(string[] enemies)
    {
        _ongoingBattle = true;
        _arena.StartFight(enemies);
    }
}

using System.Collections.Generic;
using UnityEngine;

public class Arena : MonoBehaviour 
{
    private List<Unit> _playerUnits;

    private Stage _activeStage;

    [SerializeField]
    private string[] _classes;

    [SerializeField]
    private GameObject[] _fields;

    public const int FIELD_WIDTH = 9;
    public const int FIELD_HEIGHT = 6;

    public void Start()
    {
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

            _playerUnits.Add(unit.GetComponent<Unit>());
        }
    }

    private void LoadStage(Stage stage)
    {
        _activeStage = stage;
        _activeStage.Start();
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
}

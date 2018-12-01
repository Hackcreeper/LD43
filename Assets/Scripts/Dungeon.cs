using UnityEngine;

public class Dungeon : MonoBehaviour 
{
    [SerializeField]
    private Transform[] _spawnPoints;

    public Transform[] GetSpawnPoints() => _spawnPoints;
}

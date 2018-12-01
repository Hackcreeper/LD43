using UnityEngine;

public class Game : MonoBehaviour
{
    public static Game Instance { get; private set; }

    [SerializeField]
    private Dungeon[] _dungeons;

    private int _activeDungeon;

    private void Awake()
    {
        Instance = this;
    }

    public Dungeon GetDungeon()
    {
        return _dungeons[_activeDungeon];
    }
}

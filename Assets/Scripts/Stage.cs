using UnityEngine;

public class Stage
{
    private readonly Arena _arena;

    private readonly Unit[,] _board = new Unit[Arena.FIELD_WIDTH,Arena.FIELD_HEIGHT];

    public Stage(Arena arena)
    {
        _arena = arena;
    }

    public void Start()
    {
        SpawnPlayerUnits();
    }

    private void SpawnPlayerUnits()
    {
        var playerMin = 0;
        var playerMax = Arena.FIELD_WIDTH / 3;

        foreach(var unit in _arena.GetPlayerUnits())
        {
            var x = 0;
            var y = 0;

            do
            {
                x = Random.Range(playerMin, playerMax);
                y = Random.Range(0, Arena.FIELD_HEIGHT);
            } while (_board[x, y] != null);

            unit.transform.position = _arena.FindField(x + 1, y + 1).position;
            unit.SetBoardPosition(x, y);
            _board[x, y] = unit;
        }
    }

    public Unit[,] GetBoard() => _board;

    public void Set(int x, int y, Unit unit)
    {
        _board[x, y] = unit;
    }
}
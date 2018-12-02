using UnityEngine;

public class Unit : MonoBehaviour
{
    private int _boardX;
    private int _boardY;

    public void SetBoardPosition(int x, int y)
    {
        _boardX = x;
        _boardY = y;
    }

    public int GetX() => _boardX;
    public int GetY() => _boardY;
}

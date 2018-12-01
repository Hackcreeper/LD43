using UnityEngine;

public class Field : MonoBehaviour
{
    private bool _active;

    public void Activate()
    {
        _active = true;
    }

    public void Reset()
    {
        _active = false;
    }

    private void OnMouseDown()
    {
        if (!_active)
        {
            return;
        }

        Game.Instance.GetArena().ClickedField(this);
    }
}

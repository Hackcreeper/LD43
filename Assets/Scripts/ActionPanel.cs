using UnityEngine;
using UnityEngine.UI;

public class ActionPanel : MonoBehaviour
{
    private Unit _unit;

    [SerializeField]
    private Text _jobText;

    public void SetUnit(Unit unit)
    {
        _unit = unit;
        _jobText.text = unit.GetClassLabel();
    }

    public void Move()
    {
        Arena.Instance.StartMoveAction(_unit);
    }

    public void Attack()
    {
        Arena.Instance.StartAttackAction(_unit);
    }
}

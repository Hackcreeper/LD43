using UnityEngine;
using UnityEngine.UI;

public class SacrificeButton : MonoBehaviour
{
    private Unit _unit;

    [SerializeField]
    private Image _iconImage;

    [SerializeField]
    private Text _jobText;

    [SerializeField]
    private Sacrifice _sacrifice;

    public void SetUnit(Unit unit)
    {
        _unit = unit;

        _iconImage.sprite = unit.GetIcon();
        _jobText.text = unit.GetClassLabel();
    }

    public void Select()
    {
        _sacrifice.Selected(_unit);
    }
}

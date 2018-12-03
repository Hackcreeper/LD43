using UnityEngine;
using UnityEngine.UI;

public class ActionPanel : MonoBehaviour
{
    private Unit _unit;

    [SerializeField]
    private Text _jobText;

    [SerializeField]
    private Button _skillButton;

    [SerializeField]
    private Text _skillText;

    [SerializeField]
    private Image _iconImage;

    [SerializeField]
    private Text _attackText;

    public void SetUnit(Unit unit)
    {
        _unit = unit;
        _jobText.text = unit.GetClassLabel();
        _iconImage.sprite = unit.GetIcon();
        _attackText.text = unit.GetClass() == Class.Healer ? "Heal" : "Attack";

        if (!_unit.IsSkillUnlocked())
        {
            _skillButton.gameObject.SetActive(false);
            return;
        }

        if (!_unit.IsSkillReady())
        {
            _skillButton.interactable = false;
            _skillText.text = "Skill ready in " + _unit.SkillReadyIn() + " turns";

            return;
        }

        _skillButton.interactable = true;
        switch(_unit.GetClass())
        {
            case Class.Archer:
                _skillText.text =  "Rapid fire";
                break;

            case Class.Swordsman:
                _skillText.text = "Smash Attack";
                break;

            case Class.Healer:
                _skillText.text = "Heal all";
                break;
        }
    }

    public void Move()
    {
        Arena.Instance.StartMoveAction(_unit);
    }

    public void Attack()
    {
        if (_unit.GetClass() == Class.Healer)
        {
            Arena.Instance.StartHealAction(_unit);
            return;
        }

        Arena.Instance.StartAttackAction(_unit);
    }

    public void Skill()
    {
        if (_unit.GetClass() == Class.Swordsman)
        {
            Arena.Instance.StartSmashAttackAction(_unit);
            return;
        }

        if (_unit.GetClass() == Class.Archer)
        {
            Arena.Instance.StartRapidFireAction(_unit);
            return;
        }
    }
}

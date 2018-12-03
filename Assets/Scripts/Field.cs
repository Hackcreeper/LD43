using UnityEngine;

public class Field : MonoBehaviour
{
    [SerializeField]
    private Material _defaultMaterial;

    [SerializeField]
    private Material _moveableMaterial;

    [SerializeField]
    private Material _attackableMaterial;

    [SerializeField]
    private Material _defaultMaterial2;

    [SerializeField]
    private Material _moveableMaterial2;

    [SerializeField]
    private Material _attackableMaterial2;

    [SerializeField]
    private Material _defaultMaterial3;

    [SerializeField]
    private Material _moveableMaterial3;

    [SerializeField]
    private Material _attackableMaterial3;

    [SerializeField]
    private Material _defaultMaterial4;

    [SerializeField]
    private Material _moveableMaterial4;

    [SerializeField]
    private Material _attackableMaterial4;

    private bool _active;

    private int _index;

    public void Activate(FightAction action)
    {
        _active = true;

        if (action == FightAction.Move)
        {
            if (_index == 0)
            {
                GetComponent<MeshRenderer>().material = _moveableMaterial;
                return;
            }

            if (_index == 1)
            {
                GetComponent<MeshRenderer>().material = _moveableMaterial2;
                return;
            }

            if (_index == 2)
            {
                GetComponent<MeshRenderer>().material = _moveableMaterial3;
                return;
            }

            if (_index == 3)
            {
                GetComponent<MeshRenderer>().material = _moveableMaterial4;
                return;
            }

            return;
        }

        if (action == FightAction.Attack || action == FightAction.RapidFire)
        {
            if (_index == 0)
            {
                GetComponent<MeshRenderer>().material = _attackableMaterial;
                return;
            }

            if (_index == 1)
            {
                GetComponent<MeshRenderer>().material = _attackableMaterial2;
                return;
            }

            if (_index == 2)
            {
                GetComponent<MeshRenderer>().material = _attackableMaterial3;
                return;
            }

            if (_index == 3)
            {
                GetComponent<MeshRenderer>().material = _attackableMaterial4;
                return;
            }
        }
    }

    public void Deactivate()
    {
        SetIndex(_index);

        _active = false;
    }

    private void OnMouseDown()
    {
        if (!_active)
        {
            return;
        }

        Arena.Instance.ClickedField(this);
    }

    public void SetIndex(int index)
    {
        _index = index;

        if (index == 0)
        {
            GetComponent<MeshRenderer>().material = _defaultMaterial;
            return;
        }

        if (index == 1)
        {
            GetComponent<MeshRenderer>().material = _defaultMaterial2;
            return;
        }

        if (index == 2)
        {
            GetComponent<MeshRenderer>().material = _defaultMaterial3;
            return;
        }

        if (index == 3)
        {
            GetComponent<MeshRenderer>().material = _defaultMaterial4;
            return;
        }
    }
}

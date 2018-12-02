﻿using UnityEngine;

public class Field : MonoBehaviour
{
    [SerializeField]
    private Material _defaultMaterial;

    [SerializeField]
    private Material _moveableMaterial;

    private bool _active;

    public void Activate(FightAction action)
    {
        _active = true;

        if (action == FightAction.Move)
        {
            GetComponent<MeshRenderer>().material = _moveableMaterial;
        }
    }

    public void Deactivate()
    {
        GetComponent<MeshRenderer>().material = _defaultMaterial;

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
}
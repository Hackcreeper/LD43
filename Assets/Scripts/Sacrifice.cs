using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Sacrifice : MonoBehaviour
{
    [SerializeField]
    private SacrificeButton _button1;

    [SerializeField]
    private SacrificeButton _button2;

    [SerializeField]
    private SacrificeButton _button3;

    [SerializeField]
    private SacrificeButton _button4;

    [SerializeField]
    private SacrificeButton _button5;

    [SerializeField]
    private Transform[] _spawns;

    [SerializeField]
    private Transform _sacrificeLocation;

    [SerializeField]
    private Transform _targetMoveLocation;

    [SerializeField]
    private Animator _animator;

    [SerializeField]
    private Transform _camera;

    [SerializeField]
    private Transform _cameraTarget;

    [SerializeField]
    private Text _sacrificeText;

    [SerializeField]
    private Image _blackImage;

    [SerializeField]
    private Transform _originalCamPosition;

    [SerializeField]
    private GameObject _turnText;

    private bool _sacrificed;
    private float _timer;
    private Unit _killed;

    private float _timer2;
    private float _timer3;
    private float _timer4;

    private bool _once;

    public void GoOn()
    {
        _sacrificed = false;
        _timer = 2f;
        _timer2 = 2f;
        _timer3 = 2f;
        _timer4 = 1.5f;
        _once = false;

        _animator.Play("Closed");
        _sacrificeText.color = new Color(0.6698113f, 0, 0, 1);

        var units = Arena.Instance.GetPlayerUnits();

        if (units.Length >= 1)
        {
            _button1.SetUnit(units[0]);
            _button1.gameObject.SetActive(true);
        }

        if (units.Length >= 2)
        {
            _button2.SetUnit(units[1]);
            _button2.gameObject.SetActive(true);
        }

        if (units.Length >= 3)
        {
            _button3.SetUnit(units[2]);
            _button3.gameObject.SetActive(true);
        }

        if (units.Length >= 4)
        {
            _button4.SetUnit(units[3]);
            _button4.gameObject.SetActive(true);
        }

        if (units.Length >= 5)
        {
            _button5.SetUnit(units[4]);
            _button5.gameObject.SetActive(true);
        }

        for(var i = 0; i < units.Length; i++)
        {
            units[i].GetComponent<NavMeshAgent>().enabled = false;
            units[i].transform.localScale = Vector3.one * 1.5f;
            units[i].transform.position = _spawns[i].position;
        }
    }

    public void Selected(Unit unit)
    {
        _button1.gameObject.SetActive(false);
        _button2.gameObject.SetActive(false);
        _button3.gameObject.SetActive(false);
        _button4.gameObject.SetActive(false);
        _button5.gameObject.SetActive(false);

        // TODO: Play sacrifice sound
        unit.transform.position = _sacrificeLocation.position;
        unit.gameObject.AddComponent<Rigidbody>().mass = 10f;

        _killed = unit;
        _sacrificed = true;
    }

    private void Update()
    {
        if (!_sacrificed)
        {
            return;
        }

        _timer -= Time.deltaTime;
        if (_timer > 0f)
        {
            return;
        }

        _killed.SubHealth(10000, false);
        _animator.SetBool("open", true);

        var all = Arena.Instance.GetPlayerUnits();
        foreach(var unit in all)
        {
            unit.GetComponent<NavMeshAgent>().enabled = true;
            unit.GetComponent<NavMeshAgent>().speed *= 3f;
            unit.GetComponent<NavMeshAgent>().destination = _targetMoveLocation.position;
            unit.GetComponent<NavMeshAgent>().isStopped = false;
            unit.GetComponentInChildren<Animator>().SetBool("walking", true);
        }

        _timer2 -= Time.deltaTime;
        if (_timer2 > 0)
        {
            return;
        }

        _camera.transform.position = Vector3.Lerp(_camera.transform.position, _cameraTarget.position, 2f * Time.deltaTime);
        _camera.transform.rotation = Quaternion.Lerp(_camera.transform.rotation, _cameraTarget.rotation, 2f * Time.deltaTime);

        _timer3 -= Time.deltaTime;
        if (_timer3 > 0)
        {
            return;
        }

        _sacrificeText.color = Color.Lerp(
            _sacrificeText.color,
            new Color(0.6698113f, 0, 0, 0),
            4f * Time.deltaTime
        );

        _timer4 -= Time.deltaTime;
        if (_timer4 > 0)
        {
            return;
        }

        _sacrificed = false;

        _blackImage.gameObject.SetActive(true);
        _blackImage.GetComponent<Fadeout>().enabled = false;
        _blackImage.color = new Color(0, 0, 0, 1);

        foreach (var unit in all)
        {
            unit.GetComponent<NavMeshAgent>().enabled = false;
            unit.GetComponentInChildren<Animator>().SetBool("walking", false);

            unit.transform.localScale = Vector3.one * 0.5f;
        }

        Arena.Instance.NextStage();

        foreach (var unit in all)
        {
            unit.GetComponent<NavMeshAgent>().enabled = true;
            unit.GetComponent<NavMeshAgent>().isStopped = true;
            unit.GetComponent<NavMeshAgent>().speed /= 3;
        }

        gameObject.SetActive(false);

        _camera.position = _originalCamPosition.position;
        _camera.rotation = _originalCamPosition.rotation;

        _turnText.SetActive(true);
        _blackImage.GetComponent<Fadeout>().enabled = true;

        _animator.SetBool("open", false);
        _animator.Play("Closed");
    }
}

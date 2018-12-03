using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour 
{
    [SerializeField]
    private Text[] _texts;

    [SerializeField]
    private Animator _doorAnimator;

    [SerializeField]
    private Transform _targetPosition;

    [SerializeField]
    private NavMeshAgent[] _agents;

    [SerializeField]
    private Transform _camera;

    [SerializeField]
    private Transform _cameraTarget;

    private bool _starting = false;

    private float _timer = 4f;

    private float _timer2 = 3f;

    private void Update()
    {
        if (!_starting)
        {
            if (Input.anyKeyDown)
            {
                _starting = true;
            }

            return;
        }

        foreach (var text in _texts)
        {
            text.color = Color.Lerp(text.color, new Color(1f, 1f, 1f, 0f), 15 * Time.deltaTime);
        }

        _doorAnimator.SetBool("open", true);

        foreach (var agent in _agents)
        {
            agent.GetComponentInChildren<Animator>().SetBool("walking", true);
            agent.destination = _targetPosition.position;
        }

        _timer -= Time.deltaTime;

        if (_timer <= 0)
        {
            _camera.transform.position = Vector3.Lerp(_camera.transform.position, _cameraTarget.position, 2f * Time.deltaTime);
            _camera.transform.rotation = Quaternion.Lerp(_camera.transform.rotation, _cameraTarget.rotation, 2f * Time.deltaTime);

            _timer2 -= Time.deltaTime;
            if (_timer2 <= 0)
            {
                SceneManager.LoadScene("Game");
            }
        }
    }
}

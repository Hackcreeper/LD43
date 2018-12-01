using UnityEngine;

public class Hover : MonoBehaviour 
{
    private const float BASE = 4.128f;

	void Update () 
    {
        transform.position = new Vector3(
            transform.position.x,
            BASE + Mathf.Sin(Time.time * 2f) * 0.25f,
            transform.position.z
        );
	}
}

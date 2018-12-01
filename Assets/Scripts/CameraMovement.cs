using UnityEngine;
using System.Collections;

public class CameraMovement : MonoBehaviour
{
    private void Update()
    {
        var average = GetAveragePosition();

        transform.position = new Vector3(
            average.x,
            6,
            average.z - 5.5f
        );
    }

    private Vector3 GetAveragePosition()
    {
        var totalX = 0f;
        var totalZ = 0f;

        foreach(var human in Game.Instance.GetHumans())
        {
            totalX += human.transform.position.x;
            totalZ += human.transform.position.z;
        }

        return new Vector3(
            totalX / Game.Instance.GetHumans().Length,
            0,
            totalZ / Game.Instance.GetHumans().Length
        );
    }
}

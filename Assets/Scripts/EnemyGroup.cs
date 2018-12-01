using UnityEngine;

public class EnemyGroup : MonoBehaviour
{
    private void Update()
    {
        var humans = Game.Instance.GetHumans();

        foreach(var human in humans)
        {
            if (Vector3.Distance(human.transform.position, transform.position) <= 7f)
            {
                Debug.Log("BATTLE!");
                break;
            }
        }
    }
}

using UnityEngine;

public class EnemyGroup : MonoBehaviour
{
    [SerializeField]
    private string[] _enemies;

    private void Update()
    {
        if (Game.Instance.IsOngoingBattle())
        {
            return;
        }

        var humans = Game.Instance.GetHumans();

        foreach(var human in humans)
        {
            if (Vector3.Distance(human.transform.position, transform.position) <= 4f || Input.GetKeyDown(KeyCode.B))
            {
                Game.Instance.StartBattle(_enemies);
                break;
            }
        }
    }
}

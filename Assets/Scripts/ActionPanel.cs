using UnityEngine;
using UnityEngine.UI;

public class ActionPanel : MonoBehaviour
{
    private Human _human;

    [SerializeField]
    private Text _job;

    public void SetHuman(Human human)
    {
        _human = human;
        _job.text = GetClassText(human);
        // TODO Set name and icon
    }

    private string GetClassText(Human human)
    {
        switch (human.GetClass())
        {
            case Classes.Swordsman:
                return "Swordsman";
            case Classes.Archer:
                return "Archer";
            default:
                return "-";
        }
    }

    public void Move()
    {
        Game.Instance.GetArena().StartAction(_human, BattleAction.Move);
        Destroy(gameObject);
    }
}

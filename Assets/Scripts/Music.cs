using UnityEngine;

public class Music : MonoBehaviour 
{
    public static Music Instance { private set; get; }

    private void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}

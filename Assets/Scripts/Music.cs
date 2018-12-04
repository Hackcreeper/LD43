using UnityEngine;

public class Music : MonoBehaviour 
{
    public static Music Instance { private set; get; }

    public AudioClip[] _stages;

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

    public void SetStage(int index)
    {
        GetComponent<AudioSource>().clip = _stages[index];
        GetComponent<AudioSource>().Play();
    }
}

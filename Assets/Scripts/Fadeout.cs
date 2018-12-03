using UnityEngine;
using UnityEngine.UI;

public class Fadeout : MonoBehaviour 
{
    private void Update()
    {
        GetComponent<Image>().color = Color.Lerp(
            GetComponent<Image>().color,
            new Color(1f, 1f, 1f, 0f),
            2f * Time.deltaTime
        );

        if (GetComponent<Image>().color.a <= 0.1f)
        {
            gameObject.SetActive(false);
        }
    }
}

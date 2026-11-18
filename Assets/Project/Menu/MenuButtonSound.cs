using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class MenuButtonSound : MonoBehaviour
{
    [SerializeField] private AudioClip click;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(PlayClick);
    }

    private void PlayClick()
    {
        AudioManager.Instance?.PlayUI(click);
    }
}

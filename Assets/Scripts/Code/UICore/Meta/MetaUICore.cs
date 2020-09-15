using System;
using UnityEngine;
using UnityEngine.UI;

public class MetaUICore : MonoBehaviour
{
    [SerializeField] private GameObject Root;
    [SerializeField] public Button startGame;

    public void Init(Action startPress)
    {
        startGame.onClick.AddListener(() => startPress?.Invoke());
        Show();
    }

    public void Show()
    {
        Root.SetActive(true);
    }

    public void Hide()
    {
        Root.SetActive(false);
    }

    public void Dispose()
    {
        Hide();
        startGame.onClick.RemoveAllListeners(); 
    }
}

using UnityEngine;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] 
    private GameObject root;
    [SerializeField] 
    private LoadingTextView _loadingTextView;

    public void Show()
    {
        _loadingTextView.Show();
        root.SetActive(true);
    }

    public void Hide()
    {
        _loadingTextView.Hide();
        root.SetActive(false);
    }
}

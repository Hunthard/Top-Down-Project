using TMPro;
using UnityEngine;

public class LoadingTextView : MonoBehaviour
{
    [SerializeField] private float delay = 1f;
    public TextMeshProUGUI textField;

    private int _count = 0;
    private string _text;
    private float _leftTime;
    private bool isShow;
    
    public void Show(string text = "Loading")
    {
        _text = text;
        UpdateText();
        _leftTime = delay;
        isShow = true;
    }

    public void Hide()
    {
        isShow = false;
    }

    private void Update()
    {
        if (!isShow)
            return;
        
        _leftTime -= Time.deltaTime;
        
        if (_leftTime > 0)
            return;
        
        _leftTime = delay;
        _count++;
        if (_count > 3) _count = 0;
        UpdateText();
    }

    private void UpdateText()
    {
        textField.text = _text + " ";
        for (int i = 0; i < _count; i++)
        {
            textField.text += ".";
        }
    }
}

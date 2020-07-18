using UnityEngine;
using UnityEngine.Events;

public class Menu : MonoBehaviour
{
    // Вызывается когда меню появляется на экране
    public UnityEvent menuDidAppear = new UnityEvent();

    // Вызывается когда меню исчезает с экрана
    public UnityEvent menuWillDisappear = new UnityEvent();
}

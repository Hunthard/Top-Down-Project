using UnityEngine;

public class UICore : MonoBehaviour
{
    [SerializeField]
    public SessionUICore SessionUICore;
    
    [SerializeField]
    public MetaUICore MetaUiCore;
    
    [SerializeField]
    public LoadingScreen LoadingScreen;
    private void Awake()
    {
        DontDestroyOnLoad(this);
    }
}

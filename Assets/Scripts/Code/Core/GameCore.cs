using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public class GameCore : MonoBehaviour
{
    [SerializeField] 
    public UICore UICore;
    
    private IServicesAggregator _servicesAggregator;

    public static GameCore Instance => _instance;
    private static GameCore _instance;


    private void Awake()
    {
        _instance = this;
        UICore.LoadingScreen.Show();
        DontDestroyOnLoad(this);
    }

    private bool IsServiceAggregatorInitialized;
    
    private void ServiceAggregatorInitializedCb()
    {
        IsServiceAggregatorInitialized = true;
    }
    private IEnumerator Start()
    {
        IsServiceAggregatorInitialized = false;
        _servicesAggregator = new ServicesAggregator();
        _servicesAggregator.Init(ServiceAggregatorInitializedCb);

        while (!IsServiceAggregatorInitialized)
        {
            yield return 0;
        }
        
        var assetService = _servicesAggregator.GetService<IAssetService>();
        assetService.GetSceneAsync("meta_scene", OnMetaSceneLoaded);
    }

    private MetaManager _metaManager;
    
    public void SwitchToMeta()
    {
        UICore.LoadingScreen.Show();
        sessionManager.Dispose();
        sessionManager = null;
        var assetService = _servicesAggregator.GetService<IAssetService>();
        assetService.GetSceneAsync("meta_scene", OnMetaSceneLoaded);
    }
    
    private void OnMetaSceneLoaded(SceneInstance sceneInstance)
    {
        SceneManager.SetActiveScene(sceneInstance.Scene);
        _metaManager = GameObject.FindObjectOfType<MetaManager>();
        
        StartCoroutine(_metaManager.Init(_servicesAggregator, UICore.MetaUiCore, this));
        UICore.LoadingScreen.Hide();
    }

    private SessionManager sessionManager;
    
    public void SwitchToSession(string id = "battle1_scene")
    {
        UICore.LoadingScreen.Show();
        _metaManager.Dispose();
        _metaManager = null;
        var assetService = _servicesAggregator.GetService<IAssetService>();
        assetService.GetSceneAsync("game_scene", OnSessionSceneLoaded);
    }

    private void OnSessionSceneLoaded(SceneInstance sceneInstance)
    {
        SceneManager.SetActiveScene(sceneInstance.Scene);
        sessionManager = GameObject.FindObjectOfType<SessionManager>();
        StartCoroutine(sessionManager.Init(_servicesAggregator,
            UICore.SessionUICore,
            new InitSessionParams()
            {
                locationId = "battle"
            },
            this));
        UICore.LoadingScreen.Hide();
    }
}

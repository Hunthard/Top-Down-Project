using System.Collections;
using UnityEngine;

public class MetaManager : MonoBehaviour
{
    private GameCore _gameCore;
    private MetaUICore _metaUiCore;
    
    public IEnumerator Init(IServicesAggregator servicesAggregator, MetaUICore metaUiCore, GameCore gameCore)
    {
        _metaUiCore = metaUiCore;
        _gameCore = gameCore;
        metaUiCore.Init(() => _gameCore.SwitchToSession());
        yield return 0;
    }

    public void Dispose()
    {
        _metaUiCore.Dispose();
        _metaUiCore = null;
        _gameCore = null;
    }
}

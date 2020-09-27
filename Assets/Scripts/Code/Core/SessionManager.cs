using System.Collections;
using UnityEngine;

public class SessionManager : MonoBehaviour
{
    private GameCore _gameCore;
    private SessionUICore _sessionUiCore;
    
    private IControllerAggregator _controllerAggregator;
    private IServicesAggregator _servicesAggregator;

    private bool init = false;
    private bool IsControllerAggregatorInitialized;
    
    private void Awake()
    {
        init = false;
    }

    private void ControllerAggregatorInitializedCb()
    {
        IsControllerAggregatorInitialized = true;
    }
    
    public IEnumerator Init(IServicesAggregator servicesAggregator, SessionUICore sessionUiCore, InitSessionParams sessionParams, GameCore gameCore)
    {
        _gameCore = gameCore;
        _sessionUiCore = sessionUiCore;
        IsControllerAggregatorInitialized = false;
        _servicesAggregator = servicesAggregator;

        _controllerAggregator = new ControllerAggregator();
        _controllerAggregator.Init(_servicesAggregator, sessionParams, ControllerAggregatorInitializedCb);
        
        while (!IsControllerAggregatorInitialized)
        {
            yield return 0;
        }
        
        
        init = true;

        OnStart();
    }

    public IControllerAggregator GetIControllerAggregator()
    {
        return _controllerAggregator;
    }
    
    private void OnStart()
    {
        _controllerAggregator.OnStart();
    }
    
    private void Update()
    {
        if (!init)
        {
            return;
        }
        
        _controllerAggregator.OnUpdate(Time.deltaTime);
    }


    public void Dispose()
    {
        init = false;
        _controllerAggregator.Dispose();
        _gameCore = null;
        _controllerAggregator = null;
        _servicesAggregator = null;
    }
}
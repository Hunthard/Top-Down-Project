using System;
using System.Collections.Generic;

public interface IServicesAggregator
{
    void Init(Action initializedCb);
    T GetService<T>() where T : IService;
}

public interface IService
{
    void Init(Action initializedCb);
}

public class ServicesAggregator : IServicesAggregator
{
    private AssetService _assetService;
    
    private readonly List<IService> _services;

    private event Action _initialized;
    
    public ServicesAggregator()
    {
        _assetService = new AssetService();
        _services = new List<IService>()
        {
            _assetService,
        };
    }

    public T GetService<T>() where T : IService
    {
        for (int i = 0; i < _services.Count; i++)
        {
            if (_services[i] is T value)
            {
                return value;
            }
        }

        return default;
    }

    public void Init(Action initializedCb)
    {
        _initialized = initializedCb;
        _assetService.Init(AssetServiceInitializedCb);
    }

    private void AssetServiceInitializedCb()
    {
        _initialized?.Invoke();
    }
}

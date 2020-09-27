using System;
using UnityEngine;

public interface IDungeonController : IController
{
    bool Inited { get; }
    Vector3 GetRandomPoint { get; }
}

public class DungeonController : BaseController,IDungeonController
{
    public bool Inited { get; private set; }
    public Vector3 GetRandomPoint => _generator.GetRandomPoint;
    
    private const string dungeonPrefab = "dungeon_prefab";
    private DungeonGenerator _generator;
    
    
    public override void Init(IServicesAggregator servicesAggregator, InitSessionParams sessionParams, IControllerAggregator controllers,
        Action<BaseController> initializedCb)
    {
        var assetService = servicesAggregator.GetService<IAssetService>();
        assetService.GetAssetAsync<DungeonGenerator>(dungeonPrefab, playerView =>
        {
            CreateView(playerView);
            _generator.Generate();
            Inited = true;
            initializedCb?.Invoke(this);
        });
    }

    private void CreateView(DungeonGenerator generator)
    {
        Debug.LogError("CreateView 1");
        this._generator = UnityEngine.Object.Instantiate(generator, Vector3.zero, Quaternion.identity);
        Debug.LogError("CreateView 2");
    }
    
    public override void Dispose()
    {
        
    }
}

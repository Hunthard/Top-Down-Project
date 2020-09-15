using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;

public interface IAssetService: IService
{
    void GetAssetAsync<T>(string id, Action<T> cb);

    void GetSceneAsync(string id, Action<SceneInstance> cb, LoadSceneMode loadMode = LoadSceneMode.Single,
        bool activateOnLoad = true);
    Sprite GetSprite(string id);
    //DataLibrary DataLibrary { get; }
}

public class AssetService : IAssetService
{
    #region IAssetService

    public void GetAssetAsync<T>(string id, Action<T> cb)
    {
        var loadAssetAsync = Addressables.LoadAssetAsync<GameObject>(id);
        loadAssetAsync.Completed += (operation) =>
        {
            if (operation.IsDone)
            {
                var result = operation.Result;
                cb?.Invoke(result.GetComponent<T>());
            }
        };
    }

    public void GetSceneAsync(string id, Action<SceneInstance> cb, LoadSceneMode loadMode = LoadSceneMode.Single, bool activateOnLoad = true)
    {
        var loadAssetAsync = Addressables.LoadSceneAsync(id, loadMode, activateOnLoad);
        loadAssetAsync.Completed += (operation) =>
        {
            if (operation.IsDone)
            {
                var result = operation.Result;
                cb?.Invoke(result);
            }
        };
    }
    
    public Sprite GetSprite(string id)
    {
        Sprite sprite = null;
        for (int i = 0; i < _atlases.Count; i++)
        {
            sprite = _atlases[i].GetSprite(id);
            if (sprite != null)
            {
                break;
            }
        }

        return sprite;
    }

    //public DataLibrary DataLibrary { get; private set; }

    #endregion
    
    private List<SpriteAtlas> _atlases;
    
    private enum PreloadAssets
    {
        Data,
        Atlases
    }
    private HashSet<PreloadAssets> loadedAssetsOnInit;
    
    private event Action _initialized;
    
    public void Init(Action initializedCb)
    {
        _initialized = initializedCb;
        _initialized?.Invoke();
        _initialized = null;
        // loadedAssetsOnInit = new HashSet<PreloadAssets>()
        // {
        //     PreloadAssets.Atlases,
        //     PreloadAssets.Data
        // };
        //
        //
        // LoadSoLibrary();
        // LoadSpriteAtlases();
    }

    private void LoadSpriteAtlases()
    {
        var _atlasesId = new List<object>()
        {
            "weapon_icons",
        };
        
        var loadAssetsAsync = Addressables.LoadAssetsAsync<SpriteAtlas>(_atlasesId, null, Addressables.MergeMode.None);

        loadAssetsAsync.Completed += operation =>
        {
            if (operation.IsDone)
            {
                _atlases = new List<SpriteAtlas>(operation.Result);
            }
            loadedAssetsOnInit.Remove(PreloadAssets.Atlases);
            CheckEndInit();
        };
    }
    
    private void LoadSoLibrary()
    {
        // var loadSoLibraryAssetAsync = Addressables.LoadAssetAsync<SOLibrary>("so_library");
        // loadSoLibraryAssetAsync.Completed += (operation) =>
        // {
        //     if (operation.IsDone)
        //     {
        //         DataLibrary = operation.Result.GenerateData();
        //     }
        //     loadedAssetsOnInit.Remove(PreloadAssets.Data);
        //     CheckEndInit();
        // };
    }

    private void CheckEndInit()
    {
        if (loadedAssetsOnInit.Count == 0)
        {
            _initialized?.Invoke();
        }
    }
}

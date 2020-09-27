using System;
using System.Collections;
using UnityEngine;

public interface IPlayerController: IController
{
}

public class PlayerController : BaseController, IPlayerController
{
    //Settings
    private float _playerSpeed = 4.0f;
    private const string playerPrefab = "player_prefab";

    
    private bool _isLookAtRight = true;  // Флаг для определения направления взгляда персонажа

    private PlayerView _view;

    private IInputController _inputController;
    private ICameraController _cameraController;
    private IDungeonController _dungeonController;

    public override void Init(IServicesAggregator servicesAggregator, InitSessionParams sessionParams, IControllerAggregator controllers,
        Action<BaseController> initializedCb)
    {
        _inputController = controllers.GetController<IInputController>();
        _cameraController = controllers.GetController<ICameraController>();
        _dungeonController = controllers.GetController<IDungeonController>();
        
        var assetService = servicesAggregator.GetService<IAssetService>();
        assetService.GetAssetAsync<PlayerView>(playerPrefab, playerView =>
        {
            GameCore.Instance.StartCoroutine(TestMe(playerView, initializedCb));
            // CreateView(playerView, SpawnPosition);
            // initializedCb?.Invoke(this);
        });
    }

    private IEnumerator TestMe(PlayerView view, Action<BaseController> initializedCb)
    {
        while (!_dungeonController.Inited)
        {
            yield return null;
        }
        
        CreateView(view, _dungeonController.GetRandomPoint);
        _cameraController.PlayerCamera.Follow = _view.transform;
        initializedCb?.Invoke(this);
    }
    
    private void CreateView(PlayerView view, Vector3 currentPosition)
    {
        _view = UnityEngine.Object.Instantiate(view, currentPosition, Quaternion.identity);
    }
    
    private void Move(float dt)
    {
        var move = _inputController.MoveDirection;

        // Если персонаж идёт вправо, а смотрит влево - поворот спрайта
        if (move.x > 0 && !_isLookAtRight)
        {           
            _isLookAtRight = !_isLookAtRight;
        }
        // Также если персонаж идёт влево, а смотрит вправо - поворот спрайта
        else if (move.x < 0 && _isLookAtRight)
        {
            _isLookAtRight = !_isLookAtRight;
        }

        _view.Move(move, _isLookAtRight ,dt, _playerSpeed);
    }    

    public override void OnUpdate(float dt)
    {
        Move(dt);
    }

    public override void Dispose()
    {
        
    }
}

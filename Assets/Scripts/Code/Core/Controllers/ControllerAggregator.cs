using System;
using System.Collections.Generic;

public interface IControllerAggregator
{
    void Init(IServicesAggregator servicesAggregator, InitSessionParams sessionParams, Action initializedCb);
    void OnUpdate(float dt);
    void OnStart();
    T GetController<T>() where T : IController;
    void Dispose();
}

public interface IController { }

public abstract class BaseController
{
    public virtual void Init(IServicesAggregator servicesAggregator, InitSessionParams sessionParams,
        IControllerAggregator controllers,
        Action<BaseController> initializedCb)
    {
        initializedCb?.Invoke(this);
    }

    public virtual void OnStart()
    { }

    public virtual void OnUpdate(float dt)
    { }

    public abstract void Dispose();
}

public class ControllerAggregator : IControllerAggregator
{
    private readonly InputController _inputController;
    private readonly CameraController _cameraController;

    private readonly List<BaseController> _controllers;

    public ControllerAggregator()
    {
        _inputController = new InputController();
        _cameraController = new CameraController();
        _controllers = new List<BaseController>()
        {
            _inputController,
            _cameraController,
        };
    }

    public T GetController<T>() where T : IController
    {
        for (int i = 0; i < _controllers.Count; i++)
        {
            if (_controllers[i] is T value)
            {
                return value;
            }
        }

        return default;
    }

    public void Dispose()
    {
        for (var i = 0; i < _controllers.Count; i++)
        {
            _controllers[i].Dispose();
        }
    }

    void IControllerAggregator.Init(IServicesAggregator servicesAggregator, InitSessionParams sessionParams,
        Action initializedCb)
    {
        var controllersNotInitialized = new HashSet<BaseController>(_controllers);

        for (var i = 0; i < _controllers.Count; i++)
        {
            _controllers[i].Init(servicesAggregator, sessionParams, this, OnControllerInitialized);
        }

        void OnControllerInitialized(BaseController controller)
        {
            controllersNotInitialized.Remove(controller);
            if (controllersNotInitialized.Count == 0)
            {
                initializedCb?.Invoke();
            }
        }
    }

    void IControllerAggregator.OnStart()
    {
        for (var i = 0; i < _controllers.Count; i++)
        {
            _controllers[i].OnStart();
        }
    }

    void IControllerAggregator.OnUpdate(float dt)
    {
        for (var i = 0; i < _controllers.Count; i++)
        {
            _controllers[i].OnUpdate(dt);
        }
    }
}

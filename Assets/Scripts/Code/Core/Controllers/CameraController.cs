using System;
using Cinemachine;
using Object = UnityEngine.Object;

public interface ICameraController : IController
{
    CinemachineVirtualCamera PlayerCamera { get; }
}

public class CameraController : BaseController, ICameraController
{
    public CinemachineVirtualCamera PlayerCamera { get; private set; }

    public override void Init(IServicesAggregator servicesAggregator, InitSessionParams sessionParams, IControllerAggregator controllers,
        Action<BaseController> initializedCb)
    {
        var vrCamera = Object.FindObjectOfType<CinemachineVirtualCamera>();
        PlayerCamera = vrCamera;
        initializedCb?.Invoke(this);
    }


    public override void Dispose()
    { }
}

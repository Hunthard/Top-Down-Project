using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICameraController : IController
{
    Camera PlayerCamera { get; }
}

public class CameraController : BaseController, ICameraController
{
    public Camera PlayerCamera { get; private set; }

    public override void OnStart()
    {
        PlayerCamera = Camera.main;
    }

    public override void Dispose()
    { }
}

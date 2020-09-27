using UnityEngine;

public enum Command
{
    Run,
    Fire,
    ReloadWeapon,
    SwitchWeapon1,
    SwitchWeapon2,
    SwitchWeapon3,
}

public interface IInputController : IController
{
    Vector2 MoveDirection { get; }
    Vector2 MouseDirection { get; }
    Vector2 MousePosition { get; }
    bool IsInputCommand(Command command);
}

public class InputController : BaseController, IInputController
{
    private bl_Joystick _blJoystick;
    public Vector2 MoveDirection { get; private set; }
    public Vector2 MouseDirection { get; private set; }
    public Vector2 MousePosition { get; private set; }

    public override void OnStart()
    {
        base.OnStart();
        _blJoystick = Object.FindObjectOfType<bl_Joystick>();
    }

    public override void OnUpdate(float dt)
    {
        GetMoveDirection();
        GetMouseDirection();
        GetMousePosition();
    }
    
    public bool IsInputCommand(Command command)
    {
        switch (command)
        {
            case Command.Run:
                return Input.GetKey(KeyCode.LeftShift);
            case Command.Fire:
                return Input.GetMouseButton(0);
            case Command.ReloadWeapon:
                return Input.GetKey(KeyCode.R);
            case Command.SwitchWeapon1:
                return Input.GetKey(KeyCode.Alpha1);
            case Command.SwitchWeapon2:
                return Input.GetKey(KeyCode.Alpha2);
            case Command.SwitchWeapon3:
                return Input.GetKey(KeyCode.Alpha3);
        }

        return false;// Input.GetButton(command.ToString());
    }
    
    private void GetMoveDirection()
    {
        var useJoystick = true;
        if (useJoystick)
        {
            MoveDirection = new Vector2(_blJoystick.Horizontal, _blJoystick.Vertical);
        }
        else
        {
            MoveDirection = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxisRaw("Vertical"));
        }
    }

    private void GetMouseDirection()
    {
        MouseDirection = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
    }

    private void GetMousePosition()
    {
        MousePosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
    }

    public override void Dispose()
    {
        
    }
}

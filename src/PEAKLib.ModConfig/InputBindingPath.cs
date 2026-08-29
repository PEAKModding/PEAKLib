using System;
using UnityEngine.InputSystem;

namespace PEAKLib.ModConfig;

internal enum InputBindingDevice
{
    Unsupported,
    Keyboard,
    Mouse,
    Gamepad,
}

internal static class InputBindingPath
{
    public static InputBindingDevice GetDevice(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return InputBindingDevice.Unsupported;

        string layout = InputControlPath.TryGetDeviceLayout(path);
        if (string.IsNullOrEmpty(layout))
            return InputBindingDevice.Unsupported;

        if (InputSystem.IsFirstLayoutBasedOnSecond(layout, "Keyboard"))
            return InputBindingDevice.Keyboard;

        if (InputSystem.IsFirstLayoutBasedOnSecond(layout, "Mouse"))
            return InputBindingDevice.Mouse;

        if (
            InputSystem.IsFirstLayoutBasedOnSecond(layout, "Gamepad")
            || InputSystem.IsFirstLayoutBasedOnSecond(layout, "Joystick")
        )
            return InputBindingDevice.Gamepad;

        return InputBindingDevice.Unsupported;
    }

    public static bool IsValid(string path)
    {
        if (GetDevice(path) == InputBindingDevice.Unsupported)
            return false;

        string controlLayout = InputControlPath.TryGetControlLayout(path);
        return !string.IsNullOrEmpty(controlLayout)
            && controlLayout != "*"
            && !path.EndsWith("/anyKey", StringComparison.OrdinalIgnoreCase);
    }
}

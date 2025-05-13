using ImprovedInput;
using RWCustom;
using System;
using UnityEngine;

namespace Vinki;

// Credit to ImprovedInput
public class KeyCodeTranslator
{
    public static string GetImprovedInputKeyName(int player, string oldKey, int newKey)
    {
        if (Plugin.improvedInputVersion < 2)
        {
            try
            {
                return GetOldImprovedInputKeyName(player, oldKey);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        try
        {
            return GetNewImprovedInputKeyName(player, newKey);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
    private static string GetOldImprovedInputKeyName(int player, string key)
    {
        try
        {
#pragma warning disable CS0618 // Type or member is obsolete
            return Translate(player, PlayerKeybind.Get(key).CurrentBinding(0));
#pragma warning restore CS0618 // Type or member is obsolete
        }
        catch (Exception e)
        {
            throw new Exception("ImprovedInput v1 not found!\n" + e.Message);
        }
    }
    private static string GetNewImprovedInputKeyName(int player, int key)
    {
        try
        {
            return ((PlayerKeybind)Plugin.improvedControls.GetValue(key)).CurrentBindingName(player);
        }
        catch (Exception e)
        {
            throw new Exception("ImprovedInput v2+ not found!\n" + e.Message);
        }
    }

    private static string Translate(int player, KeyCode key)
    {
        var text = key.ToString();
        if (text.Length > 14 && text.Substring(0, 14) == "JoystickButton" && int.TryParse(text.Substring(14, text.Length - 14), out int btn))
        {
            return ControllerButtonName(player, btn);
        }
        if (text.Length > 15 && text.Substring(0, 8) == "Joystick" && int.TryParse(text.Substring(15, text.Length - 15), out int btn2))
        {
            return ControllerButtonName(player, btn2);
        }
        if (KeyboardButtonName(key) is string name)
        {
            return name;
        }
        return text;
    }

    private static string ControllerButtonName(int player, int joystickButton)
    {
        // Thank the internet honestly. It's not like I knew these mappings before googling them
        // Gets whatever controller `player` is using and displays the button name for that controller
        Options.ControlSetup.Preset ty = Custom.rainWorld.options.controls[player].GetActivePreset();

        if (ty == Options.ControlSetup.Preset.XBox)
        {
            return joystickButton switch
            {
                0 => "A",
                1 => "B",
                2 => "X",
                3 => "Y",
                4 => "LB",
                5 => "RB",
                6 => "Menu",
                7 => "View",
                8 => "LSB",
                9 => "RSB",
                12 => "XBox",
                _ => $"Button {joystickButton}"
            };
        }
        else if (ty == Options.ControlSetup.Preset.PS4DualShock || ty == Options.ControlSetup.Preset.PS5DualSense)
        {
            return joystickButton switch
            {
                0 => "X",
                1 => "O",
                2 => "Square",
                3 => "Triangle",
                4 => "L1",
                5 => "R1",
                6 => "Share",
                7 => "Options",
                8 => "LSB",
                9 => "RSB",
                12 => "PS",
                13 => "Touchpad",
                _ => $"Button {joystickButton}"
            };
        }
        else if (ty == Options.ControlSetup.Preset.SwitchProController)
        {
            return joystickButton switch
            {
                0 => "B",
                1 => "A",
                2 => "Y",
                3 => "X",
                4 => "L",
                5 => "R",
                6 => "ZL",
                7 => "ZR",
                8 => "-",
                9 => "+",
                10 => "LSB",
                11 => "RSB",
                12 => "Home",
                13 => "Capture",
                _ => $"Button {joystickButton}"
            };
        }
        return "< NOT ASSIGNED! >";
    }

    private static string KeyboardButtonName(KeyCode kc)
    {
        string ret = kc switch
        {
            KeyCode.Period => ".",
            KeyCode.Comma => ",",
            KeyCode.Slash => "/",
            KeyCode.Backslash => "\\",
            KeyCode.LeftBracket => "[",
            KeyCode.RightBracket => "]",
            KeyCode.Minus => "-",
            KeyCode.Equals => "=",
            KeyCode.Plus => "+",
            KeyCode.BackQuote => "`",
            KeyCode.Semicolon => ";",
            KeyCode.Exclaim => "!",
            KeyCode.Question => "?",
            KeyCode.Dollar => "$",
            _ => null
        };
        if (ret == null)
        {
            if (kc.ToString().StartsWith("Left"))
            {
                return "L" + kc.ToString().Substring(4);
            }
            if (kc.ToString().StartsWith("Right"))
            {
                return "R" + kc.ToString().Substring(5);
            }
        }
        return ret;
    }
}

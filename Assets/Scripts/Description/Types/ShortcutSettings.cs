using UnityEngine;

namespace DLS.Description
{
    public struct ShortcutSettings
    {
        public Shortcut MainMenu_NewProjectShortcutTriggered;
        public Shortcut MainMenu_OpenProjectShortcutTriggered;
        public Shortcut MainMenu_SettingsShortcutTriggered;
        public Shortcut MainMenu_QuitShortcutTriggered;

        // ---- Bottom Bar Menu shortcuts ----
        public Shortcut SaveShortcutTriggered;
        public Shortcut LibraryShortcutTriggered;
        public Shortcut PreferencesShortcutTriggered;
        public Shortcut StatsShortcutTriggered;
        public Shortcut CreateNewChipShortcutTriggered;
        public Shortcut QuitToMainMenuShortcutTriggered;
        public Shortcut SearchShortcutTriggered;
        public Shortcut SpecialChipsShortcutTriggered;

        // ---- Misc shortcuts ----
        public Shortcut DuplicateShortcutTriggered;
        public Shortcut ToggleGridShortcutTriggered;
        public Shortcut ResetCameraShortcutTriggered;
        public Shortcut UndoShortcutTriggered;
        public Shortcut RedoShortcutTriggered;
        public Shortcut ModifierKeysOffToggleTriggered;

        // ---- Single key shortcuts ----
        public Shortcut CancelShortcutTriggered;
        public Shortcut ConfirmShortcutTriggered;
        public Shortcut DeleteShortcutTriggered;
        public Shortcut SimNextStepShortcutTriggered;
        public Shortcut SimPauseToggleShortcutTriggered;

        // ---- Dev shortcuts ----
        public Shortcut OpenSaveDataFolderShortcutTriggered;

        public static ShortcutSettings Default() =>
            new()
            {
                MainMenu_NewProjectShortcutTriggered = new(ShortcutModifier.Ctrl, KeyCode.N),
                MainMenu_OpenProjectShortcutTriggered = new(ShortcutModifier.Ctrl, KeyCode.O),
                MainMenu_SettingsShortcutTriggered = new(ShortcutModifier.Ctrl, KeyCode.S),
                MainMenu_QuitShortcutTriggered = new(ShortcutModifier.Ctrl, KeyCode.Q),

                SaveShortcutTriggered = new(ShortcutModifier.Ctrl, KeyCode.S),
                LibraryShortcutTriggered = new(ShortcutModifier.Ctrl, KeyCode.L),
                PreferencesShortcutTriggered = new(ShortcutModifier.Ctrl, KeyCode.P),
                StatsShortcutTriggered = new(ShortcutModifier.Ctrl, KeyCode.T),
                CreateNewChipShortcutTriggered = new(ShortcutModifier.Ctrl, KeyCode.N),
                QuitToMainMenuShortcutTriggered = new(ShortcutModifier.Ctrl, KeyCode.Q),
                SearchShortcutTriggered = new(ShortcutModifier.Ctrl, KeyCode.F),
                SpecialChipsShortcutTriggered = new(ShortcutModifier.Ctrl, KeyCode.B),

                DuplicateShortcutTriggered = new(ShortcutModifier.Alt, KeyCode.D, alternativeMod: ShortcutModifier.Ctrl),
                ToggleGridShortcutTriggered = new(ShortcutModifier.Ctrl, KeyCode.G),
                ResetCameraShortcutTriggered = new(ShortcutModifier.Ctrl, KeyCode.R),
                UndoShortcutTriggered = new(ShortcutModifier.Ctrl, KeyCode.Z),
                RedoShortcutTriggered = new(ShortcutModifier.CtrlAndShift, KeyCode.Z),
                ModifierKeysOffToggleTriggered = new(ShortcutModifier.RightAlt, KeyCode.F12),

                CancelShortcutTriggered = new(ShortcutModifier.None, KeyCode.Escape),
                ConfirmShortcutTriggered = new(ShortcutModifier.None, KeyCode.Return, alternativeKey: KeyCode.KeypadEnter),
                DeleteShortcutTriggered = new(ShortcutModifier.None, KeyCode.Backspace, alternativeKey: KeyCode.Delete),
                SimNextStepShortcutTriggered = new(ShortcutModifier.None, KeyCode.Space, forbiddenMod: ShortcutModifier.Ctrl),
                SimPauseToggleShortcutTriggered = new(ShortcutModifier.Ctrl, KeyCode.Space),

                OpenSaveDataFolderShortcutTriggered = new(ShortcutModifier.CtrlShiftAlt, KeyCode.O),
            };
    }

    public class Shortcut
    {
        public ShortcutModifier Modifier;
        public KeyCode KeyCode;
        public ShortcutModifier AlternativeModifier;
        public KeyCode AlternativeKeyCode;
        public ShortcutModifier ForbiddenModifier;

        public Shortcut(ShortcutModifier modifier, KeyCode key,
            ShortcutModifier alternativeMod = ShortcutModifier.None, KeyCode alternativeKey = KeyCode.None, ShortcutModifier forbiddenMod = ShortcutModifier.None)
        {
            Modifier = modifier;
            KeyCode = key;
            AlternativeModifier = alternativeMod;
            AlternativeKeyCode = alternativeKey;
            ForbiddenModifier = forbiddenMod;
        }

        public void Reassign(Shortcut shortcut)
        {
            Modifier = shortcut.Modifier;
            KeyCode = shortcut.KeyCode;
            AlternativeModifier = shortcut.AlternativeModifier;
            AlternativeKeyCode = shortcut.AlternativeKeyCode;
            ForbiddenModifier = shortcut.ForbiddenModifier;
        }

        public Shortcut Copy() {
            return new Shortcut(Modifier, KeyCode, AlternativeModifier, AlternativeKeyCode, ForbiddenModifier);
        }
    }

    public enum ShortcutModifier
    {
        None,
        Ctrl,
        Shift,
        Alt,
        CtrlAndShift,
        CtrlShiftAlt,
        RightAlt,
    }
}
using System;
using DLS.Description;
using Seb.Helpers;
using UnityEngine;

namespace DLS.Game
{
	public static class KeyboardShortcuts
	{
		// ---- Main Menu shortcuts
		public static Func<bool> MainMenu_NewProjectShortcutTriggered;
		public static Func<bool> MainMenu_OpenProjectShortcutTriggered;
		public static Func<bool> MainMenu_SettingsShortcutTriggered;
		public static Func<bool> MainMenu_QuitShortcutTriggered;

		// ---- Bottom Bar Menu shortcuts ----
		public static Func<bool> SaveShortcutTriggered;
		public static Func<bool> LibraryShortcutTriggered;
		public static Func<bool> PreferencesShortcutTriggered;
		public static Func<bool> StatsShortcutTriggered;
		public static Func<bool> CreateNewChipShortcutTriggered;
		public static Func<bool> QuitToMainMenuShortcutTriggered;
		public static Func<bool> SearchShortcutTriggered;
		public static Func<bool> SpecialChipsShortcutTriggered;

		// ---- Misc shortcuts ----
		public static Func<bool> DuplicateShortcutTriggered;
		public static Func<bool> ToggleGridShortcutTriggered;
		public static Func<bool> ResetCameraShortcutTriggered;
		public static Func<bool> UndoShortcutTriggered;
		public static Func<bool> RedoShortcutTriggered;

		// ---- Single key shortcuts ----
		public static Func<bool> CancelShortcutTriggered;
		public static Func<bool> ConfirmShortcutTriggered;
		public static Func<bool> DeleteShortcutTriggered;
		public static Func<bool> SimNextStepShortcutTriggered;
		public static Func<bool> SimPauseToggleShortcutTriggered;

		// ---- Dev shortcuts ----
		public static Func<bool> OpenSaveDataFolderShortcutTriggered;

		// ---- Modifiers ----
		public static bool SnapModeHeld => InputHelper.CtrlIsHeld;

		// In "Multi-mode", placed chips will be duplicated once placed to allow placing again; selecting a chip will add it to the current selection; etc.
		public static bool MultiModeHeld => InputHelper.AltIsHeld || InputHelper.ShiftIsHeld;
		public static bool StraightLineModeHeld => InputHelper.ShiftIsHeld;
		public static bool StraightLineModeTriggered => InputHelper.IsKeyDownThisFrame(KeyCode.LeftShift);
		public static bool CameraActionKeyHeld => InputHelper.AltIsHeld;
		public static bool TakeFirstFromCollectionModifierHeld => InputHelper.CtrlIsHeld || InputHelper.AltIsHeld || InputHelper.ShiftIsHeld;

		// ---- Helpers ----
		public static bool CtrlShortcutTriggered(KeyCode key) => InputHelper.IsKeyDownThisFrame(key) && InputHelper.CtrlIsHeld && !(InputHelper.AltIsHeld || InputHelper.ShiftIsHeld);
        public static bool CtrlShiftShortcutTriggered(KeyCode key) => InputHelper.IsKeyDownThisFrame(key) && InputHelper.CtrlIsHeld && InputHelper.ShiftIsHeld && !(InputHelper.AltIsHeld);
        public static bool ShiftShortcutTriggered(KeyCode key) => InputHelper.IsKeyDownThisFrame(key) && InputHelper.ShiftIsHeld && !(InputHelper.AltIsHeld || InputHelper.CtrlIsHeld);
		public static bool AltShortcutTriggered(KeyCode key) => InputHelper.IsKeyDownThisFrame(key) && InputHelper.AltIsHeld && !(InputHelper.CtrlIsHeld || InputHelper.ShiftIsHeld);
		public static bool CtrlShiftAltShortcutTriggered(KeyCode key) => InputHelper.IsKeyDownThisFrame(key) && InputHelper.CtrlIsHeld && InputHelper.AltIsHeld && InputHelper.ShiftIsHeld;



        public static void LoadShortcutSettings(ShortcutSettings shortcutSettings)
		{
			LoadShortcut(out MainMenu_NewProjectShortcutTriggered, shortcutSettings.MainMenu_NewProjectShortcutTriggered);
            LoadShortcut(out MainMenu_OpenProjectShortcutTriggered, shortcutSettings.MainMenu_OpenProjectShortcutTriggered);
            LoadShortcut(out MainMenu_SettingsShortcutTriggered, shortcutSettings.MainMenu_SettingsShortcutTriggered);
            LoadShortcut(out MainMenu_QuitShortcutTriggered, shortcutSettings.MainMenu_QuitShortcutTriggered);

            LoadShortcut(out SaveShortcutTriggered, shortcutSettings.SaveShortcutTriggered);
            LoadShortcut(out LibraryShortcutTriggered, shortcutSettings.LibraryShortcutTriggered);
            LoadShortcut(out PreferencesShortcutTriggered, shortcutSettings.PreferencesShortcutTriggered);
            LoadShortcut(out StatsShortcutTriggered, shortcutSettings.StatsShortcutTriggered);
            LoadShortcut(out CreateNewChipShortcutTriggered, shortcutSettings.CreateNewChipShortcutTriggered);
            LoadShortcut(out QuitToMainMenuShortcutTriggered, shortcutSettings.QuitToMainMenuShortcutTriggered);
            LoadShortcut(out SearchShortcutTriggered, shortcutSettings.SearchShortcutTriggered);
            LoadShortcut(out SpecialChipsShortcutTriggered, shortcutSettings.SpecialChipsShortcutTriggered);

            LoadShortcut(out DuplicateShortcutTriggered, shortcutSettings.DuplicateShortcutTriggered);
            LoadShortcut(out ToggleGridShortcutTriggered, shortcutSettings.ToggleGridShortcutTriggered);
            LoadShortcut(out ResetCameraShortcutTriggered, shortcutSettings.ResetCameraShortcutTriggered);
            LoadShortcut(out UndoShortcutTriggered, shortcutSettings.UndoShortcutTriggered);
            LoadShortcut(out RedoShortcutTriggered, shortcutSettings.RedoShortcutTriggered);

            LoadShortcut(out CancelShortcutTriggered, shortcutSettings.CancelShortcutTriggered);
            LoadShortcut(out ConfirmShortcutTriggered, shortcutSettings.ConfirmShortcutTriggered);
            LoadShortcut(out DeleteShortcutTriggered, shortcutSettings.DeleteShortcutTriggered);
            LoadShortcut(out SimNextStepShortcutTriggered, shortcutSettings.SimNextStepShortcutTriggered);
            LoadShortcut(out SimPauseToggleShortcutTriggered, shortcutSettings.SimPauseToggleShortcutTriggered);

            LoadShortcut(out OpenSaveDataFolderShortcutTriggered, shortcutSettings.OpenSaveDataFolderShortcutTriggered);
        }

        public static void LoadShortcut(out Func<bool> shortcutFunction, Shortcut shortcut)
		{
			if (shortcut.ForbiddenModifier == ShortcutModifier.None && shortcut.AlternativeKeyCode == KeyCode.None && shortcut.AlternativeModifier == ShortcutModifier.None)
			{
				LoadSimpleShortcut(out shortcutFunction, shortcut);
				return;
			}

			LoadComplexShortcut(out shortcutFunction, shortcut);
		}

		static void LoadSimpleShortcut(out Func<bool> shortcutFunction, Shortcut shortcut)
		{
			shortcutFunction = () => GetFuncFromShortcutModifier(shortcut.Modifier)() && InputHelper.IsKeyDownThisFrame(shortcut.KeyCode);
		}

		static void LoadComplexShortcut(out Func<bool> shortcutFunction, Shortcut shortcut)
		{
			Func<bool> modifier = GetFuncFromShortcutModifier(shortcut.Modifier);
			Func<bool> alternativeModifier = GetFuncFromShortcutModifier(shortcut.AlternativeModifier);
			Func<bool> forbiddenModifier = GetFuncFromShortcutModifier(shortcut.ForbiddenModifier);
			Func<bool> keys = () => InputHelper.IsKeyDownThisFrame(shortcut.KeyCode) && 
			(shortcut.AlternativeKeyCode == KeyCode.None ? true : InputHelper.IsKeyDownThisFrame(shortcut.AlternativeKeyCode));

			shortcutFunction = () => (modifier() || alternativeModifier()) && !forbiddenModifier() && keys();
		}

		static Func<bool> GetFuncFromShortcutModifier(ShortcutModifier modifier, bool isStandardModifier = true)
		{
			switch (modifier)
			{
				case ShortcutModifier.None:
					return () => isStandardModifier;
				case ShortcutModifier.Ctrl:
                    return () => InputHelper.CtrlIsHeld && !(InputHelper.ShiftIsHeld || InputHelper.AltIsHeld);
                case ShortcutModifier.Shift:
                    return () => InputHelper.ShiftIsHeld && !(InputHelper.CtrlIsHeld || InputHelper.AltIsHeld);
                case ShortcutModifier.Alt:
                    return () => InputHelper.AltIsHeld && !(InputHelper.CtrlIsHeld || InputHelper.ShiftIsHeld);
                case ShortcutModifier.CtrlAndShift:
                    return () => InputHelper.CtrlIsHeld && InputHelper.ShiftIsHeld && !InputHelper.AltIsHeld;
                case ShortcutModifier.CtrlShiftAlt:
					return () => InputHelper.CtrlIsHeld && InputHelper.ShiftIsHeld && InputHelper.AltIsHeld;
				default:
					return () => false;
			}
		}
	}
}
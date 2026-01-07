using DLS.Game;
using DLS.Simulation;
using Seb.Helpers;
using Seb.Vis;
using Seb.Vis.UI;
using UnityEngine;
using System.Linq;
using System;

namespace DLS.Graphics
{
	public static class RebindKeyChipMenu
	{
		static SubChipInstance keyChip;
		static string chosenKey;

		public static void DrawMenu()
		{
			MenuHelper.DrawBackgroundOverlay();
			Draw.ID panelID = UI.ReservePanel();
			DrawSettings.UIThemeDLS theme = DrawSettings.ActiveUITheme;

			Vector2 pos = UI.Centre + Vector2.up * (UI.HalfHeight * 0.25f);

			float nameUILengthMultiplier = 1.2f;

			using (UI.BeginBoundsScope(true))
			{
				if (InputHelper.AnyKeyOrMouseDownThisFrame && !string.IsNullOrEmpty(InputHelper.GetKeyCodePressedThisFrame().ToString()))
				{
					KeyCode activeKeyCode = InputHelper.GetKeyCodePressedThisFrame();

					if (InputHelper.ValidInputKeys.Contains(activeKeyCode))
					{
						// Convert KeyCode.ToString() to new name using KeyRenameNames
						string keyName = (string)typeof(InputHelper.KeyRenameNames).GetField(activeKeyCode.ToString())?.GetValue(null);

						chosenKey = keyName;
					}
				}

				UI.DrawText("Press a key to rebind\n (Alphanumeric, Symbols/Punctuation (Physical keys), and numpad keys only)", theme.FontBold, theme.FontSizeRegular, pos, Anchor.TextCentre, Color.white * 0.8f);

				float panelWidth = 3.5f + (chosenKey?.Length ?? 0) * nameUILengthMultiplier;

				UI.DrawPanel(UI.PrevBounds.CentreBottom + Vector2.down, new Vector2(panelWidth, 3.5f), new Color(0.1f, 0.1f, 0.1f), Anchor.CentreTop);
				UI.DrawText(chosenKey.ToString(), theme.FontBold, theme.FontSizeRegular * 1.5f, UI.PrevBounds.Centre, Anchor.TextCentre, Color.white);

				MenuHelper.CancelConfirmResult result = MenuHelper.DrawCancelConfirmButtons(UI.GetCurrentBoundsScope().BottomLeft, UI.GetCurrentBoundsScope().Width, true, false); // Can't use keybinds to select anymore
				MenuHelper.DrawReservedMenuPanel(panelID, UI.GetCurrentBoundsScope());

				if (result == MenuHelper.CancelConfirmResult.Cancel)
				{
					UIDrawer.SetActiveMenu(UIDrawer.MenuType.None);
				}
				else if (result == MenuHelper.CancelConfirmResult.Confirm)
				{
					Project.ActiveProject.NotifyKeyChipBindingChanged(keyChip, chosenKey.ToString());
					UIDrawer.SetActiveMenu(UIDrawer.MenuType.None);
				}
			}
		}

		public static void OnMenuOpened()
		{
			keyChip = (SubChipInstance)ContextMenu.interactionContext;
			// So we can get keycode from string
			if (Enum.TryParse<InputHelper.KeyNumberEnum>(keyChip.activationKeyString, out InputHelper.KeyNumberEnum parsedKey))
			{
				string keyName = parsedKey.ToString();
				KeyCode keyCode = (KeyCode)System.Enum.Parse(typeof(KeyCode), keyName);

				// Convert KeyCode.ToString() to new name using KeyRenameNames
				keyName = (string)typeof(InputHelper.KeyRenameNames).GetField(keyName)?.GetValue(null);
				
				chosenKey = keyName;
			}
		}
	}
}
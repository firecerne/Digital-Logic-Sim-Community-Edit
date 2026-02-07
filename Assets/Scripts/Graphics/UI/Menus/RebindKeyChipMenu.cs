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
		static KeyCode chosenKey;

		public static void DrawMenu()
		{
			MenuHelper.DrawBackgroundOverlay();
			Draw.ID panelID = UI.ReservePanel();
			DrawSettings.UIThemeDLS theme = DrawSettings.ActiveUITheme;

			Vector2 pos = UI.Centre + Vector2.up * (UI.HalfHeight * 0.25f);

			float nameUILengthMultiplier = 1.2f;

			using (UI.BeginBoundsScope(true))
			{
				// Draw the text instructions
				UI.DrawText("Press a key to rebind\n (Alphanumeric, Symbols/Punctuation (Physical keys), and numpad keys only)", theme.FontBold, theme.FontSizeRegular, pos, Anchor.TextCentre, Color.white * 0.8f);

				string chosenKeyName = InputHelper.UintToKeyName((uint)chosenKey);

				float panelWidth = 3.5f + (chosenKeyName?.Length ?? 0) * nameUILengthMultiplier;

				UI.DrawPanel(UI.PrevBounds.CentreBottom + Vector2.down, new Vector2(panelWidth, 3.5f), new Color(0.1f, 0.1f, 0.1f), Anchor.CentreTop);
				UI.DrawText(chosenKeyName, theme.FontBold, theme.FontSizeRegular * 1.5f, UI.PrevBounds.Centre, Anchor.TextCentre, Color.white);

				MenuHelper.CancelConfirmResult result = MenuHelper.DrawCancelConfirmButtons(UI.GetCurrentBoundsScope().BottomLeft, UI.GetCurrentBoundsScope().Width, true, false); // Can't use keybinds to select anymore
				
				// Check if hovering over buttons
				bool hoveringOverButtons = UI.MouseInsideBounds(UI.PrevBounds);
				
				MenuHelper.DrawReservedMenuPanel(panelID, UI.GetCurrentBoundsScope());

				// Draw no key pressed option (and check if clicked)
				bool noKeyButtonUsed = false;
				if (MenuHelper.DrawButton("No Key", new Vector2(UI.Centre.x - 2f, UI.GetCurrentBoundsScope().Bottom + 1f), 7f, true))
				{
					chosenKey = KeyCode.None;
					noKeyButtonUsed = true;
				}

				hoveringOverButtons = hoveringOverButtons || UI.MouseInsideBounds(UI.PrevBounds);

				// Check if scrolling
				float scrollInput = Input.GetAxis("Mouse ScrollWheel");
				bool scrolling = !(scrollInput == 0f);

				// Check for key input or scrolling only if not hovering over buttons
				if ((InputHelper.AnyKeyOrMouseDownThisFrame && (!string.IsNullOrEmpty(InputHelper.GetKeyCodePressedThisFrame().ToString())) || scrolling) && !hoveringOverButtons && !noKeyButtonUsed)
				{
					if (scrolling)
					{
						// turns 0 into 99998, -[any number] into 99997, and positive [any number] into 99999
						uint scrollType = 99998;
						if (scrollInput < 0f)
						{
							scrollType = 99997;
						}
						else if (scrollInput > 0f)
						{
							scrollType = 99999;
						}

						if (scrollType != 99998) chosenKey = (KeyCode)scrollType;
					}
					else
					{
						KeyCode activeKeyCode = InputHelper.GetKeyCodePressedThisFrame();

						if (InputHelper.ValidInputKeys.Contains(activeKeyCode))
						{
							chosenKey = activeKeyCode;
						}
					}
				}

				if (result == MenuHelper.CancelConfirmResult.Cancel)
				{
					UIDrawer.SetActiveMenu(UIDrawer.MenuType.None);
				}
				else if (result == MenuHelper.CancelConfirmResult.Confirm)
				{
					Project.ActiveProject.NotifyKeyChipBindingChanged(keyChip, chosenKey);
					UIDrawer.SetActiveMenu(UIDrawer.MenuType.None);
				}
			}
		}
		
		public static void OnMenuOpened()
		{
			keyChip = (SubChipInstance)ContextMenu.interactionContext;

			chosenKey = (KeyCode)keyChip.InternalData[0];
		}
	}
}
using System;
using DLS.Game;
using Seb.Helpers;
using Seb.Vis;
using Seb.Vis.UI;
using UnityEngine;

namespace DLS.Graphics
{
	public static class ConstantEditMenu
	{
		static SubChipInstance constantChip;
		static byte value;

		static readonly UIHandle ID_ValueInput = new("ConstantChipEdit_Value");
		static readonly Func<string, bool> integerInputValidator = ValidateValueInput;

		public static void DrawMenu()
		{
			MenuHelper.DrawBackgroundOverlay();
			Draw.ID panelID = UI.ReservePanel();
			DrawSettings.UIThemeDLS theme = DrawSettings.ActiveUITheme;

			Vector2 pos = UI.Centre + Vector2.up * (UI.HalfHeight * 0.25f);

			using (UI.BeginBoundsScope(true))
			{
				UI.DrawText("Value of Constant", theme.FontBold, theme.FontSizeRegular, pos, Anchor.TextCentre, Color.white * 0.8f);

				InputFieldTheme inputFieldTheme = DrawSettings.ActiveUITheme.ChipNameInputField;
				inputFieldTheme.fontSize = DrawSettings.ActiveUITheme.FontSizeRegular;

				Vector2 size = new(5.6f, DrawSettings.SelectorWheelHeight);
				Vector2 inputPos = UI.PrevBounds.CentreBottom + Vector2.down * DrawSettings.VerticalButtonSpacing;
				InputFieldState state = UI.InputField(ID_ValueInput, inputFieldTheme, inputPos, size, "0", Anchor.CentreTop, 1, integerInputValidator, forceFocus: true);
				short tempValue;
				if (state.text.Equals("-")) tempValue = 0;
				else short.TryParse(state.text, out tempValue);
				value = (byte)tempValue;

				MenuHelper.CancelConfirmResult result = MenuHelper.DrawCancelConfirmButtons(UI.GetCurrentBoundsScope().BottomLeft, UI.GetCurrentBoundsScope().Width, true);
				MenuHelper.DrawReservedMenuPanel(panelID, UI.GetCurrentBoundsScope());

				if (result == MenuHelper.CancelConfirmResult.Cancel)
				{
					UIDrawer.SetActiveMenu(UIDrawer.MenuType.None);
				}
				else if (result == MenuHelper.CancelConfirmResult.Confirm)
				{
					Project.ActiveProject.NotifyConstantEdited(constantChip, value);
					UIDrawer.SetActiveMenu(UIDrawer.MenuType.None);
				}
			}
		}

		public static void OnMenuOpened()
		{
			constantChip = (SubChipInstance)ContextMenu.interactionContext;
			value = (byte)constantChip.InternalData[0];
			UI.GetInputFieldState(ID_ValueInput).SetText(value.ToString());
		}

		public static bool ValidateValueInput(string s)
		{
			Debug.Log(s);
			if (s.Length > 4) return false;
			if (string.IsNullOrEmpty(s)) return true;
			if (s.Contains(" ")) return false;
			if (s.Equals("-")) return true;
			return short.TryParse(s, out _);
		}
	}
}
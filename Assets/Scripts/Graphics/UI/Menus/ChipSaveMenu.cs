using System;
using DLS.Description;
using DLS.Game;
using DLS.SaveSystem;
using Seb.Types;
using Seb.Vis;
using Seb.Vis.UI;
using UnityEngine;

namespace DLS.Graphics
{
	public static class ChipSaveMenu
	{
		const string MaxLengthChipName = "MY VERY LONG CHIP NAME";

		const int CancelButtonIndex = 0;
		const int CustomizeButtonIndex = 1;
		const int SaveButtonIndex = 2;
		const int SaveAsButtonIndex = 3;
		static readonly UIHandle ID_ChipNameField = new("SaveMenu_ChipNameField");
		static readonly Func<string, bool> chipNameValidator = ValidateChipNameInput;

		static ChipDescription CustomizeStateBeforeEnteringCustomizeMenu;

		static readonly string[] CancelSaveButtonNames =
		{
			"CANCEL", "CUSTOMIZE", "SAVE"
		};

		static readonly string[] CancelRenameSaveButtonNames =
		{
			"CANCEL", "CUSTOMIZE", "RENAME", "SAVE AS"
		};

		static readonly bool[] ButtonGroupInteractStates = { true, true, true, true };
		public static ChipDescription ActiveCustomizeDescription;

		public static void OnMenuOpened()
		{
			ActiveCustomizeDescription ??= CreateCustomizationState();
			InitUIFromDescription(ActiveCustomizeDescription);
		}

		public static (Vector2 size, float pad) GetTextInputSize()
		{
			const float textPad = 2;
			InputFieldTheme inputTheme = DrawSettings.ActiveUITheme.ChipNameInputField;
			Vector2 inputFieldSize = UI.CalculateTextSize(MaxLengthChipName, inputTheme.fontSize, inputTheme.font) + new Vector2(textPad * 2, 3);
			return (inputFieldSize, textPad);
		}

		public static void DrawMenu()
		{
			MenuHelper.DrawBackgroundOverlay();

			DrawSettings.UIThemeDLS theme = DrawSettings.ActiveUITheme;
			InputFieldTheme inputTheme = DrawSettings.ActiveUITheme.ChipNameInputField;

			using (UI.BeginBoundsScope(true))
			{
				Draw.ID panelID = UI.ReservePanel();

				// -- Chip name input field --
				(Vector2 inputFieldSize, float inputFieldTextPad) = GetTextInputSize();
				var inputFieldState = UI.InputField(ID_ChipNameField, inputTheme, new Vector2(50, 33), inputFieldSize, "Name", Anchor.Centre, inputFieldTextPad, chipNameValidator, true);

				Vector2 buttonTopLeft = UI.PrevBounds.BottomLeft + Vector2.down * (DrawSettings.DefaultButtonSpacing * 2);
				bool renaming = Project.ActiveProject.ChipHasBeenSavedBefore && !ChipDescription.NameMatch(inputFieldState.text, Project.ActiveProject.ViewedChip.LastSavedDescription.Name);

				bool saveButtonEnabled = IsValidSaveName(inputFieldState.text);
				ButtonGroupInteractStates[SaveButtonIndex] = saveButtonEnabled;
				ButtonGroupInteractStates[SaveAsButtonIndex] = saveButtonEnabled;
				string[] buttonGroupNames = renaming ? CancelRenameSaveButtonNames : CancelSaveButtonNames;
				int buttonIndex = UI.HorizontalButtonGroup(buttonGroupNames, ButtonGroupInteractStates, theme.ButtonTheme, buttonTopLeft, UI.PrevBounds.Width, DrawSettings.DefaultButtonSpacing, 0, Anchor.TopLeft);
				bool confirmShortcut = !renaming && KeyboardShortcuts.ConfirmShortcutTriggered();

				if (buttonIndex == CancelButtonIndex || KeyboardShortcuts.CancelShortcutTriggered())
				{
					Cancel();
					return;
				}
				if (buttonIndex == CustomizeButtonIndex)
				{
					OpenCustomizationMenu();
					return;
				}
				if (buttonIndex == SaveButtonIndex || confirmShortcut)
				{
					Save(renaming ? Project.SaveMode.Rename : Project.SaveMode.Normal);
					return;
				}
				if (buttonIndex == SaveAsButtonIndex)
				{
					Save(Project.SaveMode.SaveAs);
					return;
				}

				Bounds2D uiBounds = UI.GetCurrentBoundsScope();
				MenuHelper.DrawReservedMenuPanel(panelID, uiBounds);

				// Update customization state
				string newName = inputFieldState.text;
				if (ActiveCustomizeDescription.Name != newName)
				{
					ActiveCustomizeDescription.Name = newName;
					Vector2 minChipSize = SubChipInstance.CalculateMinChipSize(ActiveCustomizeDescription.InputPins, ActiveCustomizeDescription.OutputPins, newName);
					Vector2 chipSizeNew = Vector2.Max(minChipSize, ActiveCustomizeDescription.Size);
					ActiveCustomizeDescription.Size = chipSizeNew;
				}
			}
		}

        // Create a subChip instance based on the current dev chip (we need a subChip instance to be able to draw a preview of the chip in the customization menu)
        // The description on this subChip holds potential customizations, such as name changes, resizing, colour etc.
        static ChipDescription CreateCustomizationState()
        {
	        return DescriptionCreator.CreateChipDescription(Project.ActiveProject.ViewedChip);
        }

        static void OpenCustomizationMenu()
		{
			CustomizeStateBeforeEnteringCustomizeMenu = Saver.CloneChipDescription(ActiveCustomizeDescription);
			UIDrawer.SetActiveMenu(UIDrawer.MenuType.ChipCustomization);
		}

		public static void RevertCustomizationStateToBeforeEnteringCustomizeMenu()
		{
			ActiveCustomizeDescription = CustomizeStateBeforeEnteringCustomizeMenu;
		}

		public static bool ValidateChipNameInput(string nameInput) => nameInput.Length <= MaxLengthChipName.Length && !SaveUtils.NameContainsForbiddenChar(nameInput);

		static bool IsValidSaveName(string chipName)
		{
			Project project = Project.ActiveProject;

			bool validName = !string.IsNullOrWhiteSpace(chipName) && SaveUtils.ValidFileName(chipName);
			bool nameAlreadyUsed = project.chipLibrary.HasChip(chipName);
			bool isNameOfActiveChip = ChipDescription.NameMatch(project.ActiveDevChipName, chipName);

			bool isValid = validName && (!nameAlreadyUsed || isNameOfActiveChip);

			return isValid;
		}

		static void InitUIFromDescription(ChipDescription chipDesc)
		{
			// Set input field to current chip name
			InputFieldState inputFieldState = UI.GetInputFieldState(ID_ChipNameField);
			inputFieldState.SetText(chipDesc.Name);
		}

		static void Save(Project.SaveMode mode)
		{
			Project.ActiveProject.SaveFromDescription(ActiveCustomizeDescription, mode);
			CloseMenu();
		}

		static void Cancel()
		{
			CloseMenu();
		}

		static void CloseMenu()
		{
			Reset();
			UIDrawer.SetActiveMenu(UIDrawer.MenuType.None);
		}

		public static void Reset()
		{
			ActiveCustomizeDescription = null;
			CustomizeStateBeforeEnteringCustomizeMenu = null;
		}
	}
}
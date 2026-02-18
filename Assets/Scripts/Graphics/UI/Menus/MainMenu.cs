using System;
using System.IO.Compression;
using System.Linq;
using System.Linq.Expressions;
using DLS.Description;
using DLS.Game;
using DLS.SaveSystem;
using DLS.Simulation;
using Seb.Helpers;
using Seb.Vis;
using Seb.Vis.UI;
using UnityEngine;
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
using System.Windows.Forms;
#endif

namespace DLS.Graphics
{
	public static class MainMenu
	{
		public const int MaxProjectNameLength = 20;
		const bool capitalize = true;

		static MenuScreen activeMenuScreen = MenuScreen.Main;
		static PopupKind activePopup = PopupKind.None;
		static AppSettings EditedAppSettings;

		static readonly UIHandle ID_ProjectNameInput = new("MainMenu_ProjectNameInputField");
		static readonly UIHandle ID_DisplayResolutionWheel = new("MainMenu_DisplayResolutionWheel");
		static readonly UIHandle ID_FullscreenWheel = new("MainMenu_FullscreenWheel");
		static readonly UIHandle ID_ProjectsScrollView = new("MainMenu_ProjectsScrollView");
		static readonly UIHandle ID_ManualImportPathInput = new("MainMenu_ManualImportPathInput");

		static readonly string[] SettingsWheelFullScreenOptions = { "OFF", "MAXIMIZED", "BORDERLESS", "EXCLUSIVE" };
		static readonly FullScreenMode[] FullScreenModes = { FullScreenMode.Windowed, FullScreenMode.MaximizedWindow, FullScreenMode.FullScreenWindow, FullScreenMode.ExclusiveFullScreen };
		static readonly string[] SettingsWheelVSyncOptions = { "DISABLED", "ENABLED" };

		static readonly Func<string, bool> projectNameValidator = ProjectNameValidator;
		static readonly UI.ScrollViewDrawContentFunc loadProjectScrollViewDrawer = DrawAllProjectsInScrollView;


		static readonly string[] menuButtonNames =
		{
			FormatButtonString("New Project"),
			FormatButtonString("Open Project"),
			FormatButtonString("Settings"),
			FormatButtonString("About"),
			FormatButtonString("Quit")
		};

		static readonly string[] openProjectButtonNames =
		{
			FormatButtonString("Back"),
			FormatButtonString("Delete"),
			FormatButtonString("Duplicate"),
			FormatButtonString("Rename"),
			FormatButtonString("Import"),
			FormatButtonString("Export"),
			FormatButtonString("Open")
		};

		static readonly Vector2Int[] Resolutions =
		{
			new(960, 540),
			new(1280, 720),
			new(1920, 1080),
			new(2560, 1440)
		};

		static readonly string[] ResolutionNames = Resolutions.Select(r => ResolutionToString(r)).ToArray();
		static readonly string[] FullScreenResName = Resolutions.Select(r => ResolutionToString(Main.FullScreenResolution)).ToArray();
		static readonly string[] settingsButtonGroupNames = { "EXIT", "APPLY" };
		static readonly bool[] settingsButtonGroupStates = new bool[settingsButtonGroupNames.Length];

		static readonly bool[] openProjectButtonStates = new bool[openProjectButtonNames.Length];

		static ProjectDescription[] allProjectDescriptions;
		static string[] allProjectNames;
		static (bool compatible, string message)[] projectCompatibilities;

		static int selectedProjectIndex;

		static readonly string authorString = "Created by: Sebastian Lague";
		static readonly string versionString = $"Version: {Main.DLSVersion} ({Main.LastUpdatedString})";
		static readonly string moddedString = $"ComEdit Version : {Main.DLSVersion_ModdedID} ({Main.LastUpdatedModdedString})";
		static string SelectedProjectName => allProjectDescriptions[selectedProjectIndex].ProjectName;

		static string FormatButtonString(string s) => capitalize ? s.ToUpper() : s;

		public static void Draw()
		{
			Simulator.UpdateInPausedState();
			
			if (KeyboardShortcuts.CancelShortcutTriggered() && activePopup == PopupKind.None)
			{
				BackToMain();
			}

			UI.DrawFullscreenPanel(ColHelper.MakeCol255(47, 47, 53));
			const string title = "DIGITAL LOGIC SIM";
			const float titleFontSize = 11.5f;
			const float titleHeight = 24;
			const float shaddowOffset = -0.33f;
			Color shadowCol = ColHelper.MakeCol255(87, 94, 230);

			UI.DrawText(title, FontType.Born2bSporty, titleFontSize, UI.Centre + Vector2.up * (titleHeight + shaddowOffset), Anchor.CentreTop, shadowCol);
			UI.DrawText(title, FontType.Born2bSporty, titleFontSize, UI.Centre + Vector2.up * titleHeight, Anchor.CentreTop, Color.white);
			DrawVersionInfo();

			switch (activeMenuScreen)
			{
				case MenuScreen.Main:
					DrawMainScreen();
					break;
				case MenuScreen.LoadProject:
					DrawLoadProjectScreen();
					break;
				case MenuScreen.Settings:
					DrawSettingsScreen();
					break;
				case MenuScreen.About:
					DrawAboutScreen();
					break;
				case MenuScreen.Controls:
					DrawControlsScreen();
					break;

			}

			switch (activePopup)
			{
				case PopupKind.DeleteConfirmation:
					DrawDeleteProjectConfirmationPopup();
					break;
				case PopupKind.NamePopup_RenameProject:
					DrawNamePopup();
					break;
				case PopupKind.NamePopup_DuplicateProject:
					DrawNamePopup();
					break;
				case PopupKind.NamePopup_NewProject:
					DrawNamePopup();
					break;
				case PopupKind.ZipPopup_ImportProject:
					DrawImportProjectPopup();
					break;
				case PopupKind.ZipPopup_ExportProject:
					DrawExportProjectPopup();
					break;
				case PopupKind.ZipPopup_ManualImportPath:
					DrawManualImportPathPopup();
					break;
			}
		}

		public static void OnMenuOpened()
		{
			activeMenuScreen = MenuScreen.Main;
			activePopup = PopupKind.None;
			selectedProjectIndex = -1;
			RefreshLoadedProjects();
		}

		static void DrawMainScreen()
		{
			if (activePopup != PopupKind.None) return;

			DrawSettings.UIThemeDLS theme = DrawSettings.ActiveUITheme;
			float buttonWidth = 15;

			int buttonIndex = UI.VerticalButtonGroup(menuButtonNames, theme.MainMenuButtonTheme, UI.Centre + Vector2.up * 6, new Vector2(buttonWidth, 0), false, true, 1);

			if (buttonIndex == 0 || KeyboardShortcuts.MainMenu_NewProjectShortcutTriggered()) // New project
			{
				RefreshLoadedProjects();
				activePopup = PopupKind.NamePopup_NewProject;
			}
			else if (buttonIndex == 1 || KeyboardShortcuts.MainMenu_OpenProjectShortcutTriggered()) // Load project
			{
				RefreshLoadedProjects();
				selectedProjectIndex = -1;
				activeMenuScreen = MenuScreen.LoadProject;
			}
			else if (buttonIndex == 2 || KeyboardShortcuts.MainMenu_SettingsShortcutTriggered()) // Settings
			{
				EditedAppSettings = Main.ActiveAppSettings;
				activeMenuScreen = MenuScreen.Settings;
				OnSettingsMenuOpened();
			}
			else if (buttonIndex == 3) // About
			{
				activeMenuScreen = MenuScreen.About;
			}
			else if (buttonIndex == 4 || KeyboardShortcuts.MainMenu_QuitShortcutTriggered()) // Quit
			{
				Quit();
			}
		}

		static void DrawLoadProjectScreen()
		{
			// Ensure project data is loaded when drawing
			if (allProjectDescriptions == null)
			{
				RefreshLoadedProjects();
			}
			
			const int backButtonIndex = 0;
			const int deleteButtonIndex = 1;
			const int duplicateButtonIndex = 2;
			const int renameButtonIndex = 3;
			const int importButtonIndex = 4;
			const int exportButtonIndex = 5;
			const int openButtonIndex = 6;
			DrawSettings.UIThemeDLS theme = DrawSettings.ActiveUITheme;

			Vector2 pos = UI.Centre + new Vector2(0, -1);
			Vector2 size = new(68, 32);

			UI.DrawScrollView(ID_ProjectsScrollView, pos, size, Anchor.Centre, theme.ScrollTheme, loadProjectScrollViewDrawer);
			ButtonTheme buttonTheme = DrawSettings.ActiveUITheme.MainMenuButtonTheme;

			bool projectSelected = selectedProjectIndex >= 0 && selectedProjectIndex < allProjectDescriptions.Length;
			bool compatibleProject = projectSelected && selectedProjectIndex < projectCompatibilities.Length && projectCompatibilities[selectedProjectIndex].compatible;

			for (int i = 0; i < openProjectButtonStates.Length; i++)
			{
				bool buttonEnabled = activePopup == PopupKind.None && (compatibleProject || i == backButtonIndex || (i == deleteButtonIndex && projectSelected) || i == importButtonIndex || (i == exportButtonIndex && projectSelected));
				openProjectButtonStates[i] = buttonEnabled;
			}

			Vector2 buttonRegionPos = UI.PrevBounds.BottomLeft + Vector2.down * DrawSettings.VerticalButtonSpacing;
			int buttonIndex = UI.HorizontalButtonGroup(openProjectButtonNames, openProjectButtonStates, buttonTheme, buttonRegionPos, UI.PrevBounds.Width, UILayoutHelper.DefaultSpacing, 0, Anchor.TopLeft);

			if (projectSelected && !compatibleProject && selectedProjectIndex < projectCompatibilities.Length)
			{
				Vector2 errorMessagePos = UI.PrevBounds.BottomLeft + Vector2.down * (DrawSettings.DefaultButtonSpacing * 2);
				UI.DrawText(projectCompatibilities[selectedProjectIndex].message, buttonTheme.font, buttonTheme.fontSize, errorMessagePos, Anchor.TopLeft, Color.yellow);
			}

			// ---- Handle button input ----
			if (buttonIndex == backButtonIndex) BackToMain();
			else if (buttonIndex == deleteButtonIndex) activePopup = PopupKind.DeleteConfirmation;
			else if (buttonIndex == duplicateButtonIndex) activePopup = PopupKind.NamePopup_DuplicateProject;
			else if (buttonIndex == renameButtonIndex) activePopup = PopupKind.NamePopup_RenameProject;
			else if (buttonIndex == importButtonIndex) activePopup = PopupKind.ZipPopup_ImportProject;
			else if (buttonIndex == exportButtonIndex) activePopup = PopupKind.ZipPopup_ExportProject;
			else if (buttonIndex == openButtonIndex) Main.CreateOrLoadProject(SelectedProjectName, string.Empty);
		}

		static bool ProjectNameValidator(string inputString) => inputString.Length <= 20 && !SaveUtils.NameContainsForbiddenChar(inputString);

		static void DrawAllProjectsInScrollView(Vector2 topLeft, float width, bool isLayoutPass)
		{
			// Ensure project data is loaded
			if (allProjectDescriptions == null || allProjectNames == null || projectCompatibilities == null)
			{
				RefreshLoadedProjects();
			}
			
			// Always draw content, even if empty
			if (allProjectDescriptions == null || allProjectDescriptions.Length == 0)
			{
				// Show "No projects found" message
				if (!isLayoutPass)
				{
					DrawSettings.UIThemeDLS theme = DrawSettings.ActiveUITheme;
					UI.DrawText("No projects found", theme.FontRegular, theme.FontSizeRegular, 
						topLeft + new Vector2(width * 0.5f, 5), Anchor.CentreTop, 
						new Color(1f, 1f, 1f, 0.5f));
				}
				return;
			}

			float spacing = 0;
			bool enabled = activePopup == PopupKind.None;

			for (int i = 0; i < allProjectDescriptions.Length; i++)
			{
				ProjectDescription desc = allProjectDescriptions[i];
				bool selected = i == selectedProjectIndex;
				ButtonTheme buttonTheme = selected ? DrawSettings.ActiveUITheme.ProjectSelectionButtonSelected : DrawSettings.ActiveUITheme.ProjectSelectionButton;
				if (i < projectCompatibilities.Length && !projectCompatibilities[i].compatible) buttonTheme.textCols.normal.a = 0.5f;

				if (UI.Button(desc.ProjectName, buttonTheme, topLeft, new Vector2(width, 0), enabled, false, true, Anchor.TopLeft))
				{
					selectedProjectIndex = i;
				}

				topLeft = UI.PrevBounds.BottomLeft + Vector2.down * spacing;
			}
		}


		static void RefreshLoadedProjects()
		{
			try
			{
				Debug.Log("Refreshing project list...");
				allProjectDescriptions = Loader.LoadAllProjectDescriptions();
				if (allProjectDescriptions == null)
				{
					Debug.LogWarning("LoadAllProjectDescriptions returned null, creating empty array");
					allProjectDescriptions = new ProjectDescription[0];
				}
				else
				{
					Debug.Log($"Loaded {allProjectDescriptions.Length} project descriptions");
				}
				
				allProjectNames = allProjectDescriptions.Select(d => d.ProjectName).ToArray();
				projectCompatibilities = allProjectDescriptions.Select(d => CanOpenProject(d)).ToArray();
				
				// Ensure selected index is valid
				if (selectedProjectIndex >= allProjectDescriptions.Length)
				{
					selectedProjectIndex = -1;
				}
			}
			catch (System.Exception ex)
			{
				Debug.LogError($"Failed to load projects: {ex.Message}");
				allProjectDescriptions = new ProjectDescription[0];
				allProjectNames = new string[0];
				projectCompatibilities = new (bool compatible, string message)[0];
				selectedProjectIndex = -1;
			}
		}

		static (bool canOpen, string failureReason) CanOpenProject(ProjectDescription projectDescription)
		{
			try
			{
				Main.Version earliestCompatible = Main.Version.Parse(projectDescription.DLSVersion_EarliestCompatible);
				Main.Version currentVersion = Main.DLSVersion;

				// In case project was made with a newer version of the sim, check if this version is able to open it
				bool canOpen = currentVersion.ToInt() >= earliestCompatible.ToInt();
				string failureReason = canOpen ? string.Empty : $"This project requires version {earliestCompatible} or later.";
				return (canOpen, failureReason);
			}
			catch
			{
				Debug.Log("Incompatible project: " + projectDescription.ProjectName);
				return (false, "Unrecognized project format");
			}
		}

		public static void BackToMain()
		{
			UI.GetInputFieldState(ID_ProjectNameInput).ClearText();
			UI.GetInputFieldState(ID_ManualImportPathInput).ClearText();
			activeMenuScreen = MenuScreen.Main;
			activePopup = PopupKind.None;
			RefreshLoadedProjects();
		}


		static void OnSettingsMenuOpened()
		{
			// Automatically select whichever resolution option is closest to current window size
			WheelSelectorState resolutionWheelState = UI.GetWheelSelectorState(ID_DisplayResolutionWheel);
			int closestMatchError = int.MaxValue;
			for (int i = 0; i < Resolutions.Length; i++)
			{
				int matchError = Mathf.Min(Mathf.Abs(Screen.width - Resolutions[i].x), Mathf.Abs(Screen.height - Resolutions[i].y));
				if (matchError < closestMatchError)
				{
					closestMatchError = matchError;
					resolutionWheelState.index = i;
				}
			}

			// Automatically set curr fullscreen mode
			WheelSelectorState fullscreenWheelState = UI.GetWheelSelectorState(ID_FullscreenWheel);
			for (int i = 0; i < FullScreenModes.Length; i++)
			{
				if (Screen.fullScreenMode == FullScreenModes[i])
				{
					fullscreenWheelState.index = i;
					break;
				}
			}
		}

		static void DrawSettingsScreen()
		{
			DrawSettings.UIThemeDLS theme = DrawSettings.ActiveUITheme;

			float regionWidth = 30;
			float labelOriginLeft = UI.Centre.x - regionWidth / 2;
			float elementOriginRight = UI.Centre.x + regionWidth / 2;
			Vector2 wheelSize = new(16, 2.5f);
			Vector2 pos = new(labelOriginLeft, UI.Centre.y + 4);


            using (UI.BeginBoundsScope(true))
			{
				Draw.ID backgroundPanelID = UI.ReservePanel();

				// -- Resolution --
				bool resEnabled = EditedAppSettings.fullscreenMode == FullScreenMode.Windowed;
				UI.DrawText("Resolution", theme.FontRegular, theme.FontSizeRegular, pos, Anchor.CentreLeft, Color.white);
				string[] resNames = resEnabled ? ResolutionNames : FullScreenResName;
				int resIndex = UI.WheelSelector(ID_DisplayResolutionWheel, resNames, new Vector2(elementOriginRight, pos.y), wheelSize, theme.OptionsWheel, Anchor.CentreRight, enabled: resEnabled);
				EditedAppSettings.ResolutionX = Resolutions[resIndex].x;
				EditedAppSettings.ResolutionY = Resolutions[resIndex].y;

				// -- Full screen --
				pos += Vector2.down * 4;
				UI.DrawText("Fullscreen", theme.FontRegular, theme.FontSizeRegular, pos, Anchor.CentreLeft, Color.white);
				int fullScreenSettingIndex = UI.WheelSelector(ID_FullscreenWheel, SettingsWheelFullScreenOptions, new Vector2(elementOriginRight, pos.y), wheelSize, theme.OptionsWheel, Anchor.CentreRight);
				EditedAppSettings.fullscreenMode = FullScreenModes[fullScreenSettingIndex];
				pos += Vector2.down * 4;

				// -- Vsync --
				UI.DrawText("VSync", theme.FontRegular, theme.FontSizeRegular, pos, Anchor.CentreLeft, Color.white);
				int vsyncSetting = UI.WheelSelector(EditedAppSettings.VSyncEnabled ? 1 : 0, SettingsWheelVSyncOptions, new Vector2(elementOriginRight, pos.y), wheelSize, theme.OptionsWheel, Anchor.CentreRight);
				EditedAppSettings.VSyncEnabled = vsyncSetting == 1;

				// Background panel
				UI.ModifyPanel(backgroundPanelID, UI.GetCurrentBoundsScope().Centre, UI.GetCurrentBoundsScope().Size + Vector2.one * 3, ColHelper.MakeCol255(37, 37, 43));
			}


            Vector2 buttonPos = UI.PrevBounds.BottomLeft + Vector2.down * DrawSettings.VerticalButtonSpacing;
			settingsButtonGroupStates[0] = true;
			settingsButtonGroupStates[1] = true;

			int buttonIndex = UI.HorizontalButtonGroup(settingsButtonGroupNames, settingsButtonGroupStates, theme.MainMenuButtonTheme, buttonPos, UI.PrevBounds.Width, UILayoutHelper.DefaultSpacing, 0, Anchor.TopLeft);

			if (buttonIndex == 0)
			{
				BackToMain();
			}
			else if (buttonIndex == 1)
			{
				Main.SaveAndApplyAppSettings(EditedAppSettings);
			}

			Vector2 controlButtonPos = UI.PrevBounds.BottomLeft + Vector2.down * DrawSettings.VerticalButtonSpacing + Vector2.right * UI.PrevBounds.Width * 0.5f ;
            bool controlButtonClicked = UI.Button("CONTROLS", theme.MainMenuButtonTheme, controlButtonPos, anchor:Anchor.CentreTop, size: new(UI.PrevBounds.Width * 0.33f, 0f));

			if (controlButtonClicked) {
				activeMenuScreen = MenuScreen.Controls;
				OnControlsScreenOpen();
			}
        }

		static void DrawControlsScreen()
		{
			ControlsScreen.DrawControlsScreen();
		}

		static void OnControlsScreenOpen()
		{
			ControlsScreen.OpenControlsScreen();
		}

        static void DrawNamePopup()
		{
			DrawSettings.UIThemeDLS theme = DrawSettings.ActiveUITheme;

			UI.StartNewLayer();
			UI.DrawFullscreenPanel(theme.MenuBackgroundOverlayCol);

			using (UI.BeginBoundsScope(true))
			{
				Draw.ID panelID = UI.ReservePanel();

				InputFieldTheme inputTheme = theme.ChipNameInputField;

				Vector2 charSize = UI.CalculateTextSize("M", inputTheme.fontSize, inputTheme.font);
				Vector2 padding = new(2, 2);
				Vector2 inputFieldSize = new Vector2(charSize.x * MaxProjectNameLength, charSize.y) + padding * 2;


				InputFieldState state = UI.InputField(ID_ProjectNameInput, inputTheme, UI.Centre, inputFieldSize, "", Anchor.Centre, padding.x, projectNameValidator, true);

				string projectName = state.text;
				bool validProjectName = !string.IsNullOrWhiteSpace(projectName) && SaveUtils.ValidFileName(projectName);
				bool projectNameAlreadyExists = false;
				foreach (string existingProjectName in allProjectNames)
				{
					projectNameAlreadyExists |= string.Equals(projectName, existingProjectName, StringComparison.CurrentCultureIgnoreCase);
				}

				bool canCreateProject = validProjectName && !projectNameAlreadyExists;

				Vector2 buttonsRegionSize = new(inputFieldSize.x, 5);
				Vector2 buttonsRegionCentre = UILayoutHelper.CalculateCentre(UI.PrevBounds.BottomLeft, buttonsRegionSize, Anchor.TopLeft);
				(Vector2 size, Vector2 centre) layoutCancel = UILayoutHelper.HorizontalLayout(2, 0, buttonsRegionCentre, buttonsRegionSize);
				(Vector2 size, Vector2 centre) layoutConfirm = UILayoutHelper.HorizontalLayout(2, 1, buttonsRegionCentre, buttonsRegionSize);

				bool cancelButton = UI.Button("CANCEL", theme.MainMenuButtonTheme, layoutCancel.centre, new Vector2(layoutCancel.size.x, 0), true, false, true);
				bool confirmButton = UI.Button("CONFIRM", theme.MainMenuButtonTheme, layoutConfirm.centre, new Vector2(layoutConfirm.size.x, 0), canCreateProject, false, true);

				if (cancelButton || KeyboardShortcuts.CancelShortcutTriggered())
				{
					state.ClearText();
					activePopup = PopupKind.None;
				}

				if (confirmButton || KeyboardShortcuts.ConfirmShortcutTriggered())
				{
					state.ClearText();
					PopupKind kind = activePopup;
					activePopup = PopupKind.None;
					OnNamePopupConfirmed(kind, projectName);
				}

				UI.ModifyPanel(panelID, UI.GetCurrentBoundsScope().Centre, UI.GetCurrentBoundsScope().Size + Vector2.one * 2, ColHelper.MakeCol255(37, 37, 43));
			}
		}

		static void OnNamePopupConfirmed(PopupKind kind, string name)
		{
			if (kind is PopupKind.NamePopup_RenameProject or PopupKind.NamePopup_DuplicateProject)
			{
				if (kind is PopupKind.NamePopup_RenameProject) Saver.RenameProject(SelectedProjectName, name);
				if (kind is PopupKind.NamePopup_DuplicateProject) Saver.DuplicateProject(SelectedProjectName, name);

				RefreshLoadedProjects();
				selectedProjectIndex = 0; // the modified project will now be at top of list
				UI.GetScrollbarState(ID_ProjectsScrollView).scrollY = 0; // scroll to top so selection is visible
			}
			else if (kind is PopupKind.NamePopup_NewProject)
			{
				Main.CreateOrLoadProject(name);
			}
		}

		static void DrawDeleteProjectConfirmationPopup()
		{
			DrawSettings.UIThemeDLS theme = DrawSettings.ActiveUITheme;

			UI.StartNewLayer();
			UI.DrawFullscreenPanel(theme.MenuBackgroundOverlayCol);

			using (UI.BeginBoundsScope(true))
			{
				Draw.ID panelID = UI.ReservePanel();
				UI.DrawText("Are you sure you want to delete this project?", theme.FontRegular, theme.FontSizeRegular, UI.Centre, Anchor.Centre, Color.yellow);

				Vector2 buttonRegionTopLeft = UI.PrevBounds.BottomLeft + Vector2.down * DrawSettings.VerticalButtonSpacing;
				float buttonRegionWidth = UI.PrevBounds.Width;
				int buttonIndex = UI.HorizontalButtonGroup(new[] { "CANCEL", "DELETE" }, theme.MainMenuButtonTheme, buttonRegionTopLeft, buttonRegionWidth, DrawSettings.HorizontalButtonSpacing, 0, Anchor.TopLeft);
				UI.ModifyPanel(panelID, UI.GetCurrentBoundsScope().Centre, UI.GetCurrentBoundsScope().Size + Vector2.one * 2, ColHelper.MakeCol255(37, 37, 43));

				if (buttonIndex == 0) // Cancel
				{
					activePopup = PopupKind.None;
				}
				else if (buttonIndex == 1) // Delete
				{
					Saver.DeleteProject(SelectedProjectName);
					selectedProjectIndex = -1;
					RefreshLoadedProjects();
					activePopup = PopupKind.None;
				}
			}
		}

		static void DrawImportProjectPopup()
		{
			DrawSettings.UIThemeDLS theme = DrawSettings.ActiveUITheme;

			UI.StartNewLayer();
			UI.DrawFullscreenPanel(theme.MenuBackgroundOverlayCol);

			using (UI.BeginBoundsScope(true))
			{
				Draw.ID panelID = UI.ReservePanel();
				UI.DrawText("Import Project from ZIP", theme.FontRegular, theme.FontSizeRegular, UI.Centre, Anchor.Centre, Color.white);
				
				Vector2 instructionPos = UI.PrevBounds.BottomLeft + Vector2.down * DrawSettings.VerticalButtonSpacing;
				UI.DrawText("Select a ZIP file containing a DLS project to import.", theme.FontRegular, theme.FontSizeRegular * 0.8f, instructionPos, Anchor.TopLeft, new Color(1, 1, 1, 0.7f));

				Vector2 buttonRegionTopLeft = UI.PrevBounds.BottomLeft + Vector2.down * DrawSettings.VerticalButtonSpacing;
				float buttonRegionWidth = UI.PrevBounds.Width;
				int buttonIndex = UI.HorizontalButtonGroup(new[] { "CANCEL", "SELECT ZIP" }, theme.MainMenuButtonTheme, buttonRegionTopLeft, buttonRegionWidth, DrawSettings.HorizontalButtonSpacing, 0, Anchor.TopLeft);
				
				UI.ModifyPanel(panelID, UI.GetCurrentBoundsScope().Centre, UI.GetCurrentBoundsScope().Size + Vector2.one * 2, ColHelper.MakeCol255(37, 37, 43));

				if (buttonIndex == 0 || KeyboardShortcuts.CancelShortcutTriggered()) // Cancel
				{
					activePopup = PopupKind.None;
				}
				else if (buttonIndex == 1) // Select ZIP
				{
					string importedProjectName = ImportProjectFromZip();
					activePopup = PopupKind.None;
					if (!string.IsNullOrEmpty(importedProjectName))
					{
						RefreshLoadedProjects();
						// Find the imported project in the list
						selectedProjectIndex = -1;
						for (int i = 0; i < allProjectNames.Length; i++)
						{
							if (string.Equals(allProjectNames[i], importedProjectName, StringComparison.CurrentCultureIgnoreCase))
							{
								selectedProjectIndex = i;
								break;
							}
						}
						UI.GetScrollbarState(ID_ProjectsScrollView).scrollY = 0; // scroll to top
					}
				}
			}
		}

		static void DrawExportProjectPopup()
		{
			DrawSettings.UIThemeDLS theme = DrawSettings.ActiveUITheme;

			UI.StartNewLayer();
			UI.DrawFullscreenPanel(theme.MenuBackgroundOverlayCol);

			using (UI.BeginBoundsScope(true))
			{
				Draw.ID panelID = UI.ReservePanel();
				UI.DrawText($"Export '{SelectedProjectName}' as ZIP", theme.FontRegular, theme.FontSizeRegular, UI.Centre, Anchor.Centre, Color.white);
				
				Vector2 instructionPos = UI.PrevBounds.BottomLeft + Vector2.down * DrawSettings.VerticalButtonSpacing;
				#if UNITY_EDITOR || UNITY_STANDALONE_WIN
					UI.DrawText("Choose where to save the exported project ZIP file.", theme.FontRegular, theme.FontSizeRegular * 0.8f, instructionPos, Anchor.TopLeft, new Color(1, 1, 1, 0.7f));
				#else
					UI.DrawText("Project will be exported to your desktop.", theme.FontRegular, theme.FontSizeRegular * 0.8f, instructionPos, Anchor.TopLeft, new Color(1, 1, 1, 0.7f));
				#endif

				Vector2 buttonRegionTopLeft = UI.PrevBounds.BottomLeft + Vector2.down * DrawSettings.VerticalButtonSpacing;
				float buttonRegionWidth = UI.PrevBounds.Width;
				int buttonIndex = UI.HorizontalButtonGroup(new[] { "CANCEL", "EXPORT" }, theme.MainMenuButtonTheme, buttonRegionTopLeft, buttonRegionWidth, DrawSettings.HorizontalButtonSpacing, 0, Anchor.TopLeft);
				
				UI.ModifyPanel(panelID, UI.GetCurrentBoundsScope().Centre, UI.GetCurrentBoundsScope().Size + Vector2.one * 2, ColHelper.MakeCol255(37, 37, 43));

				if (buttonIndex == 0 || KeyboardShortcuts.CancelShortcutTriggered()) // Cancel
				{
					activePopup = PopupKind.None;
				}
				else if (buttonIndex == 1) // Export
				{
					ExportProjectToZip();
					activePopup = PopupKind.None;
				}
			}
		}

		static string ImportProjectFromZip()
		{
			try
			{
#if UNITY_EDITOR
				string zipPath = UnityEditor.EditorUtility.OpenFilePanel("Import Project ZIP", "", "zip");
				if (!string.IsNullOrEmpty(zipPath))
				{
					return ImportZipFile(zipPath);
				}
#elif UNITY_STANDALONE_WIN
				string zipPath = ShowWindowsOpenFileDialog("Import Project ZIP", "ZIP files (*.zip)|*.zip");
				if (!string.IsNullOrEmpty(zipPath))
				{
					return ImportZipFile(zipPath);
				}
#else
				// Fallback: Show input dialog for manual path entry
				activePopup = PopupKind.ZipPopup_ManualImportPath;
				UI.GetInputFieldState(ID_ManualImportPathInput).ClearText();
				return null; // Will be handled in the popup
#endif
			}
			catch (System.Exception ex)
			{
				Debug.LogError($"Failed to import project: {ex.Message}\n{ex.StackTrace}");
			}
			return null;
		}

		static void ExportProjectToZip()
		{
			try
			{
#if UNITY_EDITOR
				string defaultFileName = $"{SelectedProjectName}.zip";
				string savePath = UnityEditor.EditorUtility.SaveFilePanel("Export Project as ZIP", "", defaultFileName, "zip");
				if (!string.IsNullOrEmpty(savePath))
				{
					ExportToZipFile(savePath);
				}
#elif UNITY_STANDALONE_WIN
				string defaultFileName = $"{SelectedProjectName}.zip";
				string savePath = ShowWindowsSaveFileDialog("Export Project as ZIP", defaultFileName, "ZIP files (*.zip)|*.zip");
				if (!string.IsNullOrEmpty(savePath))
				{
					ExportToZipFile(savePath);
				}
#else
				// Fallback: Export to default location (Documents folder for better cross-platform compatibility)
				string documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
				// If Documents folder doesn't exist, fallback to Desktop
				if (!System.IO.Directory.Exists(documentsPath))
				{
					documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
				}
				// If Desktop also doesn't exist, use current directory
				if (!System.IO.Directory.Exists(documentsPath))
				{
					documentsPath = System.Environment.CurrentDirectory;
				}
				
				string defaultPath = System.IO.Path.Combine(documentsPath, $"{SelectedProjectName}.zip");
				ExportToZipFile(defaultPath);
				Debug.Log($"Project exported to: {defaultPath}");
#endif
			}
			catch (System.Exception ex)
			{
				Debug.LogError($"Failed to export project: {ex.Message}\n{ex.StackTrace}");
			}
		}

		static string ImportZipFile(string zipPath)
		{
			try
			{
				string projectsPath = SavePaths.ProjectsPath;
				string tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.Guid.NewGuid().ToString());
				
				// Extract ZIP to temporary location
				using (var archive = System.IO.Compression.ZipFile.OpenRead(zipPath))
				{
					archive.ExtractToDirectory(tempPath);
				}
				
				// Get project name from ZIP file name
				string projectName = System.IO.Path.GetFileNameWithoutExtension(zipPath);
				
				// Ensure unique project name
				int counter = 1;
				string originalName = projectName;
				while (System.IO.Directory.Exists(System.IO.Path.Combine(projectsPath, projectName)))
				{
					projectName = $"{originalName}_{counter}";
					counter++;
				}

				// Find the actual project content using SavePaths structure validation
				string sourceContentPath = tempPath;
				
				// Check if extracted directory contains valid DLS project structure
				string projectFileName = SavePaths.ProjectFileName;
				string chipsDirectoryName = System.IO.Path.GetFileName(SavePaths.GetChipsPath("temp"));
				
				bool hasProjectFile = System.IO.File.Exists(System.IO.Path.Combine(sourceContentPath, projectFileName));
				bool hasChipsDir = System.IO.Directory.Exists(System.IO.Path.Combine(sourceContentPath, chipsDirectoryName));
				
				// If not found in root, look in first non-metadata subdirectory
				if (!hasProjectFile && !hasChipsDir)
				{
					string[] subDirs = System.IO.Directory.GetDirectories(tempPath);
					foreach (string subDir in subDirs)
					{
						string dirName = System.IO.Path.GetFileName(subDir);
						// Skip metadata folders
						if (dirName.StartsWith("_") || dirName.StartsWith(".") || dirName.Equals("__MACOSX", System.StringComparison.OrdinalIgnoreCase))
							continue;
							
						// Check if this subdirectory has project structure
						bool subHasProjectFile = System.IO.File.Exists(System.IO.Path.Combine(subDir, projectFileName));
						bool subHasChipsDir = System.IO.Directory.Exists(System.IO.Path.Combine(subDir, chipsDirectoryName));
						
						if (subHasProjectFile || subHasChipsDir)
						{
							sourceContentPath = subDir;
							break;
						}
					}
				}
				
				// Verify we found valid project content
				hasProjectFile = System.IO.File.Exists(System.IO.Path.Combine(sourceContentPath, projectFileName));
				hasChipsDir = System.IO.Directory.Exists(System.IO.Path.Combine(sourceContentPath, chipsDirectoryName));
				
				if (!hasProjectFile && !hasChipsDir)
				{
					Debug.LogError("Could not find valid project content in ZIP file");
					return null;
				}

				string finalProjectPath = System.IO.Path.Combine(projectsPath, projectName);
				System.IO.Directory.CreateDirectory(finalProjectPath);
				
				// Copy all project content preserving structure
				CopyProjectContent(sourceContentPath, finalProjectPath);
				
				// Clean up temp directory
				try
				{
					System.IO.Directory.Delete(tempPath, true);
				}
				catch
				{
					// Ignore cleanup errors
				}
				
				Debug.Log($"Successfully imported project: {projectName}");
				return projectName;
			}
			catch (System.Exception ex)
			{
				Debug.LogError($"Failed to import ZIP file: {ex.Message}");
				return null;
			}
		}

		static bool IsMetadataFolder(string folderName)
		{
			return folderName.StartsWith("_") || 
				   folderName.StartsWith(".") || 
				   folderName.Equals("__MACOSX", System.StringComparison.OrdinalIgnoreCase);
		}

		static void CopyProjectContent(string sourcePath, string destPath)
		{
			// Copy all files
			string[] files = System.IO.Directory.GetFiles(sourcePath, "*", System.IO.SearchOption.AllDirectories);
			foreach (string filePath in files)
			{
				string relativePath = System.IO.Path.GetRelativePath(sourcePath, filePath);
				
				// Skip metadata files
				if (IsMetadataFile(relativePath)) continue;
				
				string destFilePath = System.IO.Path.Combine(destPath, relativePath);
				string destDir = System.IO.Path.GetDirectoryName(destFilePath);
				
				// Create directory if it doesn't exist
				if (!System.IO.Directory.Exists(destDir))
				{
					System.IO.Directory.CreateDirectory(destDir);
				}
				
				// Copy the file
				System.IO.File.Copy(filePath, destFilePath, true);
			}
			
			// Create empty directories that don't contain files
			string[] dirs = System.IO.Directory.GetDirectories(sourcePath, "*", System.IO.SearchOption.AllDirectories);
			foreach (string dirPath in dirs)
			{
				string relativePath = System.IO.Path.GetRelativePath(sourcePath, dirPath);
				
				// Skip metadata directories
				if (IsMetadataPath(relativePath)) continue;
				
				string destDirPath = System.IO.Path.Combine(destPath, relativePath);
				if (!System.IO.Directory.Exists(destDirPath))
				{
					System.IO.Directory.CreateDirectory(destDirPath);
				}
			}
		}

		static bool IsMetadataFile(string relativePath)
		{
			return relativePath.Contains("__MACOSX") ||
				   relativePath.Contains("/.DS_Store") ||
				   relativePath.Contains("\\.DS_Store") ||
				   relativePath.StartsWith(".") ||
				   relativePath.Contains("/.") ||
				   relativePath.Contains("\\.");
		}

		static bool IsMetadataPath(string relativePath)
		{
			string[] pathParts = relativePath.Split(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar);
			foreach (string part in pathParts)
			{
				if (IsMetadataFolder(part))
				{
					return true;
				}
			}
			return false;
		}

		static void ExportToZipFile(string savePath)
		{
			try
			{
				string projectPath = System.IO.Path.Combine(SavePaths.ProjectsPath, SelectedProjectName);
				if (!System.IO.Directory.Exists(projectPath))
				{
					Debug.LogError($"Project directory not found: {projectPath}");
					return;
				}

				// Delete existing file if it exists
				if (System.IO.File.Exists(savePath))
				{
					System.IO.File.Delete(savePath);
				}

				// Get all files and directories to export
				string[] allFiles = System.IO.Directory.GetFiles(projectPath, "*", System.IO.SearchOption.AllDirectories);
				string[] allDirs = System.IO.Directory.GetDirectories(projectPath, "*", System.IO.SearchOption.AllDirectories);
				
				Debug.Log($"Exporting {allFiles.Length} files and {allDirs.Length} directories from project: {SelectedProjectName}");

				// Create ZIP manually to ensure complete project structure
				using (var archive = System.IO.Compression.ZipFile.Open(savePath, System.IO.Compression.ZipArchiveMode.Create))
				{
					int filesAdded = 0;
					int dirsAdded = 0;
					
					// Add all files
					foreach (string filePath in allFiles)
					{
						try
						{
							string relativePath = System.IO.Path.GetRelativePath(projectPath, filePath);
							// Normalize path separators for ZIP compatibility
							relativePath = NormalizePath(relativePath);
							
							archive.CreateEntryFromFile(filePath, relativePath);
							filesAdded++;
						}
						catch (System.Exception ex)
						{
							Debug.LogWarning($"Failed to add file {filePath} to ZIP: {ex.Message}");
						}
					}
					
					// Add empty directories
					foreach (string dirPath in allDirs)
					{
						try
						{
							// Only add directory entry if it's empty
							if (IsEmptyDirectory(dirPath))
							{
								string relativePath = System.IO.Path.GetRelativePath(projectPath, dirPath);
								relativePath = NormalizePath(relativePath) + "/";
								
								archive.CreateEntry(relativePath);
								dirsAdded++;
							}
						}
						catch (System.Exception ex)
						{
							Debug.LogWarning($"Failed to add directory {dirPath} to ZIP: {ex.Message}");
						}
					}
					
					Debug.Log($"Successfully added {filesAdded} files and {dirsAdded} empty directories to ZIP");
				}
				
				Debug.Log($"Successfully exported project '{SelectedProjectName}' to: {savePath}");
			}
			catch (System.Exception ex)
			{
				Debug.LogError($"Failed to export to ZIP file: {ex.Message}\n{ex.StackTrace}");
			}
		}

		static string NormalizePath(string path)
		{
			// Convert to forward slashes for ZIP compatibility
			return path.Replace('\\', '/');
		}

		static bool IsEmptyDirectory(string dirPath)
		{
			try
			{
				return System.IO.Directory.GetFiles(dirPath, "*", System.IO.SearchOption.AllDirectories).Length == 0;
			}
			catch
			{
				return false;
			}
		}

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
		static string ShowWindowsOpenFileDialog(string title, string filter)
		{
			try
			{
				using (var dialog = new System.Windows.Forms.OpenFileDialog())
				{
					dialog.Title = title;
					dialog.Filter = filter;
					dialog.Multiselect = false;
					if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
					{
						return dialog.FileName;
					}
				}
			}
			catch (System.Exception ex)
			{
				Debug.LogError($"Windows file dialog error: {ex.Message}");
			}
			return null;
		}

		static string ShowWindowsSaveFileDialog(string title, string fileName, string filter)
		{
			try
			{
				using (var dialog = new System.Windows.Forms.SaveFileDialog())
				{
					dialog.Title = title;
					dialog.FileName = fileName;
					dialog.Filter = filter;
					if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
					{
						return dialog.FileName;
					}
				}
			}
			catch (System.Exception ex)
			{
				Debug.LogError($"Windows file dialog error: {ex.Message}");
			}
			return null;
		}
#endif
		static void DrawManualImportPathPopup()
		{
			DrawSettings.UIThemeDLS theme = DrawSettings.ActiveUITheme;

			UI.StartNewLayer();
			UI.DrawFullscreenPanel(theme.MenuBackgroundOverlayCol);

			using (UI.BeginBoundsScope(true))
			{
				Draw.ID panelID = UI.ReservePanel();
				UI.DrawText("Import Project from ZIP", theme.FontRegular, theme.FontSizeRegular, UI.Centre, Anchor.Centre, Color.white);
				
				Vector2 instructionPos = UI.PrevBounds.BottomLeft + Vector2.down * DrawSettings.VerticalButtonSpacing;
				UI.DrawText("Enter the full path to the ZIP file:", theme.FontRegular, theme.FontSizeRegular * 0.8f, instructionPos, Anchor.TopLeft, new Color(1, 1, 1, 0.7f));

				// Input field for file path
				InputFieldTheme inputTheme = theme.ChipNameInputField;
				Vector2 inputSize = new Vector2(60, 3);
				Vector2 inputPos = UI.PrevBounds.BottomLeft + Vector2.down * DrawSettings.VerticalButtonSpacing;
				InputFieldState pathState = UI.InputField(ID_ManualImportPathInput, inputTheme, inputPos, inputSize, "", Anchor.TopLeft, 1f, null, true);

				Vector2 buttonRegionTopLeft = UI.PrevBounds.BottomLeft + Vector2.down * DrawSettings.VerticalButtonSpacing;
				float buttonRegionWidth = UI.PrevBounds.Width;
				bool canImport = !string.IsNullOrWhiteSpace(pathState.text) && System.IO.File.Exists(pathState.text) && pathState.text.ToLower().EndsWith(".zip");
				bool[] buttonStates = { true, canImport };
				int buttonIndex = UI.HorizontalButtonGroup(new[] { "CANCEL", "IMPORT" }, buttonStates, theme.MainMenuButtonTheme, buttonRegionTopLeft, buttonRegionWidth, DrawSettings.HorizontalButtonSpacing, 0, Anchor.TopLeft);
				
				if (!canImport && !string.IsNullOrWhiteSpace(pathState.text))
				{
					Vector2 errorPos = UI.PrevBounds.BottomLeft + Vector2.down * DrawSettings.VerticalButtonSpacing;
					UI.DrawText("Please enter a valid path to a .zip file", theme.FontRegular, theme.FontSizeRegular * 0.8f, errorPos, Anchor.TopLeft, Color.red);
				}
				
				UI.ModifyPanel(panelID, UI.GetCurrentBoundsScope().Centre, UI.GetCurrentBoundsScope().Size + Vector2.one * 2, ColHelper.MakeCol255(37, 37, 43));

				if (buttonIndex == 0 || KeyboardShortcuts.CancelShortcutTriggered()) // Cancel
				{
					pathState.ClearText();
					activePopup = PopupKind.None;
				}
				else if (buttonIndex == 1) // Import
				{
					string importedProjectName = ImportZipFile(pathState.text);
					pathState.ClearText();
					activePopup = PopupKind.None;
					if (!string.IsNullOrEmpty(importedProjectName))
					{
						RefreshLoadedProjects();
						// Find the imported project in the list
						selectedProjectIndex = -1;
						for (int i = 0; i < allProjectNames.Length; i++)
						{
							if (string.Equals(allProjectNames[i], importedProjectName, StringComparison.CurrentCultureIgnoreCase))
							{
								selectedProjectIndex = i;
								break;
							}
						}
						UI.GetScrollbarState(ID_ProjectsScrollView).scrollY = 0; // scroll to top
					}
				}
			}
		}

		static void DrawAboutScreen()
		{
			ButtonTheme theme = DrawSettings.ActiveUITheme.MainMenuButtonTheme;

			UI.DrawText("Todo: write something helpful here...", theme.font, theme.fontSize, UI.Centre, Anchor.Centre, Color.white);
			if (UI.Button("Back", theme, UI.CentreBottom + Vector2.up * 22, Vector2.zero, true, true, true))
			{
				BackToMain();
			}
		}

		static void DrawVersionInfo()
		{
			DrawSettings.UIThemeDLS theme = DrawSettings.ActiveUITheme;
			UI.DrawPanel(UI.BottomLeft, new Vector2(UI.Width, 4), ColHelper.MakeCol255(37, 37, 43), Anchor.BottomLeft);

			float pad = 1;
			Color col = new(1, 1, 1, 0.5f);
			Color modColor = new(0.98f, 0.76f, 0.26f);

            Vector2 versionPos = UI.PrevBounds.CentreLeft + Vector2.right * pad;
			Vector2 datePos = UI.PrevBounds.CentreRight + Vector2.left * pad;
			Vector2 moddedPos = UI.PrevBounds.Centre;

			UI.DrawText(authorString, theme.FontRegular, theme.FontSizeRegular, versionPos, Anchor.TextCentreLeft, col);
			UI.DrawText(versionString, theme.FontRegular, theme.FontSizeRegular, datePos, Anchor.TextCentreRight, col);
            UI.DrawText(moddedString, theme.FontRegular, theme.FontSizeRegular, moddedPos, Anchor.TextCentre, modColor);

        }

        static string ResolutionToString(Vector2Int r) => $"{r.x} x {r.y}";

		static void Quit()
		{
			#if UNITY_EDITOR
				// There should be a NullReferenceException when quitting, but it does not affect the application.
				UnityEditor.EditorApplication.isPlaying = false;
			#else
				Application.Quit();
			#endif
		}

		enum MenuScreen
		{
			Main,
			LoadProject,
			Settings,
			About,
			Controls
		}

		enum PopupKind
		{
			None,
			DeleteConfirmation,
			NamePopup_RenameProject,
			NamePopup_DuplicateProject,
			NamePopup_NewProject,
			ZipPopup_ImportProject,
			ZipPopup_ExportProject,
			ZipPopup_ManualImportPath
		}
	}
}
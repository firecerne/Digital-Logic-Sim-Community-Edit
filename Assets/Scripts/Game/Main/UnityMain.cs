using DLS.Graphics;
using DLS.Simulation;
using Seb.Helpers;
using UnityEngine;

namespace DLS.Game
{
	public class UnityMain : MonoBehaviour
	{
#if DEBUG
		[Header("Dev Settings (editor only)")]
		public bool openSaveDirectory;
		public bool openInMainMenu;

		public string testProjectName;
		public bool openA = true;
		public string chipToOpenA;
		public string chipToOpenB;
#endif

		void Awake()
		{
			ResetStatics();

			AudioState audioState = new();
			FindFirstObjectByType<AudioUnity>().audioState = audioState;

			Main.Init(audioState);


#if DEBUG
			if (openInMainMenu) Main.LoadMainMenu();
			else Main.CreateOrLoadProject(testProjectName, openA ? chipToOpenA : chipToOpenB);
#else
			Main.LoadMainMenu();
#endif
		}

		void Update()
		{
#if DEBUG
			EditorDebugUpdate();
#endif

			Main.Update();
		}

		void OnDestroy()
		{
			if (Project.ActiveProject != null) Project.ActiveProject.NotifyExit();
		}

#if DEBUG
		void EditorDebugUpdate()
		{
			if (InputHelper.AltIsHeld && InputHelper.IsKeyDownThisFrame(KeyCode.P))
			{
				if (InteractionState.PinUnderMouse != null)
				{
					SimPin simPin = Project.ActiveProject.rootSimChip.GetSimPinFromAddress(InteractionState.PinUnderMouse.Address);
					uint bitData = simPin.State.GetValue();
					uint tristateFlags = simPin.State.GetTristatedFlags() ;
					string bitString = StringHelper.CreateBinaryString(bitData);
					string triStateString = StringHelper.CreateBinaryString(tristateFlags);

					string displayString = "";
					for (int i = 0; i < bitString.Length; i++)
					{
						if (triStateString[i] == '1')
						{
							displayString += bitString[i] == '1' ? "?" : "x";
						}
						else
						{
							displayString += bitString[i];
						}
					}

					Debug.Log($"Pin state: {displayString}");
				}
			}
		}

		void OnValidate()
		{
			if (openSaveDirectory)
			{
				openSaveDirectory = false;
				Main.OpenSaveDataFolderInFileBrowser();
			}
		}
#endif

		/// Ensure static stuff gets properly reset (on account of domain-reloading being disabled in editor)
		static void ResetStatics()
		{
			Simulator.Reset();
			UIDrawer.Reset();
			InteractionState.Reset();
			CameraController.Reset();
			WorldDrawer.Reset();
		}
	}
}
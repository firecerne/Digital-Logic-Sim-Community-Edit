using System.Collections.Generic;
using Seb.Helpers;
using UnityEngine;
using System;

namespace DLS.Simulation
{
	public static class SimKeyboardHelper
	{

		static readonly HashSet<KeyCode> KeyLookup = new();
		static bool HasAnyInput;

		// Call from Main Thread
		public static void RefreshInputState()
		{
			lock (KeyLookup)
			{
				KeyLookup.Clear();
				HasAnyInput = false;

				if (!InputHelper.AnyKeyOrMouseHeldThisFrame) return; // early exit if no key held

				// Don't trigger key chips if modifier is held
				if (InputHelper.ModifierKeysOff == false)
				{
					// Do for each in SpecialKeys
					foreach (KeyCode key in InputHelper.ModifierKeys)
					{
						if (InputHelper.IsKeyHeld(key))
						{
							return; // If special key is held and special keys are not off, don't process keys
						}
					}
				}

				foreach (KeyCode key in Seb.Helpers.InputHelper.ValidInputKeys)
				{
					if (InputHelper.IsKeyHeld(key))
					{
						KeyLookup.Add(key);
						HasAnyInput = true;
					}
				}
			}
		}

		// Call from Sim Thread
		public static bool KeyIsHeld(uint key)
		{
			bool isHeld;

			// Convert uint to KeyNumberEnum to KeyCode
			InputHelper.KeyNumberEnum keyEnum = (InputHelper.KeyNumberEnum)key;
			string keyName = keyEnum.ToString();
			KeyCode chosenKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), keyName);
			
			lock (KeyLookup)
			{
				isHeld = HasAnyInput && KeyLookup.Contains(chosenKey);
			}

			return isHeld;
		}
	}
}
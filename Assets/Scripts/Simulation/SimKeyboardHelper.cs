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

				float scrollInput = Input.GetAxis("Mouse ScrollWheel");

				if (!InputHelper.AnyKeyOrMouseHeldThisFrame && scrollInput == 0f) return; // early exit if no key held and not scrolling

				foreach (KeyCode key in Seb.Helpers.InputHelper.ValidInputKeys)
				{
					if (key == (KeyCode)99997 || key == (KeyCode)99999)
					{
						if (scrollInput < 0f)
						{
							KeyLookup.Add((KeyCode)99997); // Scroll Down
							HasAnyInput = true;
						}
						else if (scrollInput > 0f)
						{
							KeyLookup.Add((KeyCode)99999); // Scroll Up
							HasAnyInput = true;
						}
					}
					else if (InputHelper.IsKeyHeld(key))
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

			KeyCode chosenKey = (KeyCode)key;

			// Deal with no key
			if (chosenKey == KeyCode.None)
			{
				return !HasAnyInput;
			}

			lock (KeyLookup)
			{
				isHeld = HasAnyInput && KeyLookup.Contains(chosenKey);
			}

			return isHeld;
		}
	}
}
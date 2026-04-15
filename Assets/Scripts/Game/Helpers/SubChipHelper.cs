using System.Diagnostics.Contracts;
using System.Linq;
using Seb.Vis;
using UnityEngine;
using static DLS.Graphics.DrawSettings;

namespace DLS.Game
{
	public static class SubChipHelper
	{
		/// Display on single line if name fits comfortably, otherwise use 'formatted' version (split across multiple lines)
		[Pure]
		public static string GetDisplayName(string name, Vector2 size)
		{
			if (Draw.CalculateTextBoundsSize(name, FontSizeChipName, FontBold).x < size.x - PinRadius * 2.5f)
			{
				return name;
			}
			return CreateMultiLineName(name);
		}

		/// <summary>
		/// Split name into two lines if necessary.
		/// </summary>
		/// <param name="name"> Unformatted name of the chip </param>
		/// <remarks>
		/// If <paramref name="name"/> is shorter than 7 characters or contains no spaces, unformatted name will be returned.
		/// </remarks>
		[Pure]
		public static string CreateMultiLineName(string name)
		{
			if (name.Length <= 6 || !name.Contains(' ')) return name;

			string[] lines = { name };
			float bestSplitPenalty = float.MaxValue;

			for (int i = 0; i < name.Length; i++)
			{
				if (name[i] == ' ')
				{
					string lineA = name.Substring(0, i).Trim();
					string lineB = name.Substring(i).Trim();
					int lenDiff = lineA.Length - lineB.Length;
					float splitPenalty = Mathf.Abs(lenDiff);
					if (splitPenalty < bestSplitPenalty)
					{
						lines = new[] { lineA, lineB };
						bestSplitPenalty = splitPenalty;
					}
				}
			}

			// Pad lines with spaces to centre justify
			string formatted = "";
			int longestLine = lines.Max(l => l.Length);

			for (int i = 0; i < lines.Length; i++)
			{
				string line = lines[i];
				int numPadChars = longestLine - line.Length;
				int numPadLeft = numPadChars / 2;
				int numPadRight = numPadChars - numPadLeft;
				line = line.PadLeft(line.Length + numPadLeft, ' ');
				line = line.PadRight(line.Length + numPadRight, ' ');

				// Add half space tag to center if padding is uneven
				if (numPadLeft < numPadRight)
				{
					line = "<halfSpace>" + line;
				}

				formatted += line;
				if (i < lines.Length - 1) formatted += "\n";
			}

			return formatted;
		}
	}
}
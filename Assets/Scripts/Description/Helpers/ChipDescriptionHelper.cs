using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLS.Description
{
    // Contains functions for calculating various surface properties of ChipDescriptions when they aren't yet part of a chip library.
    // For functions that require looking up chip descriptions (e.g. for subchips), see ChipLibrary.
    // This is useful when such a value is needed BEFORE the chip library is built, like when loading the project from disk.
    public static class ChipDescriptionHelper
    {

        public static int CountTotalInputWidth(ChipDescription chip)
        {
            int totalWidth = 0;
            foreach (PinDescription pin in chip.InputPins)
            {
                totalWidth += pin.BitCount;
            }
            return totalWidth;
        }

        public static int GetBiggestOutputPinWidth(ChipDescription chip)
        {
            int biggestWidth = 0;
            foreach (PinDescription pin in chip.OutputPins)
            {
                if (pin.BitCount > biggestWidth) biggestWidth = pin.BitCount;
            }
            return biggestWidth;
        }

        public static int GetBiggestInputPinWidth(ChipDescription chip)
        {
            int biggestWidth = 0;
            foreach (PinDescription pin in chip.InputPins)
            {
                if (pin.BitCount > biggestWidth) biggestWidth = pin.BitCount;
            }
            return biggestWidth;
        }
    }
}

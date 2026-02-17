using DLS.Description;
using DLS.Simulation;

namespace DLS.SaveSystem
{
    public static class SavedDescriptionCorrector
    {

        // Corrects issues with chip descriptions that arise with software or user error, not those that are created when updating versions.
        // For that, see UpgradeHelper.
        public static void CorrectChipDescription(ChipDescription description)
        {
            CorrectCachingAbility(description);
        }
        static void CorrectCachingAbility(ChipDescription description)
        {
            int totalInputPinCount = ChipDescriptionHelper.CountTotalInputWidth(description);
            int biggestOutputPinCount = ChipDescriptionHelper.GetBiggestOutputPinWidth(description);
            int biggestInputPinCount = ChipDescriptionHelper.GetBiggestInputPinWidth(description);

            // Only a primitive implementation of CanBeCached because other caching issues are managed when simulating.
            // This only removes the more obvious problems. Should be enough.

            bool appropriatelySizedIO = totalInputPinCount <= SimChip.MAX_NUM_INPUT_BITS_WHEN_USER_CACHING
                                     && biggestOutputPinCount <= SimChip.MAX_PIN_WIDTH_WHEN_CACHING
                                     && biggestInputPinCount <= SimChip.MAX_PIN_WIDTH_WHEN_CACHING;

            // If somehow the chip was saved as cacheable when it shouldn't be, correct that.
            if (!appropriatelySizedIO)
            {
                description.CanBeCached = false;
            }

            // If a chip that can't be cached was saved as should be cached, correct that.
            if (!description.CanBeCached)
            {
                description.ShouldBeCached = false;
            }

        }
    }
}
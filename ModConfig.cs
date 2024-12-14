using StardewModdingAPI;

namespace QuickEatKeybinds
{
    public class ModConfig
    {
        // Keybind for eating the highest energy-restoring item
        public SButton EatHighestEnergyKey { get; set; } = SButton.G;

        // Keybind for eating the highest health-restoring item
        public SButton EatHighestHealthKey { get; set; } = SButton.T;

        // Keybind for eating the currently selected item
        public SButton EatSelectedItemKey { get; set; } = SButton.H;

        // Option to play the eating animation
        public bool PlayEatingAnimation { get; set; } = true;
    }
}


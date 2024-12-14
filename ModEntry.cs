using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System.Linq;

namespace QuickEatKeybinds
{
    public class ModEntry : Mod
    {
        private ModConfig Config = null!;

        public override void Entry(IModHelper helper)
        {
            Config = helper.ReadConfig<ModConfig>();

            // Listen for input events
            helper.Events.Input.ButtonPressed += OnButtonPressed;

            // Log loaded keybinds and settings
            Monitor.Log($"Quick Eat Keybinds initialized. Keybinds: " +
                        $"EatHighestEnergyKey: [{Config.EatHighestEnergyKey}], " +
                        $"EatHighestHealthKey: [{Config.EatHighestHealthKey}], " +
                        $"EatSelectedItemKey: [{Config.EatSelectedItemKey}], " +
                        $"PlayEatingAnimation: [{Config.PlayEatingAnimation}].",
                        LogLevel.Info);
        }

        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            if (e.Button == Config.EatHighestEnergyKey)
            {
                var highestEnergyItem = Game1.player.Items
                    .OfType<StardewValley.Object>()
                    .Where(item => item != null && item.Edibility > 0)
                    .OrderByDescending(CalculateEnergy)
                    .FirstOrDefault();

                if (highestEnergyItem != null)
                {
                    ConsumeItem(highestEnergyItem);
                }
                else
                {
                    Monitor.Log("No energy restoring items found in the inventory.", LogLevel.Warn);
                }
            }

            if (e.Button == Config.EatHighestHealthKey)
            {
                var highestHealthItem = Game1.player.Items
                    .OfType<StardewValley.Object>()
                    .Where(item => item != null && item.Edibility > 0)
                    .OrderByDescending(CalculateHealth)
                    .FirstOrDefault();

                if (highestHealthItem != null)
                {
                    ConsumeItem(highestHealthItem);
                }
                else
                {
                    Monitor.Log("No health restoring items found in the inventory.", LogLevel.Warn);
                }
            }

            if (e.Button == Config.EatSelectedItemKey)
            {
                var selectedItem = Game1.player.CurrentItem;
                if (selectedItem is StardewValley.Object edibleItem && edibleItem.Edibility > 0)
                {
                    ConsumeItem(edibleItem);
                }
                else
                {
                    Monitor.Log("Selected item is not edible.", LogLevel.Warn);
                }
            }
        }

        private void ConsumeItem(StardewValley.Object item)
        {
            // Check if it's a drink
            bool isDrink = item.Category == -27;

            // Calculate energy and health
            int energy = CalculateEnergy(item);
            int health = CalculateHealth(item);

            // Play eating animation if enabled in the config
            if (Config.PlayEatingAnimation)
            {
                Game1.player.eatObject(item);
            }

            // Log the consumption
            string action = isDrink ? "Drank" : "Ate";
            Monitor.Log($"{action} {item.DisplayName}. (+{energy} Energy, +{health} Health)", LogLevel.Info);

            // Apply effects to the player
            Game1.player.Stamina = System.Math.Min(Game1.player.MaxStamina, Game1.player.Stamina + energy);
            Game1.player.health = System.Math.Min(Game1.player.maxHealth, Game1.player.health + health);

            // Consume the item
            item.Stack--;
            if (item.Stack <= 0)
            {
                Game1.player.removeItemFromInventory(item);
            }
        }

        private static int CalculateEnergy(StardewValley.Object item)
        {
            int baseEnergy = item.Edibility;
            if (baseEnergy <= 0)
                return 0;

            // Adjust for quality
            float qualityMultiplier = item.Quality switch
            {
                1 => 1.1f,  // Silver
                2 => 1.25f, // Gold
                4 => 1.5f,  // Iridium
                _ => 1f,    // Normal
            };

            return (int)(baseEnergy * qualityMultiplier);
        }

        private static int CalculateHealth(StardewValley.Object item)
        {
            // Use the healthRecoveredOnConsumption method if defined; fallback to half of energy.
            return item.healthRecoveredOnConsumption() > 0
                ? item.healthRecoveredOnConsumption()
                : CalculateEnergy(item) / 2;
        }
    }
}

// ============================================================================
// GAMECONSTANTS.CS
// ============================================================================
// PURPOSE:      Central hub for ALL string constants, enums, and config values
//               Eliminates magic strings throughout the codebase
// VERSION:      v1 — Foundation
// CREATED:      April 4, 2026
// USAGE:        Reference via GameConstants.Resources.CASH, GameConstants.Buildings.SALOON, etc.
// ============================================================================
// DEV GUIDE:
//   To add a new resource:
//     1. Add a const string in the Resources class below
//     2. Add starting value in InventoryManager inspector
//     3. Add UI text field in ResourceUI
//     4. Done — the event system handles the rest
//
//   To add a new building:
//     1. Add name const in Buildings class below
//     2. Create BuildingData ScriptableObject (Assets > Create > Building Data)
//     3. Add to BuildingDatabase array in inspector
//     4. Create prefab with BuildingBehavior or CropBehavior
//     5. Done — production loop auto-starts
//
//   To add a new tool:
//     1. Add entry to ToolType enum (ToolType.cs)
//     2. Add highlight color in TileHighlightController
//     3. Add case in BuildingPlacementManager.HandleClick()
//     4. Add hotkey in UIManager.Update() or BuildingSelector
// ============================================================================

/// <summary>
/// Centralized constants for the entire Whiskeyville project.
/// Use these instead of magic strings to prevent typos and enable refactoring.
/// </summary>
public static class GameConstants
{
    // ========================================================================
    // RESOURCE NAMES — use instead of hardcoded strings
    // ========================================================================
    public static class Resources
    {
        public const string CASH          = "Cash";
        public const string CORN          = "Corn";
        public const string MASH          = "Mash";
        public const string WHISKEY       = "Whiskey";
        public const string AGED_WHISKEY  = "AgedWhiskey";
        public const string WOOD          = "Wood";
        public const string BARREL        = "Barrel";

        // Phase 2 resources (add when ready)
        public const string FAME          = "Fame";
        public const string GOLD_INGOT    = "GoldIngot";
        public const string WATER         = "Water";
        public const string RYE           = "Rye";
        public const string WHEAT         = "Wheat";
        public const string BARLEY        = "Barley";
    }

    // ========================================================================
    // BUILDING NAMES — use instead of == "Saloon" comparisons
    // ========================================================================
    public static class Buildings
    {
        public const string CORN_FIELD   = "Corn Field";
        public const string MASH_TUN     = "Mash Tun";
        public const string STILL        = "Still";
        public const string COOPERAGE    = "Cooperage";
        public const string RICKHOUSE    = "Rickhouse";
        public const string SALOON       = "Saloon";

        // Phase 2 buildings
        public const string GENERAL_STORE  = "General Store";
        public const string BANK           = "Bank";
        public const string WELL           = "Well";
        public const string BUNKHOUSE      = "Bunkhouse";
        public const string TASTING_ROOM   = "Tasting Room";
        public const string WAREHOUSE      = "Warehouse";
        public const string GRAIN_ELEVATOR = "Grain Elevator";
        public const string WATER_TOWER    = "Water Tower";
        public const string CHURCH         = "Church";
        public const string SCHOOLHOUSE    = "Schoolhouse";
        public const string TRAIN_DEPOT    = "Train Depot";
        public const string POST_OFFICE    = "Post Office";
        public const string DOCK           = "Dock";
        public const string CASINO         = "Lucky Barrel Casino";
    }

    // ========================================================================
    // SORTING LAYERS — matches Unity sorting layer setup
    // ========================================================================
    public static class SortLayers
    {
        public const string GROUND   = "Ground";
        public const string OBJECTS  = "Buildings";
        public const string UI       = "UI";
    }

    // ========================================================================
    // PLAYERPREFS KEYS — all saved settings in one place
    // ========================================================================
    public static class Prefs
    {
        // Audio
        public const string MUSIC_VOLUME   = "MusicVolume";
        public const string SFX_VOLUME     = "SFXVolume";
        public const string AMBIENT_VOLUME = "AmbientVolume";

        // Tutorial
        public const string TUTORIAL_COMPLETE = "TutorialComplete";
        public const string TUTORIAL_STEP     = "TutorialStep";

        // Save
        public const string SAVE_FILE       = "WhiskeyvilleSave";
        public const string LAST_SAVE_TIME  = "LastSaveTime";

        // Settings
        public const string VIBRATION_ON    = "VibrationEnabled";
        public const string NOTIFICATIONS   = "NotificationsEnabled";
    }

    // ========================================================================
    // TAGS — all tags used in the project
    // ========================================================================
    public static class Tags
    {
        public const string TILE     = "Tile";
        public const string BUILDING = "Building";
        public const string TREE     = "Tree";
        public const string NPC      = "NPC";
        public const string PLAYER   = "Player";
    }

    // ========================================================================
    // GAME BALANCE — tweak in one place
    // ========================================================================
    public static class Balance
    {
        // Economy
        public const int    STARTING_CASH         = 200;
        public const int    BASE_WHISKEY_PRICE     = 50;
        public const float  QUALITY_PRICE_MULT_MAX = 3.0f;

        // Grid
        public const int    DEFAULT_GRID_WIDTH    = 20;
        public const int    DEFAULT_GRID_HEIGHT   = 15;
        public const float  DEFAULT_TILE_SIZE     = 1f;

        // Camera
        public const float  MIN_ZOOM              = 3f;
        public const float  MAX_ZOOM              = 15f;
        public const float  DEFAULT_ZOOM_SPEED    = 5f;
        public const float  PINCH_ZOOM_SPEED      = 0.05f;

        // Auto-save
        public const float  AUTO_SAVE_INTERVAL    = 300f;  // 5 minutes

        // Day/Night
        public const float  DAY_CYCLE_DURATION    = 120f;  // 2 minutes real time

        // Whiskey Quality Weights (sum = 1.0)
        public const float  QUALITY_AGING_WEIGHT   = 0.25f;
        public const float  QUALITY_GRAIN_WEIGHT   = 0.20f;
        public const float  QUALITY_MASHBILL_WEIGHT = 0.20f;
        public const float  QUALITY_CHAR_WEIGHT    = 0.15f;
        public const float  QUALITY_BARREL_WEIGHT  = 0.10f;
        public const float  QUALITY_WATER_WEIGHT   = 0.10f;

        // Fame thresholds
        public const int    FAME_TIER_1 = 100;    // Known
        public const int    FAME_TIER_2 = 500;    // Respected
        public const int    FAME_TIER_3 = 2000;   // Famous
        public const int    FAME_TIER_4 = 10000;  // Legendary
        public const int    FAME_TIER_5 = 50000;  // Iconic
    }

    // ========================================================================
    // SCENE NAMES — use instead of hardcoded strings in SceneManager.LoadScene
    // ========================================================================
    public static class Scenes
    {
        public const string INTRO       = "IntroLoading";
        public const string MAIN_MENU   = "MainMenu";
        public const string GAME        = "GameScene";
    }
}

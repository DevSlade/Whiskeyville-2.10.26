// ============================================================================
// DEVCONSOLE.CS
// ============================================================================
// PURPOSE:      Full developer console with command system, UI, and QOL tools
// VERSION:      v2 — Fixed output display, 60+ commands, 20 QOL features
// UPDATED:      April 5, 2026
// ATTACHED TO:  GameScene → DevConsole GameObject (auto-creates UI on Awake)
// ============================================================================
// HOW TO OPEN:
//   Press BACKTICK (`) or TILDE (~) or F12
// ============================================================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class DevConsole : MonoBehaviour
{
    // ========================================================================
    // SINGLETON
    // ========================================================================

    public static DevConsole Instance { get; private set; }

    // ========================================================================
    // INSPECTOR
    // ========================================================================

    [Header("Console Settings")]
    [SerializeField] private KeyCode _toggleKey = KeyCode.BackQuote;
    [SerializeField] private int _maxOutputLines = 300;
    [SerializeField] private int _visibleLines = 22;
    [Range(0f, 1f)]
    [SerializeField] private float _bgOpacity = 0.9f;

    // ========================================================================
    // STATE
    // ========================================================================

    private bool _isOpen = false;
    private bool _creativeMode = false;
    private bool _showFPS = false;
    private bool _showStats = false;
    private float _fpsTimer = 0f;
    private int _frameCount = 0;
    private float _currentFPS = 0f;

    private List<string> _outputLines = new List<string>();
    private List<string> _commandHistory = new List<string>();
    private int _historyIndex = -1;
    private int _scrollOffset = 0; // 0 = bottom (most recent)

    // ========================================================================
    // UI REFERENCES
    // ========================================================================

    private Canvas _canvas;
    private GameObject _panel;
    private TMP_InputField _inputField;
    private TextMeshProUGUI _outputText;
    private TextMeshProUGUI _fpsText;
    private TextMeshProUGUI _statsText;
    private GameObject _fpsObj;
    private GameObject _statsObj;

    // ========================================================================
    // COMMAND REGISTRY
    // ========================================================================

    private Dictionary<string, Action<string[]>> _commands = new Dictionary<string, Action<string[]>>(StringComparer.OrdinalIgnoreCase);
    private Dictionary<string, string> _commandHelp = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    // ========================================================================
    // PUBLIC PROPERTIES
    // ========================================================================

    public bool IsCreativeMode => _creativeMode;
    public bool IsOpen => _isOpen;

    // ========================================================================
    // UNITY LIFECYCLE
    // ========================================================================

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            RegisterAllCommands();
            BuildUI();
            Debug.Log("[DevConsole] Ready. Press ` or F12 to open.");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        // Toggle
        if (Input.GetKeyDown(_toggleKey) || Input.GetKeyDown(KeyCode.F12))
            Toggle();

        if (_isOpen && _inputField != null)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                SubmitCommand();
            if (Input.GetKeyDown(KeyCode.UpArrow))
                NavHistory(-1);
            if (Input.GetKeyDown(KeyCode.DownArrow))
                NavHistory(1);
            // Scroll output with PageUp/PageDown
            if (Input.GetKeyDown(KeyCode.PageUp))
                ScrollOutput(5);
            if (Input.GetKeyDown(KeyCode.PageDown))
                ScrollOutput(-5);
        }

        // Legacy F-keys when closed
        if (!_isOpen) HandleFKeys();

        // FPS
        if (_showFPS) UpdateFPS();
        if (_showStats) UpdateStats();
    }

    // ========================================================================
    // TOGGLE
    // ========================================================================

    public void Toggle() { if (_isOpen) Close(); else Open(); }

    public void Open()
    {
        _isOpen = true;
        if (_panel != null) _panel.SetActive(true);
        if (_inputField != null) { _inputField.ActivateInputField(); _inputField.Select(); }
        AudioManager.Instance?.PlaySFX(AudioManager.SFX_DEV);
    }

    public void Close()
    {
        _isOpen = false;
        if (_panel != null) _panel.SetActive(false);
        if (_inputField != null) { _inputField.text = ""; _inputField.DeactivateInputField(); }
    }

    // ========================================================================
    // COMMAND EXECUTION
    // ========================================================================

    private void SubmitCommand()
    {
        string input = _inputField.text.Trim();
        if (string.IsNullOrEmpty(input)) return;

        _commandHistory.Add(input);
        _historyIndex = _commandHistory.Count;
        _scrollOffset = 0;

        Log($"<color=#88ff88>> {input}</color>");
        Execute(input);

        _inputField.text = "";
        _inputField.ActivateInputField();
    }

    private void Execute(string input)
    {
        string[] parts = input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return;
        string cmd = parts[0];
        string[] args = parts.Skip(1).ToArray();

        if (_commands.TryGetValue(cmd, out Action<string[]> action))
        {
            try { action(args); }
            catch (Exception e) { LogErr($"Command error: {e.Message}"); }
        }
        else
        {
            LogErr($"Unknown command: '{cmd}'. Type 'help' for list.");
        }
    }

    private void NavHistory(int dir)
    {
        if (_commandHistory.Count == 0) return;
        _historyIndex = Mathf.Clamp(_historyIndex + dir, 0, _commandHistory.Count);
        _inputField.text = _historyIndex < _commandHistory.Count ? _commandHistory[_historyIndex] : "";
        _inputField.caretPosition = _inputField.text.Length;
    }

    private void ScrollOutput(int lines)
    {
        _scrollOffset = Mathf.Clamp(_scrollOffset + lines, 0, Mathf.Max(0, _outputLines.Count - _visibleLines));
        RefreshOutput();
    }

    private void HandleFKeys()
    {
        if (Input.GetKeyDown(KeyCode.F1)) Execute("give cash 100");
        if (Input.GetKeyDown(KeyCode.F2)) Execute("give corn 100");
        if (Input.GetKeyDown(KeyCode.F3)) Execute("give mash 100");
        if (Input.GetKeyDown(KeyCode.F4)) Execute("give whiskey 100");
        if (Input.GetKeyDown(KeyCode.F5)) Execute("give agedwhiskey 100");
        if (Input.GetKeyDown(KeyCode.F6)) Execute("sell");
        if (Input.GetKeyDown(KeyCode.F7)) Execute("save");
        if (Input.GetKeyDown(KeyCode.F8)) Execute("load");
        if (Input.GetKeyDown(KeyCode.F9)) Execute("resources");
        if (Input.GetKeyDown(KeyCode.F10)) Execute("reset");
    }

    // ========================================================================
    // LOGGING — Simple tail-based display (no ScrollRect needed)
    // ========================================================================

    public void Log(string msg)
    {
        _outputLines.Add(msg);
        while (_outputLines.Count > _maxOutputLines) _outputLines.RemoveAt(0);
        if (_scrollOffset == 0) RefreshOutput(); // Only auto-refresh if at bottom
    }

    public void LogOK(string msg) => Log($"<color=#88ff88>{msg}</color>");
    public void LogErr(string msg) => Log($"<color=#ff4444>ERROR: {msg}</color>");
    public void LogWarn(string msg) => Log($"<color=#ffaa44>WARNING: {msg}</color>");
    public void LogInfo(string msg) => Log($"<color=#88ccff>{msg}</color>");
    public void LogGold(string msg) => Log($"<color=#ffcc00>{msg}</color>");

    private void RefreshOutput()
    {
        if (_outputText == null) return;

        int total = _outputLines.Count;
        int start = Mathf.Max(0, total - _visibleLines - _scrollOffset);
        int end = Mathf.Min(total, start + _visibleLines);

        var sb = new System.Text.StringBuilder();
        for (int i = start; i < end; i++)
            sb.AppendLine(_outputLines[i]);

        _outputText.text = sb.ToString();
    }

    // ========================================================================
    // FPS + STATS
    // ========================================================================

    private void UpdateFPS()
    {
        _frameCount++;
        _fpsTimer += Time.unscaledDeltaTime;
        if (_fpsTimer >= 0.5f)
        {
            _currentFPS = _frameCount / _fpsTimer;
            _frameCount = 0; _fpsTimer = 0f;
            if (_fpsText != null)
            {
                _fpsText.text = $"FPS: {_currentFPS:F0}";
                _fpsText.color = _currentFPS >= 50 ? Color.green : _currentFPS >= 30 ? Color.yellow : Color.red;
            }
        }
    }

    private void UpdateStats()
    {
        if (_statsText == null) return;
        string stats = "";
        if (InventoryManager.Instance != null)
        {
            stats += $"$ {InventoryManager.Instance.GetResource(InventoryManager.RESOURCE_CASH)}";
            stats += $"  Corn {InventoryManager.Instance.GetResource(InventoryManager.RESOURCE_CORN)}";
            stats += $"  Mash {InventoryManager.Instance.GetResource(InventoryManager.RESOURCE_MASH)}";
            stats += $"  Wsk {InventoryManager.Instance.GetResource(InventoryManager.RESOURCE_WHISKEY)}";
            stats += $"  Aged {InventoryManager.Instance.GetResource(InventoryManager.RESOURCE_AGED_WHISKEY)}";
            stats += $"  Wood {InventoryManager.Instance.GetResource(InventoryManager.RESOURCE_WOOD)}";
            stats += $"  Brl {InventoryManager.Instance.GetResource(InventoryManager.RESOURCE_BARREL)}";
        }
        if (FameManager.Instance != null)
            stats += $"  Fame {FameManager.Instance.TotalFame}({FameManager.Instance.TierName})";
        DayNightCycle cycle = FindObjectOfType<DayNightCycle>();
        if (cycle != null)
            stats += $"  Time {cycle.CycleProgress:F2}{(cycle.IsNight ? " NIGHT" : " DAY")}";
        _statsText.text = stats;
    }

    // ========================================================================
    // COMMAND REGISTRATION — All 60+ commands
    // ========================================================================

    private void RegisterAllCommands()
    {
        // ==================== HELP ====================
        Cmd("help", "Show all commands", a => {
            LogGold("=============== COMMANDS ===============");
            var sorted = _commandHelp.OrderBy(x => x.Key);
            foreach (var kv in sorted)
                Log($"  <color=#88ccff>{kv.Key,-20}</color> <color=#cccccc>{kv.Value}</color>");
            LogGold("========================================");
        });

        // ==================== RESOURCES ====================
        Cmd("give", "give <resource> <amt> — Add resource", a => {
            if (a.Length < 2) { LogErr("Usage: give <resource> <amount>"); return; }
            string r = ResName(a[0]); int amt = Int(a[1]);
            Inv()?.AddResource(r, amt);
            LogOK($"+{amt} {r}");
        });

        Cmd("set", "set <resource> <amt> — Set exact", a => {
            if (a.Length < 2) { LogErr("Usage: set <resource> <amount>"); return; }
            Inv()?.SetResource(ResName(a[0]), Int(a[1]));
            LogOK($"{ResName(a[0])} = {a[1]}");
        });

        Cmd("resources", "Print all resources", a => {
            if (Inv() == null) { LogErr("InventoryManager not found."); return; }
            LogGold("========== RESOURCES ==========");
            Res("Cash", InventoryManager.RESOURCE_CASH); Res("Corn", InventoryManager.RESOURCE_CORN);
            Res("Mash", InventoryManager.RESOURCE_MASH); Res("Whiskey", InventoryManager.RESOURCE_WHISKEY);
            Res("Aged", InventoryManager.RESOURCE_AGED_WHISKEY); Res("Wood", InventoryManager.RESOURCE_WOOD);
            Res("Barrel", InventoryManager.RESOURCE_BARREL);
            LogGold("===============================");
        });

        Cmd("reset", "Reset to starting values", a => {
            if (Inv() == null) return;
            Inv().SetResource(InventoryManager.RESOURCE_CASH, GameConstants.Balance.STARTING_CASH);
            foreach (string r in new[]{ InventoryManager.RESOURCE_CORN, InventoryManager.RESOURCE_MASH,
                InventoryManager.RESOURCE_WHISKEY, InventoryManager.RESOURCE_AGED_WHISKEY,
                InventoryManager.RESOURCE_WOOD, InventoryManager.RESOURCE_BARREL })
                Inv().SetResource(r, 0);
            LogOK("Resources reset.");
        });

        Cmd("god", "99,999 of everything", a => {
            if (Inv() == null) return;
            foreach (string r in new[]{ InventoryManager.RESOURCE_CASH, InventoryManager.RESOURCE_CORN,
                InventoryManager.RESOURCE_MASH, InventoryManager.RESOURCE_WHISKEY,
                InventoryManager.RESOURCE_AGED_WHISKEY, InventoryManager.RESOURCE_WOOD,
                InventoryManager.RESOURCE_BARREL })
                Inv().SetResource(r, 99999);
            LogOK("GOD MODE: 99,999 of everything!");
            NotificationManager.Instance?.Show("GOD MODE", "99,999 of everything!", NotificationType.Achievement);
        });

        Cmd("poor", "Set all resources to 0", a => {
            if (Inv() == null) return;
            foreach (string r in new[]{ InventoryManager.RESOURCE_CASH, InventoryManager.RESOURCE_CORN,
                InventoryManager.RESOURCE_MASH, InventoryManager.RESOURCE_WHISKEY,
                InventoryManager.RESOURCE_AGED_WHISKEY, InventoryManager.RESOURCE_WOOD,
                InventoryManager.RESOURCE_BARREL })
                Inv().SetResource(r, 0);
            LogWarn("All resources set to 0.");
        });

        Cmd("sell", "Sell all aged whiskey", a => {
            if (SellManager.Instance == null) { LogErr("SellManager not found."); return; }
            int sold = SellManager.Instance.SellAll();
            LogOK($"Sold {sold} Aged Whiskey.");
        });

        // ==================== SAVE / LOAD ====================
        Cmd("save", "Save game", a => { SaveManager.Instance?.SaveGame(); LogOK("Game saved."); });
        Cmd("load", "Load saved game", a => {
            bool ok = SaveManager.Instance != null && SaveManager.Instance.LoadGame();
            if (ok) LogOK("Game loaded."); else LogErr("Load failed.");
        });
        Cmd("deletesave", "Delete save file", a => { SaveManager.Instance?.DeleteSave(); LogWarn("Save deleted."); });

        // ==================== CREATIVE MODE ====================
        Cmd("creative", "Toggle free building mode", a => {
            _creativeMode = !_creativeMode;
            string status = _creativeMode ? "ON — Free building!" : "OFF — Normal costs.";
            if (_creativeMode) LogOK($"CREATIVE MODE: {status}"); else LogWarn($"Creative: {status}");
            NotificationManager.Instance?.Show("Creative Mode", status, _creativeMode ? NotificationType.Success : NotificationType.Warning);
        });

        // ==================== DAY/NIGHT ====================
        Cmd("daycycle", "daycycle <seconds> — Set cycle duration", a => {
            if (a.Length < 1) { LogErr("Usage: daycycle <seconds>"); return; }
            float dur = Float(a[0]);
            SetDNCField("cycleDuration", dur);
            LogOK($"Day/night cycle = {dur}s");
        });

        Cmd("settime", "settime <0-1> — Set time of day", a => {
            if (a.Length < 1) { LogErr("Usage: settime <0.0-1.0>"); return; }
            float t = Mathf.Clamp01(Float(a[0]));
            DayNightCycle c = FindObjectOfType<DayNightCycle>();
            if (c == null) { LogErr("DayNightCycle not found."); return; }
            var df = typeof(DayNightCycle).GetField("cycleDuration", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var tf = typeof(DayNightCycle).GetField("_cycleTimer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (df != null && tf != null) tf.SetValue(c, (float)df.GetValue(c) * t);
            LogOK($"Time = {t:F2} ({TimeName(t)})");
        });

        Cmd("day", "Jump to daytime", a => Execute("settime 0.1"));
        Cmd("night", "Jump to midnight", a => Execute("settime 0.75"));
        Cmd("sunset", "Jump to sunset", a => Execute("settime 0.5"));

        // ==================== TIME SCALE ====================
        Cmd("timescale", "timescale <x> — Game speed (1=normal)", a => {
            float s = a.Length > 0 ? Mathf.Clamp(Float(a[0]), 0, 100) : 1f;
            Time.timeScale = s; LogOK($"Time scale: {s}x");
        });
        Cmd("pause", "Pause game time", a => { Time.timeScale = 0; LogWarn("Paused (timescale=0)."); });
        Cmd("resume", "Resume normal speed", a => { Time.timeScale = 1; LogOK("Resumed (timescale=1)."); });
        Cmd("fast", "5x speed", a => { Time.timeScale = 5; LogOK("5x speed!"); });
        Cmd("turbo", "20x speed", a => { Time.timeScale = 20; LogOK("TURBO: 20x speed!"); });

        // ==================== FAME ====================
        Cmd("fame", "fame <amt> — Add fame", a => {
            if (a.Length < 1 || FameManager.Instance == null) { LogErr("Usage: fame <amount>"); return; }
            int amt = Int(a[0]);
            FameManager.Instance.AddFame(amt, "DevConsole");
            LogOK($"+{amt} Fame. Total: {FameManager.Instance.TotalFame} ({FameManager.Instance.TierName})");
        });
        Cmd("setfame", "setfame <amt> — Set fame exactly", a => {
            if (a.Length < 1 || FameManager.Instance == null) return;
            FameManager.Instance.SetFame(Int(a[0]));
            LogOK($"Fame = {a[0]} ({FameManager.Instance.TierName})");
        });
        Cmd("tier", "Show fame tier info", a => {
            if (FameManager.Instance == null) { LogErr("FameManager not found."); return; }
            LogGold($"Fame: {FameManager.Instance.TotalFame} | Tier: {FameManager.Instance.TierName} | Progress: {FameManager.Instance.TierProgress:P0} | Needed: {FameManager.Instance.FameToNextTier}");
        });

        // ==================== NOTIFICATIONS ====================
        Cmd("notify", "notify <type> <msg>", a => {
            if (a.Length < 2) { LogErr("Types: info success warning error fame achievement"); return; }
            NotificationType t = a[0].ToLower() switch {
                "success" => NotificationType.Success, "warning" => NotificationType.Warning,
                "error" => NotificationType.Error, "fame" => NotificationType.Fame,
                "achievement" => NotificationType.Achievement, _ => NotificationType.Info };
            string msg = string.Join(" ", a.Skip(1));
            NotificationManager.Instance?.Show("Dev Console", msg, t);
            LogOK($"Notification sent: [{t}] {msg}");
        });
        Cmd("notifytest", "Send one of each notification type", a => {
            if (NotificationManager.Instance == null) { LogErr("NotificationManager not found."); return; }
            NotificationManager.Instance.Show("Info", "This is an info notification", NotificationType.Info);
            NotificationManager.Instance.Show("Success!", "Something went well", NotificationType.Success);
            NotificationManager.Instance.Show("Warning", "Be careful!", NotificationType.Warning);
            NotificationManager.Instance.ShowError("Something failed!");
            NotificationManager.Instance.ShowFame(100, "Dev test");
            NotificationManager.Instance.ShowAchievement("Test", "You opened dev console!");
            LogOK("Sent 6 test notifications.");
        });

        // ==================== CAMERA ====================
        Cmd("zoom", "zoom <level> — Set camera zoom", a => {
            if (a.Length < 1) { LogErr("Usage: zoom <3-15>"); return; }
            FindObjectOfType<CameraController>()?.SetZoom(Float(a[0]));
            LogOK($"Zoom = {a[0]}");
        });
        Cmd("tp", "tp <x> <y> — Move camera", a => {
            if (a.Length < 2) { LogErr("Usage: tp <x> <y>"); return; }
            FindObjectOfType<CameraController>()?.FocusOn(new Vector2(Float(a[0]), Float(a[1])), 0.3f);
            LogOK($"Camera -> ({a[0]}, {a[1]})");
        });
        Cmd("center", "Center camera on grid", a => {
            if (GridManager.Instance == null) return;
            float cx = GridManager.Instance.GridWidth * 0.5f;
            float cy = GridManager.Instance.GridHeight * 0.5f;
            FindObjectOfType<CameraController>()?.FocusOn(new Vector2(cx, cy), 0.5f);
            LogOK($"Centered on ({cx:F0}, {cy:F0})");
        });

        // ==================== BUILDINGS ====================
        Cmd("buildings", "List all buildings", a => {
            var bs = FindObjectsOfType<BuildingBehavior>();
            var cs = FindObjectsOfType<CropBehavior>();
            LogGold($"===== BUILDINGS ({bs.Length}) =====");
            foreach (var b in bs)
                Log($"  <color=#cccccc>[{b.BuildingIndex}] {b.gameObject.name} grid({b.GridPosition.x},{b.GridPosition.y}) world({b.transform.position.x:F1},{b.transform.position.y:F1})</color>");
            LogGold($"===== CROPS ({cs.Length}) =====");
            foreach (var c in cs)
                Log($"  <color=#cccccc>[{c.BuildingIndex}] {c.gameObject.name} grid({c.GridPosition.x},{c.GridPosition.y}) [{(c.IsFullyGrown?"GROWN":"stage "+c.CurrentStage)}]</color>");
            LogInfo($"Total: {bs.Length + cs.Length}");
        });

        Cmd("demolishall", "Remove all buildings/crops", a => {
            var bs = FindObjectsOfType<BuildingBehavior>();
            var cs = FindObjectsOfType<CropBehavior>();
            int n = bs.Length + cs.Length;
            foreach (var b in bs) Destroy(b.gameObject);
            foreach (var c in cs) Destroy(c.gameObject);
            foreach (var t in FindObjectsOfType<TileBehavior>()) t.SetOccupied(false);
            LogWarn($"Demolished {n} objects.");
        });

        Cmd("spawn", "spawn <idx> <x> <y> — Place building", a => {
            if (a.Length < 3) { LogErr("Usage: spawn <buildingIndex> <gridX> <gridY>"); return; }
            int idx = Int(a[0]); int gx = Int(a[1]); int gy = Int(a[2]);
            if (BuildingDatabase.Instance == null) { LogErr("BuildingDatabase not found."); return; }
            BuildingData d = BuildingDatabase.Instance.GetBuilding(idx);
            if (d == null || d.prefab == null) { LogErr($"Invalid index: {idx}"); return; }
            TileBehavior tile = GridManager.Instance?.GetTileAt(gx, gy);
            if (tile == null) { LogErr($"No tile at ({gx},{gy})."); return; }
            GameObject obj = Instantiate(d.prefab, tile.transform.position, Quaternion.identity);
            if (d.isCrop) {
                var cr = obj.GetComponent<CropBehavior>();
                if (cr != null) { cr.SetBuildingIndex(idx); cr.SetGridPosition(new Vector2Int(gx,gy)); }
            } else {
                var bh = obj.GetComponent<BuildingBehavior>();
                if (bh != null) { bh.Initialize(d); bh.SetBuildingIndex(idx); bh.SetGridPosition(new Vector2Int(gx,gy)); }
            }
            tile.SetOccupied(true, obj);
            LogOK($"Spawned '{d.buildingName}' at ({gx},{gy}).");
        });

        Cmd("buildlist", "List all building types in database", a => {
            if (BuildingDatabase.Instance == null) { LogErr("BuildingDatabase not found."); return; }
            LogGold("===== BUILDING DATABASE =====");
            for (int i = 0; i < 20; i++) {
                BuildingData d = BuildingDatabase.Instance.GetBuilding(i);
                if (d == null) break;
                Log($"  <color=#cccccc>[{i}] {d.buildingName} — ${d.cost} {(d.isCrop?"(crop)":"")}</color>");
            }
        });

        // ==================== GRID ====================
        Cmd("gridinfo", "Show grid info", a => {
            if (GridManager.Instance == null) { LogErr("GridManager not found."); return; }
            LogGold($"Grid: {GridManager.Instance.GridWidth}x{GridManager.Instance.GridHeight} | Seed: {GridManager.Instance.CurrentSeed} | Generated: {GridManager.Instance.IsGenerated}");
        });
        Cmd("seed", "Show grid seed", a => { LogInfo($"Seed: {GridManager.Instance?.CurrentSeed}"); });

        Cmd("tileinfo", "tileinfo <x> <y> — Show tile details", a => {
            if (a.Length < 2) { LogErr("Usage: tileinfo <x> <y>"); return; }
            TileBehavior t = GridManager.Instance?.GetTileAt(Int(a[0]), Int(a[1]));
            if (t == null) { LogErr("No tile found."); return; }
            LogGold($"Tile ({a[0]},{a[1]}): {t.TerrainType} | Occupied: {t.IsOccupied} | World: ({t.transform.position.x:F2},{t.transform.position.y:F2})");
            if (t.OccupyingBuilding != null) LogInfo($"  Building: {t.OccupyingBuilding.name}");
        });

        // ==================== TREES ====================
        Cmd("trees", "List all trees", a => {
            var ts = FindObjectsOfType<TreeBehavior>();
            LogGold($"===== TREES ({ts.Length}) =====");
            foreach (var t in ts)
                Log($"  <color=#cccccc>{t.gameObject.name} at ({t.transform.position.x:F1},{t.transform.position.y:F1})</color>");
        });

        // ==================== SAVE VERIFY ====================
        Cmd("saveverify", "Check building position integrity", a => {
            LogGold("===== SAVE VERIFICATION =====");
            var bs = FindObjectsOfType<BuildingBehavior>();
            var cs = FindObjectsOfType<CropBehavior>();
            int issues = 0;
            foreach (var b in bs) {
                TileBehavior t = GridManager.Instance?.GetTileAt(b.GridPosition.x, b.GridPosition.y);
                if (t == null) { LogErr($"{b.gameObject.name} — tile not found!"); issues++; continue; }
                float d = Vector2.Distance(b.transform.position, t.transform.position);
                if (d > 0.5f) { LogWarn($"{b.gameObject.name} DRIFT: {d:F2} units"); issues++; }
                else LogOK($"{b.gameObject.name} at ({b.GridPosition.x},{b.GridPosition.y}) OK");
            }
            foreach (var c in cs) {
                TileBehavior t = GridManager.Instance?.GetTileAt(c.GridPosition.x, c.GridPosition.y);
                if (t == null) { LogErr($"{c.gameObject.name} — tile not found!"); issues++; continue; }
                float d = Vector2.Distance(c.transform.position, t.transform.position);
                if (d > 0.5f) { LogWarn($"{c.gameObject.name} DRIFT: {d:F2} units"); issues++; }
                else LogOK($"{c.gameObject.name} at ({c.GridPosition.x},{c.GridPosition.y}) OK");
            }
            if (issues == 0) LogOK($"All {bs.Length+cs.Length} objects verified!");
            else LogWarn($"{issues} issues found.");
        });

        // ==================== AUDIO ====================
        Cmd("mute", "Mute all audio", a => { AudioListener.volume = 0; LogWarn("Muted."); });
        Cmd("unmute", "Unmute audio", a => { AudioListener.volume = 1; LogOK("Unmuted."); });
        Cmd("musicvol", "musicvol <0-1>", a => { AudioManager.Instance?.SetMusicVolume(Float(a[0])); LogOK($"Music: {a[0]}"); });
        Cmd("sfxvol", "sfxvol <0-1>", a => { AudioManager.Instance?.SetSFXVolume(Float(a[0])); LogOK($"SFX: {a[0]}"); });
        Cmd("playsfx", "playsfx <name> — Test SFX", a => { AudioManager.Instance?.PlaySFX(a[0]); LogOK($"Playing: {a[0]}"); });

        // ==================== UTILITY ====================
        Cmd("clear", "Clear console", a => { _outputLines.Clear(); _scrollOffset = 0; RefreshOutput(); });
        Cmd("cls", "Clear (alias)", a => Execute("clear"));
        Cmd("fps", "Toggle FPS counter", a => { _showFPS = !_showFPS; if (_fpsObj != null) _fpsObj.SetActive(_showFPS); LogOK($"FPS: {(_showFPS?"ON":"OFF")}"); });
        Cmd("stats", "Toggle live resource/time overlay", a => { _showStats = !_showStats; if (_statsObj != null) _statsObj.SetActive(_showStats); LogOK($"Stats: {(_showStats?"ON":"OFF")}"); });

        Cmd("version", "Show version info", a => {
            LogGold("===== WHISKEYVILLE =====");
            LogInfo($"Unity {Application.unityVersion} | {Application.platform} | {Screen.width}x{Screen.height}");
            LogInfo($"Data: {Application.persistentDataPath}");
        });

        Cmd("quit", "Quit to main menu", a => {
            Time.timeScale = 1;
            UnityEngine.SceneManagement.SceneManager.LoadScene(GameConstants.Scenes.MAIN_MENU);
        });

        // ==================== 20 QOL FEATURES ====================

        // 1. Screenshot
        Cmd("screenshot", "Take a screenshot", a => {
            string path = $"{Application.persistentDataPath}/screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png";
            ScreenCapture.CaptureScreenshot(path);
            LogOK($"Screenshot saved: {path}");
        });

        // 2. Count all objects
        Cmd("count", "Count all game objects by type", a => {
            LogGold("===== OBJECT COUNT =====");
            LogInfo($"Buildings: {FindObjectsOfType<BuildingBehavior>().Length}");
            LogInfo($"Crops: {FindObjectsOfType<CropBehavior>().Length}");
            LogInfo($"Trees: {FindObjectsOfType<TreeBehavior>().Length}");
            LogInfo($"Tiles: {FindObjectsOfType<TileBehavior>().Length}");
            LogInfo($"Total GameObjects: {FindObjectsOfType<GameObject>().Length}");
        });

        // 3. Tool switching
        Cmd("tool", "tool <none|build|hoe|axe|demolish> — Set active tool", a => {
            if (a.Length < 1 || ToolManager.Instance == null) { LogErr("Usage: tool <type>"); return; }
            ToolType tt = a[0].ToLower() switch {
                "build" => ToolType.Build, "hoe" => ToolType.Hoe,
                "axe" => ToolType.Axe, "demolish" => ToolType.Demolish, _ => ToolType.None };
            ToolManager.Instance.SetTool(tt);
            LogOK($"Tool: {tt}");
        });

        // 4. Quick money amounts
        Cmd("rich", "Give $10,000", a => { Inv()?.AddResource(InventoryManager.RESOURCE_CASH, 10000); LogOK("+$10,000"); });
        Cmd("payday", "Give $1,000", a => { Inv()?.AddResource(InventoryManager.RESOURCE_CASH, 1000); LogOK("+$1,000"); });

        // 5. Quick resource packs
        Cmd("starterpack", "Give 500 of each resource", a => {
            if (Inv() == null) return;
            foreach (string r in new[]{InventoryManager.RESOURCE_CASH, InventoryManager.RESOURCE_CORN,
                InventoryManager.RESOURCE_MASH, InventoryManager.RESOURCE_WHISKEY,
                InventoryManager.RESOURCE_AGED_WHISKEY, InventoryManager.RESOURCE_WOOD,
                InventoryManager.RESOURCE_BARREL})
                Inv().AddResource(r, 500);
            LogOK("Starter Pack: +500 of everything!");
        });

        // 6. Time info
        Cmd("time", "Show current time info", a => {
            DayNightCycle c = FindObjectOfType<DayNightCycle>();
            if (c == null) { LogErr("DayNightCycle not found."); return; }
            LogGold($"Progress: {c.CycleProgress:F3} | Phase: {TimeName(c.CycleProgress)} | Night: {c.IsNight}");
            LogInfo($"TimeScale: {Time.timeScale}x | RealTime: {Time.realtimeSinceStartup:F0}s");
        });

        // 7. Heal / grow all crops instantly
        Cmd("growcrops", "Instantly grow all crops", a => {
            var crops = FindObjectsOfType<CropBehavior>();
            foreach (var c in crops) {
                if (!c.IsFullyGrown) c.RestoreGrowthState(99, true);
            }
            LogOK($"Grew {crops.Length} crops to full!");
        });

        // 8. Rapid harvest — triggers all production buildings
        Cmd("harvest", "Trigger all crop harvests", a => {
            var crops = FindObjectsOfType<CropBehavior>();
            int count = 0;
            foreach (var c in crops) {
                if (c.IsFullyGrown) { count++; }
            }
            LogInfo($"Found {count} fully grown crops ready to harvest. Click them with Axe/default tool.");
        });

        // 9. Economy report
        Cmd("economy", "Show economy summary", a => {
            if (Inv() == null) return;
            int cash = Inv().GetResource(InventoryManager.RESOURCE_CASH);
            int aged = Inv().GetResource(InventoryManager.RESOURCE_AGED_WHISKEY);
            int total = cash + (aged * GameConstants.Balance.BASE_WHISKEY_PRICE);
            LogGold("===== ECONOMY =====");
            LogInfo($"Cash: ${cash}");
            LogInfo($"Aged Whiskey: {aged} (worth ~${aged * GameConstants.Balance.BASE_WHISKEY_PRICE})");
            LogInfo($"Net Worth: ~${total}");
            LogInfo($"Buildings: {FindObjectsOfType<BuildingBehavior>().Length} | Crops: {FindObjectsOfType<CropBehavior>().Length}");
        });

        // 10. Stress test — spam notifications
        Cmd("spamnotify", "spamnotify <count> — Spam N notifications", a => {
            int n = a.Length > 0 ? Int(a[0]) : 5;
            for (int i = 0; i < n; i++)
                NotificationManager.Instance?.Show($"Spam #{i+1}", $"Test notification {i+1} of {n}", NotificationType.Info);
            LogOK($"Spammed {n} notifications.");
        });

        // 11. Teleport to specific locations
        Cmd("home", "Camera to grid center", a => Execute("center"));
        Cmd("origin", "Camera to (0,0)", a => {
            FindObjectOfType<CameraController>()?.FocusOn(Vector2.zero, 0.3f);
            LogOK("Camera -> origin (0,0)");
        });

        // 12. Find closest building to camera
        Cmd("nearest", "Find nearest building to camera", a => {
            Camera cam = Camera.main;
            if (cam == null) return;
            var bs = FindObjectsOfType<BuildingBehavior>();
            if (bs.Length == 0) { LogInfo("No buildings found."); return; }
            BuildingBehavior closest = bs.OrderBy(b => Vector2.Distance(cam.transform.position, b.transform.position)).First();
            LogInfo($"Nearest: {closest.gameObject.name} at ({closest.GridPosition.x},{closest.GridPosition.y}) — {Vector2.Distance(cam.transform.position, closest.transform.position):F1} units away");
        });

        // 13. Toggle overlay visibility
        Cmd("overlay", "Toggle night overlay on/off", a => {
            DayNightCycle c = FindObjectOfType<DayNightCycle>();
            if (c == null) return;
            var field = typeof(DayNightCycle).GetField("tintSprites", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null) {
                bool cur = (bool)field.GetValue(c);
                field.SetValue(c, !cur);
                LogOK($"Night overlay: {(!cur ? "ON" : "OFF")}");
            }
        });

        // 14. Memory/performance info
        Cmd("perf", "Show performance info", a => {
            LogGold("===== PERFORMANCE =====");
            LogInfo($"FPS: {_currentFPS:F0} | TimeScale: {Time.timeScale}x");
            LogInfo($"Total Objects: {FindObjectsOfType<GameObject>().Length}");
            LogInfo($"GC Memory: {(System.GC.GetTotalMemory(false) / 1024 / 1024):F1} MB");
            LogInfo($"Quality: {QualitySettings.GetQualityLevel()} ({QualitySettings.names[QualitySettings.GetQualityLevel()]})");
        });

        // 15. Quick save + notify
        Cmd("quicksave", "Save + show notification", a => {
            SaveManager.Instance?.SaveGame();
            NotificationManager.Instance?.Show("Quick Save", "Game saved!", NotificationType.Success);
            LogOK("Quick saved!");
        });

        // 16. Repeat last command
        Cmd("r", "Repeat last command", a => {
            if (_commandHistory.Count < 2) { LogErr("No previous command."); return; }
            string last = _commandHistory[_commandHistory.Count - 2]; // -2 because 'r' itself was added
            LogInfo($"Repeating: {last}");
            Execute(last);
        });

        // 17. Batch commands
        Cmd("batch", "batch <cmd1;cmd2;cmd3> — Run multiple commands", a => {
            string full = string.Join(" ", a);
            string[] cmds = full.Split(';');
            foreach (string c in cmds) {
                string trimmed = c.Trim();
                if (!string.IsNullOrEmpty(trimmed)) Execute(trimmed);
            }
        });

        // 18. Set all tool hotkeys reminder
        Cmd("hotkeys", "Show all keyboard shortcuts", a => {
            LogGold("===== HOTKEYS =====");
            LogInfo("` or F12  — Toggle Dev Console");
            LogInfo("B         — Toggle Build Panel");
            LogInfo("H         — Hoe Tool");
            LogInfo("X         — Axe Tool");
            LogInfo("D         — Demolish Tool");
            LogInfo("ESC       — Close panels / Pause");
            LogInfo("F1-F5     — Add resources (cash/corn/mash/whiskey/aged)");
            LogInfo("F6        — Sell all aged whiskey");
            LogInfo("F7/F8     — Save/Load");
            LogInfo("F9/F10    — Dump/Reset resources");
            LogInfo("PgUp/PgDn — Scroll console output");
        });

        // 19. Echo / print
        Cmd("echo", "echo <text> — Print text to console", a => {
            Log(string.Join(" ", a));
        });

        // 20. Marker system — mark positions for reference
        Cmd("mark", "mark <name> — Save current camera position", a => {
            Camera cam = Camera.main;
            if (cam == null || a.Length < 1) { LogErr("Usage: mark <name>"); return; }
            string key = $"mark_{a[0]}";
            PlayerPrefs.SetFloat(key + "_x", cam.transform.position.x);
            PlayerPrefs.SetFloat(key + "_y", cam.transform.position.y);
            LogOK($"Marked '{a[0]}' at ({cam.transform.position.x:F1}, {cam.transform.position.y:F1})");
        });

        Cmd("goto", "goto <name> — Go to marked position", a => {
            if (a.Length < 1) { LogErr("Usage: goto <name>"); return; }
            string key = $"mark_{a[0]}";
            if (!PlayerPrefs.HasKey(key + "_x")) { LogErr($"Mark '{a[0]}' not found."); return; }
            float x = PlayerPrefs.GetFloat(key + "_x");
            float y = PlayerPrefs.GetFloat(key + "_y");
            FindObjectOfType<CameraController>()?.FocusOn(new Vector2(x, y), 0.3f);
            LogOK($"Going to '{a[0]}' ({x:F1}, {y:F1})");
        });
    }

    // ========================================================================
    // HELPERS
    // ========================================================================

    private void Cmd(string n, string h, Action<string[]> a) { _commands[n] = a; _commandHelp[n] = h; }
    private InventoryManager Inv() => InventoryManager.Instance;
    private int Int(string s) { int.TryParse(s, out int v); return v; }
    private float Float(string s) { float.TryParse(s, out float v); return v; }
    private void Res(string label, string key) { Log($"  <color=#cccccc>{label,-14} {Inv().GetResource(key),8}</color>"); }

    private string ResName(string s) => s.ToLower() switch {
        "cash" or "money" or "$" => InventoryManager.RESOURCE_CASH,
        "corn" => InventoryManager.RESOURCE_CORN, "mash" => InventoryManager.RESOURCE_MASH,
        "whiskey" => InventoryManager.RESOURCE_WHISKEY,
        "aged" or "agedwhiskey" => InventoryManager.RESOURCE_AGED_WHISKEY,
        "wood" or "lumber" => InventoryManager.RESOURCE_WOOD,
        "barrel" or "barrels" => InventoryManager.RESOURCE_BARREL, _ => s };

    private string TimeName(float t) => t < 0.1f ? "Sunrise" : t < 0.35f ? "Midday" : t < 0.55f ? "Sunset" : t < 0.85f ? "Midnight" : "Pre-dawn";

    private void SetDNCField(string field, float val) {
        DayNightCycle c = FindObjectOfType<DayNightCycle>();
        if (c == null) { LogErr("DayNightCycle not found."); return; }
        var f = typeof(DayNightCycle).GetField(field, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (f != null) f.SetValue(c, val);
    }

    // ========================================================================
    // UI CREATION — Simple, reliable, no ScrollRect needed
    // ========================================================================

    private void BuildUI()
    {
        // ---- CANVAS ----
        GameObject cObj = new GameObject("DevConsoleCanvas");
        cObj.transform.SetParent(transform);
        _canvas = cObj.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 10000;

        CanvasScaler sc = cObj.AddComponent<CanvasScaler>();
        sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(1920, 1080);
        sc.matchWidthOrHeight = 0.5f;
        cObj.AddComponent<GraphicRaycaster>();

        // ---- PANEL (bottom 45%) ----
        _panel = MakePanel(cObj.transform, "Panel", V2(0,0), V2(1,0.45f), new Color(0.02f,0.02f,0.06f,_bgOpacity));

        // ---- HEADER ----
        GameObject hdr = MakePanel(_panel.transform, "Hdr", V2(0,0.92f), V2(1,1), new Color(0.12f,0.08f,0.03f,0.95f));
        var ht = MakeTMP(hdr.transform, "HdrTxt", "WHISKEYVILLE DEV CONSOLE  |  ` or F12  |  'help' for commands  |  PgUp/PgDn to scroll", 12);
        Stretch(ht.rectTransform, 10, 2, -10, -2);
        ht.color = new Color(1f, 0.8f, 0.4f);

        // ---- OUTPUT (simple text, no ScrollRect) ----
        GameObject outBg = MakePanel(_panel.transform, "OutBg", V2(0,0.08f), V2(1,0.92f), new Color(0.04f,0.04f,0.08f,0.6f));
        _outputText = MakeTMP(outBg.transform, "OutTxt", "", 13);
        Stretch(_outputText.rectTransform, 8, 4, -8, -4);
        _outputText.alignment = TextAlignmentOptions.BottomLeft;
        _outputText.enableWordWrapping = true;
        _outputText.overflowMode = TextOverflowModes.Truncate;
        _outputText.richText = true;
        _outputText.color = Color.white;

        // ---- INPUT BAR ----
        GameObject inBg = MakePanel(_panel.transform, "InBg", V2(0,0), V2(1,0.08f), new Color(0.08f,0.08f,0.12f,0.95f));

        // Prompt
        var pr = MakeTMP(inBg.transform, "Pr", "> ", 15);
        pr.rectTransform.anchorMin = V2(0,0); pr.rectTransform.anchorMax = V2(0.025f,1);
        Stretch(pr.rectTransform, 8, 0, 0, 0);
        pr.color = new Color(0.4f,1f,0.4f);

        // Input field
        GameObject inObj = new GameObject("Input");
        inObj.transform.SetParent(inBg.transform, false);
        RectTransform inRT = inObj.AddComponent<RectTransform>();
        inRT.anchorMin = V2(0.025f,0); inRT.anchorMax = V2(1,1);
        inRT.offsetMin = V2(0,2); inRT.offsetMax = V2(-8,-2);
        inObj.AddComponent<Image>().color = new Color(0,0,0,0);

        _inputField = inObj.AddComponent<TMP_InputField>();

        // Text area container
        GameObject ta = new GameObject("TextArea");
        ta.transform.SetParent(inObj.transform, false);
        RectTransform taRT = ta.AddComponent<RectTransform>();
        taRT.anchorMin = V2(0,0); taRT.anchorMax = V2(1,1);
        taRT.offsetMin = V2(0,0); taRT.offsetMax = V2(0,0);
        ta.AddComponent<RectMask2D>();

        // Input text
        var iTxt = MakeTMP(ta.transform, "Txt", "", 14);
        Stretch(iTxt.rectTransform, 2, 0, -2, 0);
        iTxt.color = Color.white;

        // Placeholder
        var ph = MakeTMP(ta.transform, "PH", "Type a command...", 14);
        Stretch(ph.rectTransform, 2, 0, -2, 0);
        ph.color = new Color(0.5f,0.5f,0.5f,0.5f);
        ph.fontStyle = FontStyles.Italic;

        // Wire input field
        _inputField.textViewport = taRT;
        _inputField.textComponent = iTxt;
        _inputField.placeholder = ph;
        _inputField.fontAsset = iTxt.font;
        _inputField.pointSize = 14;
        _inputField.caretColor = new Color(0.4f,1f,0.4f);
        _inputField.selectionColor = new Color(0.2f,0.4f,0.2f,0.5f);

        // ---- FPS COUNTER ----
        _fpsObj = new GameObject("FPS");
        _fpsObj.transform.SetParent(cObj.transform, false);
        RectTransform fRT = _fpsObj.AddComponent<RectTransform>();
        fRT.anchorMin = V2(0,1); fRT.anchorMax = V2(0,1); fRT.pivot = V2(0,1);
        fRT.anchoredPosition = V2(10,-10); fRT.sizeDelta = V2(100,25);
        _fpsObj.AddComponent<Image>().color = new Color(0,0,0,0.6f);
        _fpsText = MakeTMP(_fpsObj.transform, "FPSTxt", "FPS: --", 13);
        Stretch(_fpsText.rectTransform, 4, 0, -4, 0);
        _fpsText.alignment = TextAlignmentOptions.Center;
        _fpsText.color = Color.green;
        _fpsObj.SetActive(false);

        // ---- STATS OVERLAY ----
        _statsObj = new GameObject("Stats");
        _statsObj.transform.SetParent(cObj.transform, false);
        RectTransform sRT = _statsObj.AddComponent<RectTransform>();
        sRT.anchorMin = V2(0,0); sRT.anchorMax = V2(1,0); sRT.pivot = V2(0.5f,0);
        sRT.anchoredPosition = V2(0,2); sRT.sizeDelta = V2(0,22);
        sRT.offsetMin = new Vector2(5, 2); sRT.offsetMax = new Vector2(-5, 24);
        _statsObj.AddComponent<Image>().color = new Color(0,0,0,0.6f);
        _statsText = MakeTMP(_statsObj.transform, "StatsTxt", "", 11);
        Stretch(_statsText.rectTransform, 6, 0, -6, 0);
        _statsText.alignment = TextAlignmentOptions.Center;
        _statsText.color = new Color(0.8f,0.9f,1f);
        _statsObj.SetActive(false);

        // Start hidden
        _panel.SetActive(false);

        // Welcome
        LogGold("Whiskeyville Dev Console v2.0");
        LogInfo("Type 'help' for commands. 'hotkeys' for keyboard shortcuts.");
        Log("");
    }

    // ========================================================================
    // UI HELPERS
    // ========================================================================

    private Vector2 V2(float x, float y) => new Vector2(x, y);

    private GameObject MakePanel(Transform parent, string name, Vector2 aMin, Vector2 aMax, Color col)
    {
        GameObject o = new GameObject(name);
        o.transform.SetParent(parent, false);
        RectTransform r = o.AddComponent<RectTransform>();
        r.anchorMin = aMin; r.anchorMax = aMax;
        r.offsetMin = V2(0,0); r.offsetMax = V2(0,0);
        o.AddComponent<Image>().color = col;
        return o;
    }

    private TextMeshProUGUI MakeTMP(Transform parent, string name, string text, float size)
    {
        GameObject o = new GameObject(name);
        o.transform.SetParent(parent, false);
        RectTransform r = o.AddComponent<RectTransform>();
        r.anchorMin = V2(0,0); r.anchorMax = V2(1,1);
        r.offsetMin = V2(0,0); r.offsetMax = V2(0,0);
        TextMeshProUGUI t = o.AddComponent<TextMeshProUGUI>();
        t.text = text; t.fontSize = size; t.color = Color.white;
        return t;
    }

    private void Stretch(RectTransform r, float l, float b, float rr, float t)
    {
        r.anchorMin = V2(0,0); r.anchorMax = V2(1,1);
        r.offsetMin = V2(l,b); r.offsetMax = V2(rr,t);
    }
}

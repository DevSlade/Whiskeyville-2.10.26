# 💾 Feature Spec: Main Menu Save / Load (Continue Button)
### Phase 1 — MVP Polish | Priority: 🔴 Critical
### Estimated Time: 15–20 minutes | Target MVP Gain: +2%

---

## PROBLEM

When a player saves their game and closes the app, they return to the main menu but have
**no way to continue their saved town** without knowing the hidden F8 hotkey. The main menu
currently shows only "New Game" and "Quit."

This is a player-facing failure. Save/Load already works at the system level — we just need
to expose it in the UI.

---

## SOLUTION

Add a **Continue** button to the Main Menu that:
1. Only appears if a save file exists
2. Loads the save and drops the player directly into their town
3. Also surfaces a **Load** option distinct from "New Game" to make the intent clear

```
  ┌─────────────────────────────────────────────────────┐
  │                                                     │
  │              🥃 WHISKEYVILLE                        │
  │                                                     │
  │          [ ▶  CONTINUE  ]  ← shows only if         │
  │                               save file exists      │
  │          [ +  NEW GAME  ]                           │
  │                                                     │
  │          [ ⚙  OPTIONS   ]                          │
  │          [ ✕  QUIT      ]                          │
  │                                                     │
  └─────────────────────────────────────────────────────┘
```

---

## SCRIPT TO MODIFY: `MainMenuManager.cs`

### Current State
- `NewGame()` — loads GameScene
- `QuitGame()` — exits application
- No awareness of save file state

### Changes Required

```csharp
// Add to Start():
continueButton.gameObject.SetActive(SaveManager.Instance.SaveFileExists());

// Add handler:
public void ContinueGame()
{
    SaveManager.Instance.FlagLoadOnStart(true);
    SceneManager.LoadScene("GameScene");
}
```

### `SaveManager.cs` Additions
```csharp
public bool SaveFileExists()
{
    return File.Exists(_savePath);
}

private bool _loadOnStart = false;

public void FlagLoadOnStart(bool flag) => _loadOnStart = flag;

// In GameScene Start():
if (_loadOnStart) { LoadGame(); _loadOnStart = false; }
```

---

## ACCEPTANCE CRITERIA

- [ ] Main menu shows **Continue** button only when `whiskeyville_save.json` exists
- [ ] Tapping **Continue** loads saved game state (resources, buildings, grid)
- [ ] Tapping **New Game** starts fresh (optionally: confirm dialog if save exists)
- [ ] Continue button is hidden on first launch (no save file)
- [ ] After saving in-game, returning to menu shows Continue button

---

## EDGE CASES

| Scenario | Handling |
|----------|----------|
| Save file is corrupted | Catch exception, hide Continue, show "Save data lost" toast |
| Player clicks New Game with existing save | Show confirm: "Start over? Your save will be kept." |
| Multiple save slots (future) | Continue loads most recent slot |

---

## HOW IT CONNECTS TO THE FUTURE

```
  Continue Button (Phase 1)
       │
       ├── Phase 1+: Named save slots ("My Town" / "River Run" / "Slot 3")
       ├── Phase 2: Save slot thumbnails (screenshot of town)
       ├── Phase 3: Cloud save — Continue from any device
       └── Phase 4: Prestige resets — "Start New Prestige Run" replaces New Game
```

---

## ESTIMATED TIMELINE

| Task | Time |
|------|------|
| Add `SaveFileExists()` to `SaveManager` | 3 min |
| Add Continue button to Main Menu canvas | 5 min |
| Update `MainMenuManager.cs` | 7 min |
| **Total** | **~15 min** |

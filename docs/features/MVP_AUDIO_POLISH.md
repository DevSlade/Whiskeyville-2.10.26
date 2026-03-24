# 🎵 Feature Spec: Audio Polish (Full SFX + Music Pass)
### Phase 1 — MVP Polish | Priority: 🟡 Medium
### Estimated Time: 30–60 minutes | Target MVP Gain: +2%

---

## CURRENT STATE

The `AudioManager.cs` singleton is built and functional. Inspector slots for 5 SFX clips
and 2 music tracks exist. Placeholder clips have been assigned (Feb 18). However:
- SFX clips do not match game vibe — they are generic placeholders
- No clip is triggered for several key player actions
- Music doesn't fade between main menu and gameplay
- No ambient/environmental sound layer

---

## AUDIO IDENTITY

Whiskeyville should feel like a **warm, rustic Southern town**. The audio identity is:

```
  TONE:       Dusty. Warm. Lived-in.
  MUSIC:      Acoustic guitar, banjo, slow percussion. Americana/folk.
  AMBIENCE:   Crickets at night. Wind in grass. Distant creek.
  SFX:        Satisfying. Tactile. A little chunky.
```

Not: electronic, synth, or high-energy. Think front-porch, not dance floor.

---

## SFX REQUIREMENTS

| Action | Sound | Priority | Notes |
|--------|-------|----------|-------|
| Building placed | Wooden thud / hammer tap | 🔴 Critical | Satisfying placement feel |
| Crop harvested | Rustle + pop | 🔴 Critical | Key feedback moment |
| Tree chopped | Axe chop (woody crack) | 🔴 Critical | Short, punchy |
| Item produced (Mash Tun, etc.) | Liquid pour / bubble | 🟡 Medium | Loopable if needed |
| Aged whiskey produced | Glass clink / deep resonant pour | 🔴 Critical | Premium feel |
| Sell (cash register) | Cash register / coin drop | 🔴 Critical | Most satisfying moment in loop |
| UI button tap | Light tap / click | 🟡 Medium | Subtle, not annoying |
| Sell panel open | Wooden creak (saloon doors) | 🟡 Medium | Flavor |
| Building demolish | Crash / wood fall | 🟡 Medium | Satisfying destruction |
| Cannot afford | Low bonk / denied tone | 🟡 Medium | Clear negative feedback |
| Day transition | Rooster crow / soft bell | 🟢 Low | Atmospheric |
| Night transition | Owl hoot / crickets swell | 🟢 Low | Atmospheric |
| Save game | Satisfying click / typewriter | 🟢 Low | Confidence feedback |

---

## MUSIC REQUIREMENTS

| Track | Scene | Style | Notes |
|-------|-------|-------|-------|
| Main Menu Theme | MainMenu | Slow acoustic guitar | Inviting, nostalgic |
| Gameplay Day | GameScene (day) | Up-tempo folk/bluegrass | Productive energy |
| Gameplay Night | GameScene (night) | Slower, atmospheric | Reflective |
| Victory sting | Sell moment | Short stab | 2–3 seconds, plays on big sell |

### Music System Improvements
```csharp
// AudioManager.cs additions:

[Header("Music")]
[SerializeField] private AudioClip _menuTheme;
[SerializeField] private AudioClip _gameplayDayTheme;
[SerializeField] private AudioClip _gameplayNightTheme;

// Crossfade between day/night (tie to DayNightCycle.cs events)
public void CrossfadeTo(AudioClip clip, float fadeDuration = 2f)
{
    StartCoroutine(CrossfadeCoroutine(clip, fadeDuration));
}
```

---

## AUDIO CLIP SOURCING GUIDE

Free, license-appropriate sources recommended:
- **freesound.org** — Vast library, check CC0 or CC-BY licenses
- **zapsplat.com** — Free with account, good SFX
- **itch.io audio packs** — Curated game audio bundles
- **Incompetech (Kevin MacLeod)** — Royalty-free Americana/folk music

Search terms:
- "acoustic guitar loop folk"
- "cash register ding"
- "wood chop"
- "liquid pour bubble"
- "grain harvest rustle"
- "cricket night ambience loop"

---

## `AudioManager.cs` EXTENSION PLAN

```csharp
// Current: 5 SFX slots + 2 music slots
// Target: Named methods for each action

public void PlayBuildingPlaced()  => PlaySFX(_buildingPlacedClip);
public void PlayCropHarvested()   => PlaySFX(_cropHarvestedClip);
public void PlayTreeChopped()     => PlaySFX(_treeChoppedClip);
public void PlaySellCash()        => PlaySFX(_sellCashClip);
public void PlayCannotAfford()    => PlaySFX(_cannotAffordClip);
public void PlayProduced()        => PlaySFX(_producedClip);
public void PlayDemolish()        => PlaySFX(_demolishClip);

// Ambient layer (looping, low volume)
public void StartAmbience(bool isNight) { ... }
```

---

## INTEGRATION CHECKLIST

Wire `AudioManager` calls into these scripts:

| Script | Call to Add |
|--------|------------|
| `BuildingPlacementManager.cs` | `AudioManager.Instance.PlayBuildingPlaced()` |
| `CropBehavior.cs` | `AudioManager.Instance.PlayCropHarvested()` |
| `TreeBehavior.cs` | `AudioManager.Instance.PlayTreeChopped()` |
| `SellManager.cs` | `AudioManager.Instance.PlaySellCash()` |
| `BuildingBehavior.cs` | `AudioManager.Instance.PlayProduced()` |
| `DayNightCycle.cs` | `AudioManager.Instance.CrossfadeTo(dayClip/nightClip)` |
| `BuildPanelUI.cs` | `AudioManager.Instance.PlayCannotAfford()` if grayed-out button tapped |

---

## ACCEPTANCE CRITERIA

- [ ] Every critical player action (place, harvest, sell) has a distinct SFX
- [ ] SFX clips match the rustic/folk game vibe (not placeholder/generic)
- [ ] Music crossfades between day and night
- [ ] Main menu theme is distinct from gameplay music
- [ ] Cannot-afford action gives clear negative audio feedback
- [ ] No SFX causes console errors (null clip gracefully handled)
- [ ] Audio volume is appropriate — no single SFX dominates

---

## HOW IT CONNECTS TO THE FUTURE

```
  Audio Polish (Phase 1)
       │
       ├── Phase 2: Ambient crowd noise in Saloon when selling
       ├── Phase 2: Building-specific ambient loops (still bubbling, rickhouse creaking)
       ├── Phase 3: Dynamic music that intensifies during events
       ├── Phase 3: NPC voice lines / grunts
       └── Phase 4: Seasonal audio themes (winter hush, summer festival)
```

---

## ESTIMATED TIMELINE

| Task | Time |
|------|------|
| Source / download SFX clips (12 clips) | 20 min |
| Source / download music tracks (3 tracks) | 10 min |
| Extend `AudioManager.cs` with named methods | 10 min |
| Wire calls into 7 scripts | 10 min |
| Test all audio in game loop | 10 min |
| **Total** | **~60 min** |

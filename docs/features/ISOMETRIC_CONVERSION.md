# 🎭 Feature Spec: Isometric Art Conversion
### Phase 5 — Visual Identity Upgrade | Priority: Post-MVP (2027+)
### Risk Level: 🔴 High | Estimated Scope: 2–4 months (art + code)

---

## CONCEPT

The current game uses a **top-down 2D perspective** — tiles are viewed from directly above,
buildings are flat sprites. This is functional and shippable, but lacks visual depth and
the "charm" factor that makes town-builders feel special on an app store page.

An **isometric (2.5D)** conversion would give Whiskeyville a visual identity that:
- Pops on a screenshot
- Reads immediately as a town-builder (genre cue)
- Allows for depth, shadows, and building character
- Differentiates the game in a crowded market

```
  CURRENT (Top-Down):           ISOMETRIC TARGET:

    [FIELD] [MASH] [STILL]           ╱‾‾╲  ╱‾‾╲
    [COOP] [RICK] [SALN]           ╱_RICK_╲╱_SALN_╲
                                   ╲_FIELD_╱╲_MASH_╱
                                     ╲__╱   ╲__╱
```

---

## WHAT CHANGES

### Art Assets (Very High Effort)
Every sprite in the game must be redrawn in isometric perspective:

| Asset | Count | Notes |
|-------|-------|-------|
| Buildings (6 base) | 6 | Each needs N/S/E/W facings or 1 canonical |
| Building upgrades | ~18 | 3 tiers per building |
| Crop growth stages | 4×4 = 16 | 4 crops × 4 stages |
| Trees (stages) | ~6 | Growth + chopped states |
| Terrain tiles | ~8 | Grass, dirt, tilled, path, water, etc. |
| NPCs/Tourists | ~10 | Walk cycles, directions |
| Decorations | ~20 | All decoration types |
| UI icons | ~30 | Buttons, HUD elements |
| **Total assets** | **~110+** | Likely 3–6 months of art work |

### Code Changes (Medium Effort)

#### `GridManager.cs` — Iso Grid Rewrite
Current grid uses direct `(x, y)` tile positions. Isometric requires:
```csharp
// World position from grid coords:
Vector3 IsoToWorld(int gridX, int gridY)
{
    float worldX = (gridX - gridY) * (tileWidth / 2f);
    float worldY = (gridX + gridY) * (tileHeight / 4f);
    return new Vector3(worldX, worldY, 0);
}
```

#### Sorting Order (Already Solved)
Current Y-sort system (`(gridHeight - y) * SORT_MULTIPLIER`) translates naturally to iso:
- Tiles at higher Y (further back in iso view) render behind tiles at lower Y (closer)
- This is the same principle — just the coordinate space changes

#### `CameraController.cs` — Iso Camera
- Fixed camera angle (no rotation needed for standard iso)
- Zoom via orthographic size (unchanged concept)
- Pan must account for iso diamond shape (not axis-aligned)

#### `BuildingPlacementManager.cs` — Iso Click Detection
Mouse/touch → screen position → iso grid coordinate (inverse of IsoToWorld):
```csharp
Vector2Int WorldToIsoGrid(Vector3 worldPos)
{
    int gridX = Mathf.RoundToInt((worldPos.x / (tileWidth / 2f) + worldPos.y / (tileHeight / 4f)) / 2);
    int gridY = Mathf.RoundToInt((worldPos.y / (tileHeight / 4f) - worldPos.x / (tileWidth / 2f)) / 2);
    return new Vector2Int(gridX, gridY);
}
```

---

## MIGRATION STRATEGY

**Recommended: Parallel Branch Approach**

```
  main branch        → continues shipping MVP + v1.0 in top-down style
  feature/iso        → isometric conversion in isolation
                       merges into main when art is 80%+ complete
```

This avoids blocking the main game roadmap with a multi-month art project.

**Do NOT start isometric conversion until:**
- [ ] MVP is 100% complete
- [ ] v1.0 content is shipped and stable
- [ ] An artist is committed to the project full-time

---

## TILE DIMENSIONS (Standard Iso)

```
  Standard 2:1 isometric tile:
  Width:  64px
  Height: 32px

  Building sprite height: 2–4× tile height depending on building
  e.g., Rickhouse = 128px tall (4 tiles)
```

---

## ART STYLE REFERENCE

Target aesthetic: **Stardew Valley meets Spirited Away** — warm, hand-painted, slightly
rustic. Not pixel art (too many competitors). Not 3D-rendered (too expensive). Target:
lush, slightly overdrawn 2D sprites in iso perspective.

- Color palette: Warm earthy tones (amber, brown, sage green, cream)
- Lighting: Soft top-down with implied shadow shapes under buildings
- Night mode: Same palette but desaturated + lantern glow

---

## ACCEPTANCE CRITERIA (Phase 5)

- [ ] All current building types have isometric sprites
- [ ] Grid correctly positions tiles in isometric layout
- [ ] Click/touch correctly converts to iso grid coordinates
- [ ] Y-sort renders buildings correctly (back = behind, front = in front)
- [ ] Camera pan works in iso coordinate space
- [ ] Full MVP loop playable in isometric view
- [ ] Performance: 60fps on iPhone 12+ with full grid

---

## COST / BENEFIT ASSESSMENT

| Metric | Top-Down (Current) | Isometric (Phase 5) |
|--------|-------------------|---------------------|
| Art cost | Low (simple sprites) | Very High (custom per-tile) |
| App store visual appeal | Medium | High |
| Code complexity | Low | Medium |
| Dev time | — | +2–4 months |
| Player "wow" factor | Medium | High |
| Differentiation | Low | High |

**Verdict:** Worth doing — but only after the game is financially self-sustaining or with
an art partner committed to the scope.

---

## HOW IT CONNECTS TO THE FUTURE

```
  Isometric Conversion
       │
       ├── NPCs become full walking characters (iso sprites)
       ├── Buildings can have interior views (saloon interior on click)
       ├── Steam/PC release viability increases significantly
       └── Opens door to 3D upgrade path (iso → 3D is smaller gap than top-down → 3D)
```

---

*"Top-down ships. Isometric sells. Build the game first, make it beautiful second."*
*— Black Label Interactive*

# 🧬 Memetic Research — Word-of-Mouth Spread in Whiskeyville
### Research Folder | Internal Design Reference
### Updated: March 2026

---

## CONCEPT

Memetic Research covers how ideas, flavors, and your distillery's reputation **spread
organically** through the town and beyond — without the player spending marketing dollars.
A meme (in the original Dawkins sense) is any idea that self-replicates; in Whiskeyville,
that means **happy customers who tell other people about your whiskey**.

```
  One satisfied tourist tells two friends
  Two friends tell four more
  Your fame compounds without ad spend
```

When a player's whiskey has a strong identity (unique label, distinct flavor, high quality),
it becomes *memorable* — and memorable things spread.

---

## GAME MECHANICS

### Meme Score

Each whiskey batch carries a hidden **Meme Score** (0–100) calculated from:

| Factor | Weight | Max Contribution |
|--------|--------|-----------------|
| Label uniqueness (custom vs. default) | 30% | 30 pts |
| Flavor distinctiveness (rare combos) | 25% | 25 pts |
| Quality tier (1–5 stars) | 20% | 20 pts |
| Bottle type (premium glass) | 15% | 15 pts |
| Temperature profile (Hot/Cold) | 10% | 10 pts |

```
  Meme Score = (labelBonus × 0.30) + (flavorBonus × 0.25)
             + (qualityBonus × 0.20) + (bottleBonus × 0.15)
             + (tempBonus × 0.10)
```

### Spread Events

When a tourist purchases whiskey with Meme Score ≥ 50, there is a chance they generate
a **Spread Event** on departure. Spread Events increase the passive tourist spawn rate
for a short window.

| Meme Score Range | Spread Chance | Spawn Rate Boost | Duration |
|-----------------|--------------|-----------------|---------|
| 50–64           | 10%          | +15%            | 30s     |
| 65–79           | 25%          | +30%            | 60s     |
| 80–94           | 50%          | +60%            | 120s    |
| 95–100          | 90%          | +100%           | 300s    |

### Viral Batch

A batch that scores 90+ Meme Score becomes a **Viral Batch**, shown with a flame icon
in the Saloon UI. During a Viral Batch window:
- Tourist spawn rate doubled
- Fame earned per sale ×3
- A random Journalist NPC may appear (Phase 3)

---

## RESEARCH UPGRADES (ResearchManager)

The **Memetic Research** tree can be unlocked at the Research Lab building (250 Fame).

| Research Node | Cost | Effect | Prerequisite |
|--------------|------|--------|-------------|
| Word-of-Mouth Basics | 5 Research Pts | Meme Score threshold lowered to 40 | None |
| Community Buzz | 10 Research Pts | Spread duration +50% | Word-of-Mouth Basics |
| Viral Loop | 20 Research Pts | Viral Batch triggers at Meme Score 80 | Community Buzz |
| Legend Status | 40 Research Pts | Viral Batch fame multiplier ×5 | Viral Loop |

---

## INTEGRATION POINTS

- **WhiskeyPropertyManager.cs** — Provides flavor, quality, temperature inputs for Meme Score
- **BottleCustomizationManager.cs** — Provides label uniqueness + bottle type inputs
- **TouristSpawner.cs** (Phase 3) — Reads Spread Events to adjust spawn rate
- **SellManager.cs** — Triggers Spread Event check on sale completion
- **FameMilestoneManager.cs** — Viral Batch events generate bonus fame

---

## PLAYER-FACING COMMUNICATION

Players should understand why word-of-mouth is happening without seeing the math:

- **Tooltip on Meme Score bar**: "People are talking about this batch!"
- **Spread Event notification**: "A happy customer is telling their friends! (+30% tourists for 60s)"
- **Viral Batch UI**: Flame icon on the Saloon + pulsing golden text

---

## TOWN IMPACT

As Meme Score compounds over time, the town transforms:

| Cumulative Viral Batches | Town Change |
|--------------------------|-------------|
| 3 Viral Batches          | New signpost appears near Saloon ("Famous Local Whiskey") |
| 10 Viral Batches         | Tourists arrive in pairs rather than solo |
| 25 Viral Batches         | Whiskeyville appears on the in-game regional map |
| 50 Viral Batches         | Festival event unlocked: "Whiskeyville Meme Fest" |

---

## REFERENCES

- Dawkins, R. (1976). The Selfish Gene — original meme theory
- Jonah Berger (2013). Contagious: Why Things Catch On — STEPPS framework
- Applied to games: word-of-mouth mechanics in Stardew Valley (seasonal spreads),
  Recettear (customer gossip), and Moonlighter (item reputation system)

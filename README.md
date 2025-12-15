# Cog 'em All!

A steampunk-themed 3D tower defense game built with Unity 6.

## Overview

Cog 'em All! is a tower defense game featuring steampunk aesthetics with a
faction-based progression system. Players defend their base against waves of
enemies by strategically placing towers and utilizing various skills. The game
includes a skill tree system, multiple tower types with upgrades, and abilities.

## Game Features

### Towers

The game features 4 distinct tower types, each with unique mechanics and upgradeable stats:

- **Gatling Tower** - Rapid fire projectile tower
- **Tesla Tower** - Beam based tower with chain lightning that can jump between enemies
- **Mortar Tower** - AoE explosive tower with shell projectiles
- **Flamethrower Tower** - Close range tower that applies burning damage over time

Towers can be:

- Placed anywhere
- Upgraded through multiple levels (up to 3, if unlocked in skill tree)
- Controlled directly by the player (Gatling and Tesla, if unlocked)

### Skills & Abilities

Players can deploy various tactical skills:

| Skill        | Type      | Description                                                          |
| ------------ | --------- | -------------------------------------------------------------------- |
| Wall         | Placement | Deploys a defensive barrier, with different modifiers                |
| Oil Spill    | Placement | Creates a slowing zone that can be ignited, with different modifiers |
| Mine         | Placement | Explosive trap with different modifier options                       |
| Airstrike    | Airship   | Calls in aerial bombardment at target location                       |
| Freeze Zone  | Airship   | Creates a freezing area via airship drop                             |
| Disable Zone | Airship   | Disables enemy buffs in target area                                  |
| Mark Enemy   | Raycast   | Marks an enemy for priority targeting by towers                      |
| Sudden Death | Instant   | High-risk/high-reward mode with boosted gear rewards                 |

Skills can be enhanced with modifiers unlocked through the skill tree and by
leveling a faction, which earns ability points

### Enemies

Three enemy types challenge players:

- **Bandit** - Standard enemy
- **Bomber** - Suicide enemy that deals explosive damage
- **Dreadnought** - Heavy armored enemy

Enemies can be affected by various status effects:

### Factions

Three playable factions with unique skill trees:

- **The Brass Army**
- **The Valvebound Seraphs**
- **Overpressure Collective**

Each faction has its own progression and skill tree.

### Economy & Resources

- **Gears** - Primary currency for building and upgrading towers
- Passive income generation over time
- Enemy kills reward gears
- Skill tree modifiers can affect economy rates

### Progression System

- **Experience (XP)** - Earned by completing operations
- **Faction Levels** - Up to level 15 per faction
- **Skill Tree** - Unlock tower upgrades, ability modifiers, and passive bonuses
- **Save System** - Multiple save slots with per-faction progress

## Technical Details

### Project Structure

```
Assets/Scripts/
├── Audio/           # Sound and music management
├── Enemies/         # Enemy types and behaviors
├── Factions/        # Faction data definitions
├── GameStatePersistence/  # Save system
├── Levels/          # Wave spawning, orchestration, modifiers
├── Nexus/           # Player base and health management
├── Projectiles/     # Bullets, beams, shells, flames
├── Skills/          # Deployable abilities
├── Towers/          # Tower types and mechanics
└── UI/              # HUD, menus, overlays
```

### Key Systems

- **Orchestrator** - Central game loop controller managing waves, resources, and win/lose conditions
- **Spline-based paths** - Enemy movement along Unity Splines
- **Level Editor** - Custom inspector for wave design and JSON import/export
- **Modifiers System** - Skill tree unlocks that modify tower/economy/enemy stats
- **Save System** - JSON-based persistent storage for player progress

### Level Design

Levels are defined in JSON format with:

- Operation metadata (name, difficulty, index)
- Player starting resources
- Wave definitions with spawn groups and patterns
- Spline path data for enemy movement

## Getting Started

1. Open the project in Unity 6 (6000.2.6f1 or compatible)
2. Open a scene from `Assets/Scenes/`
3. Level data is stored in `Assets/StreamingAssets/Levels/`
4. Configure operation settings via `DevOperationData` scriptable object for testing

## Inspiration

Tower defense inspiration https://www.youtube.com/watch?v=f6KTtb1r1lg

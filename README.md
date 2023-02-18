# BugFixes

This mod aims to add various bug fixes to the game. 

To report a bug from the game that you would want fixed, or a problem with this mod, please raise an issue on [GitHub](https://github.com/oliviersamson/Muck-BugFixes/issues "GitHub")

Note that all players in a lobby need to have this mod installed to play together.

## Fixes

- Fixed a bug where all players wouldn't spawn the same amount of items in chests. The items themselves would be the same, but the amount would be different (ex: 6 red apples vs 3 red apples for two different players).
- Fixed a bug where houses and tents would be generated in a way that would make it impossible to get inside (houses) or under (tents).
- Fixed a bug where camp structures can spawn very close or in another camp structure, or house.

## TODO: Future fixes

- Fix bug where diagonal movement boosts jumping (add a way to deactivate this fix for players who want to take advantage of this movement mechanic).
- Fix bug where one shotting Woodmen don't trigger their hostility towards you.
- Fix bug where resources can spawn on/inside structures.
- Fix bug where wheat renders late.
- Fix bug where shallow water slows down players.
- Fix bug where Chief's spinning attack continues after his death, still causing damage to players in range.
- Fix bug where non-camp structures can spawn very close or in another structure.
- Fix bug where the full amount of structures calculated won't be spawned in the world (which can be seen in the BepInEx console with a log message, e.g.: "spawned 1 / 3")

# BugFixes

This mod aims to add various bug fixes to the game. 

To report a bug from the game that you would want fixed, or a problem with this mod, please raise an issue on [GitHub](https://github.com/oliviersamson/Muck-BugFixes/issues "GitHub")

Note that all players in a lobby need to have this mod installed to play together.

## Fixes

- Fixed a bug where all players wouldn't spawn the same amount of items in chests. The items themselves would be the same, but the amount would be different (ex: 6 red apples vs 3 red apples for two different players).
- Fixed a bug where houses and tents would be generated in a way that would make it impossible to get inside (houses) or under (tents).
- Fixed a bug where camp structures could spawn very close or in another camp structure, or house.
- Fixed a bug where the full amount of structures calculated for a camp wouldn't be spawned (which could be seen in the BepInEx console with a log message, e.g.: "spawned 1 / 3")
- Fixed a bug where resources can spawn on/inside structures.
- Fixed a bug where non-camp structures can spawn very close or in another structure.
- Fixed a bug where Chief's spinning attack continues after his death, still causing damage to players in range.
- Fixed a bug where mobs can spawn on top of structures
- Fixed a bug where one shotting Woodmen wouldn't trigger their hostility towards you.
- Fixed a bug where items would not visually update in hotbar after boat repair
- Fixed a bug where the usual UI background was not applied to the Game Settings UI in the Lobby menu UI

## TODO: Future fixes

- Fix bug where wheat renders late.
- Fix bug where some trees pop out of render before other trees further from them
- Fix bug where structures can spawn with a too steep incline, making it impossible for the player to reach (especially important for tents and houses)
- Fix bug where sound volume falloff with distance is inconsistent between different sounds (some can be heard across the map)
- Fix bug where it is possible to see further into fog from the sides of the screen
- Fix bug where Chief's spinning attack hitbox allows to be in contact with him while taking no damage
- Fix bug where a player gets HP over his limit when entering combat with Bob
- Fix bug where spectating a player when the gameover UI pops up will keep the camera movements enabled while browsing the menu.
- Fix bug where taking the boat map from a chest with Shift+Click doesn't trigger the "boat map found event" (might be the same with the gems map)
- Fix bug where changing control settings while in game doesn't focus the mouse controls back to the game
- Fix bug where the prompt for changing a key bind isn't shown when changed while in a game
- Fix bug where the back button on the settings menu while in pause gets you back into the game instantly with the mouse unfocused on the game
- Fix bug where workbench texture is not the same between the craftable workbench and the workben associated with the wood cutter
- Fix bug where mob attack is delayed right after spawning
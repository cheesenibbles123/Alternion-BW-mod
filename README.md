![custom Image](https://github.com/cheesenibbles123/yeold/blob/master/bwCsharp.jpg?raw=true)
# Alternion
This is a split off from my first mod which was initially created for custom badges and outfits. That one allowed for a user to use a local file to assign badges to players, and replace outfits, however this was't replicated across all mod users. Hence why this was made, the original mod can still be found here: [https://github.com/cheesenibbles123/Original-CharacterCustomization-thingy-bw-mod](https://github.com/cheesenibbles123/Original-CharacterCustomization-thingy-bw-mod).
The original goal was simply to modify and allow for custom badges, and well... it kind of spiralled from there onto what it is now. If you come across any issues make sure to mention/report them, either here or to me on discord **Archie the inventor#4744**

# Features:
 - Custom Badges based on steamID's managed by a website
 - Custom Sail skins based on captain's steamID
 - Custom Cannon skins based on captain's steamID
 - Custom Weapon skins based on player's steamID
 
# Installation
 - Copy **0Harmony.dll**, **Assembly-CSharp.dll** and **BWModLoader.dll** into your **Blackwake_Data/Managed/** folder.
   (These files are added on the latest release version, starting at **v 4.0**, for those trying to run an older version see ***Required files*** below)
 - Run the game, it should generate a mods folder in **Blackwake_Data/Managed/Mods**
 - Copy **Alternion.dll** into the mods folder
   (All assets required will be automatically downloaded)
 - Play!

***Required files:***
 - **0Harmonydll** - version 1
 - **Assembly-CSharp.dll** - Modified assembly-csharp file
 - **BWModLoader.dll** - modloader created by da_google
 
# Issues:
 - The main known issue currently is possible lag every few seconds, I believe this is tied to the leaderboard updating, as this is more noticeable on servers with more players. Best fix for this might be to cache the badges on main Start(), although depending on the future badge count this may/may not be unsustainable (will require a deeper look).
 (with the latest caching update I believe I have for the most part fixed this, however I need more validation before I can confirm it) 
 
# Additional notes:
 - All required files will be downloaded automatically by the mod when it runs
 - I suck at this
 - Enjoy my crappy code

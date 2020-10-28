# Alternion
![custom Image](https://github.com/cheesenibbles123/Alternion-BW-mod/blob/master/Images/bwCsharp.jpg)

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
 
# Additional notes:
 - All required files will be downloaded automatically by the mod when it runs
 - On launching into the main menu, press **Insert** and go to the **logs** tab, scroll down and when it says either **Cannons Downloaded.** ***OR*** **All Textures Downloaded!** you can go join a server, this is because the mod downloads all required assets on starup as mentioned above, however as more and more stuff gets added, it will take longer and longer to load up. For now however it should be ok as there is still a very small pool of items.
 
 - I suck at this
 - Enjoy my crappy code
 - To those interested, there is a beta branch, however this one likely wont work, as its me messing about with unstable code, and using it to save it whilst I work on other stuff

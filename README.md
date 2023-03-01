# EqTool

<h2>THIS PROGRAM WORKS SOLELY BY READING YOUR LOG FILE. This is legal according to the <a href="https://www.project1999.com/forums/showthread.php?t=325349">rules.</a></h2>
<img width="851" alt="image" src="https://user-images.githubusercontent.com/3393733/214930917-994fa61b-f1c8-414b-9761-1c93ca247b63.png">

Instructions:
<ul>
<li>
<h2>Download the latest <a href="https://github.com/smasherprog/EqTool/releases/download/1.0.1.863/EQTool_1.0.1.863.zip">EQTool.zip</a>, Unzip it and run EQTool.exe</h2>
</li>
<li>The program runs in the system tray. Look there to reopen spells window or settings! Program will check for updates on startup and self update if required, but if you want to check for a new Update, use the menu in the system tray!</li>
</ul>
<h5>Why the pig?</h5>
<p>https://discord.gg/nSrz8hAwxM</p>
Features:
<br/>
<ul>
<li>Detect EQ directory location instead of user required to enter it.</li> 
<li>Detect Spells cast on others (this is a best guess as I am reading the log file so chloroplast and Regrowth of the growth have the same message)</li>
<li>Filter spells show by class</li> 
<li>Remove Spells from List if "Worn off message occurs"</li> 
<li>Mob Info Window gives details about mobs tht you con in game.</li>
<li>Automatically remove dead npc/player from the spell list.</li>
<li>DPS window. Saves fight data to a CSV so you can review later!</li>
<li>Auto detect level and class!</li>
<li>DPS is trailing 12 second average.</li>
<li>Maps</li>
<li>Timers (Only Minutes are supported)</li>
</ul>
<h5>Timers (Only Minutes are supported) -- All below commands work in regular say!</h5>
<ul>
<li>Timer Start Crypt Camp 35</li>
<li>Start Timer Crypt Camp 35</li>
<li>Timer Cancel Crypt Camp</li>
<li>Cancel Timer Crypt Camp</li>
</ul>
<img width="806" alt="image" src="https://user-images.githubusercontent.com/3393733/221380745-7b584b8d-cc75-4132-aab3-4d632d34bfbe.png">

<img width="808" alt="image" src="https://user-images.githubusercontent.com/3393733/215292103-89c83b08-c495-4b65-806e-beec92ade86e.png">

<h4>System Tray Icon</h4>
<img width="152" alt="image" src="https://user-images.githubusercontent.com/3393733/212717141-6e26b9af-660a-493d-9f73-2c3464b7c224.png">

<h4>TO DO List</h4>
<ul>
<li>Enable EQ logging automatically if EQ is not running.</li>
<li>Add option to auto prune eq log file. EQ logfiles can cause issues with EQ itself if they get too large!</li>
<li>Remove spells from self using the worn off messages.</li>
<li>Self update when NOT in use</li>
<li>Raid Group suggestions for guild: AOE; CH Chain; AOE+Ch Chain; Other</li>
<li>Better track players levels and classes</li>
<li>Respawn Time in Mob Info window</li>
<li>Ability Hide/show mob info data</li>
<li>Automatically add timer when named npc dies. Use Wiki for notable npc names</li>
<li>Add donals BP to timers list</li> 
<li>Rename Application to Pig Parse</li>
<li>Add ability to delete individual spells and entire section</li>
<li>Enrage alert/advanced alert.</li>
<li>charm break alert</li>
<li>charm spell effect removal</li>
<li>Always on top toggle for each window</li>
<li>Add Transparency setting for Map window</li>
<li>Map window add toggle to follow location</li>
<li>Map window make it possible to go entierly transparent!</li>  
<liMake settings window and Mob window remember last position/size/state</li>
<li>Add loot TAB to Mob Window. This tab will show item name, looted from, player name who looted, and unix geek price data, running total looted.</li>
</ul>

<h3>Faqs</h3>
<h4>Why does my settings window say Configuration missing?</h4> 
<img alt="image" src="https://user-images.githubusercontent.com/3393733/222051822-fc4b750d-2efa-4eb9-bc00-589d3cc5b781.png">
<ul>
<li>EQTool was unable to automatically detect your P99 install folder. You must specific it yourself!</li>
<li>EQTool detected that eq logging is turned off. You must click enable logging. This will turn on EQ's logging which is where EQTool gets informatioon from.</li>
</ul>

<h4>Do i have to set my class and level?</h4> 
<ul>
<li>If you cast spells eqtool will automatically detect your class and level once you start casting spells.</li>
<li>You should still enter your class and level. It helps ensure calculations on spell durations are accurate.</li>
</ul>

<h4>I only care about spells cast on me, not everyone else!</h4> 
<ul>
<li>Great, goto settings and make sure the box is checked; 'Only show spells that effect you'.</li> 
</ul>

<h4>I only want to see cleric buffs; there are too many buffs to see!</h4> 
<ul>
<li>Great, goto settings and make sure that cleric is the only class selected in the "Other Soells" section.</li> 
</ul>

<h4>I have everything working, but i dont see my location on the map, why?</h4> 
<ul>
<li>You need to type /loc into chat so that your location is feed to the log file.</li> 
<li>Normally, players create a hotkey that is bound to their movement keys. Then add a /loc so that each time you move, the macro for /loc is called.</li> 
<li>I set up my movement keys 'a' and 'd' to activate my hotbar 1 macro which has a /loc in it.</li> 
</ul>

<h4>How do i get the latest update?</h4> 
<ul>
<li>Goto the system tray, click the pig icon and goto check for updates.</li> 
<li>Updates are checked for every timee the application starts as well.</li> 
<li>If an update is available it will download and start the new version. The old version will be deleted.</li> 
</ul>

# OpenSkyPlus

![OpenSkyPlus logo](https://github.com/OpenSkyPlus/OpenSkyPlus/blob/main/logo.png)

## Table of Contents
1. [What is OpenSkyPlus](#what-is-openskyplus)
2. [How to Use](#how-to-use)
3. [Contributing](#contributing)
4. [App FAQ](#faq)
<br>
<br>
<a id="what-is-openskyplus"></a>

### What is OpenSkyPlus
OpenSkyPlus (OSP) is a framework for integrating C#-based golf simulator launch monitors to arbritrary simulator software.

Currently this framework only supports one launch monitor and one simulator, but can easily be expanded to others.


OSP is written in C#. It uses [BepInEx](https://github.com/BepInEx/BepInEx) 5.4.22 framework backed by [Unity-Doorstop](https://github.com/NeighTools/UnityDoorstop) for code injection.

Ball and club data is taken from Unity and passed to arbritrary plugins for use in other applications.

Currently there is one plugin written for OSP, [GSPro4OSP](https://github.com/OpenSkyPlus/OpenSkyPlus/GSPro4OSP). It is recommended to use this project as an example for adding more plugins.
<br>
<br>
<br>
<a id="how-to-use"></a>

### How to Use

**I Don't Know What I'm Doing**
<br>
There is an EXE [installer](https://github.com/OpenSkyPlus/OpenSkyPlus/releases) for non-tech savvy people who just want to play.

When choosing where to install OpenSkyPlus, it will default to 
`C:\Program Files\[Launch Monitor]`
==Make sure you change [Launch Monitor] to the directory where you have your launch monitor software installed!==

When prompted about replacing old plugins, it is recommended that you choose 'Y'.

The packaged version comes with GSPro4OSP, so if this is the plugin you want, all you have to do is launch GSPro, and then your launch monitor software.

Wait for the green light, and you should be good to go.
<br>
<br>
**I Know What I'm Doing**
<br>
If you want to get fancy and write your own plugins or contribute, you can submit a PR. It is recommended to use the GSPro4OSP project linked above as a template for new plugins.

1. Clone the repo
2. Open the OpenSkyPlus.sln file in Visual Studio
3. Verify your nuget dependencies are correctly downloaded and all files in the lib directory are linked to your project
4. Compile. You're good to go.

If you are not familiar with BepInEx, it is suggested to [read up on it](https://github.com/BepInEx/BepInEx/wiki) as well as [Harmony](https://github.com/pardeike/Harmony/wiki). It will make things much clearler more quickly. 
BepInEx has its own extension of Harmony, but the OG wiki is the best source for learning the concepts of it.
<br>
<br>
<br>
<a id="contributing"></a>

### Contributing

Contributions to this project are welcome from the community. Custom plugins will not be hosted here, but if there is a popular integration, we can link to it.

Some of the high priority items to do are as below:

- [ ] Validate units of measurement from the launch monitor (especially putting)
- [ ] Add additional club measurements
- [ ] Stability and relisency of framework
- [ ] Increase performance
<br>
<br>
<br>
<a id="faq"></a>

### FAQ

**Q. My monitor is stuck with a red light**
<br>
*A. Open your launch monitor software and click the black OSP box in the bottom-right corner.*

*A debugging menu will come up.*

*Press "Force Normal Mode" or "Force Putting Mode" to change modes to the type you need and automatically re-arm the monitor.*
<br>
<br>
**Q. Nothing is getting sent to GSPro**
<br>
*A. GSPro likely got disconnected from OSP.*

*Close the GSPro API and from the GSPro Settings menu, choose to reset the API.*

*Once it loads, OSP will automatically reconnect and data should flow again. It can take up to 30 seconds for it to reconnect.*
<br>
<br>
**Q. I'm having some other problem**
<br>
*A. For anyone having issues, it's recommended to visit the SGT Simulator Golf Tour Discord server. There is a channel just for your launch monitor.
There, you can submit issues by including your debugging log and someone will help you.*
<br>
<br>
**Q. How do I get a debugging log?**
<br>
*A. Close your launch monitor software.*
*Locate the OSP config file (C:\Program Files\\[Launch Monitor]\BepInEx\plugins\OpenSkyPlus\settings.cfg)*
*Change ==LogLevel = Info== to ==LogLevel = Debug==*
*Restart your launch monitor software, recreate your issue, then open the log file (C:\Program Files\[Launch Monitor]\BepInEx\plugins\OpenSkyPlus\Log.txt)*
*Send it to the Discord channel.*
<br>
<br>
**Q. What do the other settings do?**
<br>
*A.*

*Most items are self explainatory except:*

*- LaunchMonitor is the name of the launch monitor brand you're using and should be in PascalCase (JustLikeThis). 
This is to avoid OSP using any trademarked names in the codebase.*

*- ShotConfidence is how aggressive OSP will be in discarding "junk" shots or misreads from the monitor.*
<br>
<br>
***Forgiving** will discard anything your launch monitor likely would.*
<br>
***Strict** will report nearly any ball movement whatsoever*
<br>
***Normal** is inbetwen and recommended.*

*Plugins should have their own configurations and are typically located in the root of the plugin folder.*






# osu-rx [![CodeFactor](https://www.codefactor.io/repository/github/mrflashstudio/osu-rx/badge?style=for-the-badge)](https://www.codefactor.io/repository/github/mrflashstudio/osu-rx) [![ko-fi](https://www.ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/mrflashstudio) [![discord server](https://discordapp.com/api/guilds/725077972075151430/widget.png?style=shield)](https://discord.gg/q3vS9yp)
osu!standard relax hack

## Status
osu!rx and the [framework](OsuManager) it's built on are still heavily under development.  

[Click here](#what-can-i-do-to-help-osurx) to see how you can help the development of osu!rx

## Features
- **Automatic beatmap detection:** you don't need to alt-tab from osu! to select beatmap manually, osu!rx will do all the dirty work for you.

- **Playstyles:** osu!rx has 3 most(?) popular built-in playstyles that you can select!
  - Singletap
  - Alternate
  - TapX
  
- **Hit Timing randomization:** osu!rx automatically randomizes click timings depending on whether you're currently alternating or not.

- **HitScan:** it will scan for current cursor position and determine whether relax should hit right now, earlier or later.

- **Customizability:** pretty much everything that is already fully implemented can be configured to better suit your needs.

- **[osu!rewrite](https://github.com/xxCherry/osu-rewrite) support:** osu!rx is the first [osu!rewrite](https://github.com/xxCherry/osu-rewrite)-compatible hack ever!

## Running osu!rx
**Download latest build from one of the sources below:**  
| [MPGH (every build is approved by forum admin)](https://www.mpgh.net/forum/showthread.php?t=1488076) | [Latest GitHub test build](https://github.com/mrflashstudio/osu-rx/releases/latest) |
|-----------------------------------------------|-----------|  

*Paranoids can compile source code themselves ;)*

- Extract downloaded archive into any folder.
- Run osu!rx.exe
- Change any setting you want.
- Select "Start relax" option in the main menu.
- Go back to osu! and select beatmap you want to play.
- Start playing!

### Requirements
- .net framework 4.7.2 is required to run osu!rx. You can download it [here](https://dotnet.microsoft.com/download/thank-you/net472).  

### Important notes
- If you're using osu!rewrite and changed its executable name, then change it in the *"osu!manager.ini"* file too or else osu!rx will not work with osu!rewrite.

- If you plan on changing executable's name, then change the name of *"osu!rx.exe.config"* too or else it will crash.  

- If you see something like *"Unhandled exception: System.IO.FileNotFoundException: ..."* follow these steps:
  - Right click on downloaded archive.
  - Click "Properties".
  - Click "Unblock".
  - Click "Apply" and repeat all steps described in **Running osu!rx** section.
   ![s](https://i.ibb.co/jZY8fk0/image.png)

## Detection state
**Relax:** undetected  
**Timewarp:** partially **detected**. Bancho (and probably any other server) can't detect it and auto-restrict you. But third party anticheats, such as firedigger's replay analyzer and circleguard will detect timewarp presence.

## What can i do to help osu!rx?
If you like what i'm doing and are willing to support me financially - consider becoming a sponsor!  
Click on **"❤︎Sponsor"** button at the top of this page to find out how i'm accepting donations.  
*Sponsors currently have no benefits, but i'll definitely consider adding some in the near future.*  

If you can't or don't want to support me financially - that's totally fine!  
You can still help me by providing any feedback, reporting bugs, creating pull requests and requesting features!  
Any help is highly appreciated!  
  
### Areas that need your attention
Hit Timing randomization and HitScan need improvements and your feedback. Take a look at the code to get started!
- [Hit Timings randomization](osu!rx/Core/Relax/Accuracy/AccuracyManager.cs#L72)
- [HitScan](osu!rx/Core/Relax/Accuracy/AccuracyManager.cs#L116)

## Demonstation video
***osu!rx does not affect performance. In this case lags were caused by obs and cute girls in the background.***
[![Video](https://i.ibb.co/grQSzMP/screenshot065.png)](https://www.youtube.com/watch?v=1FUxnGqjASQ)

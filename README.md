# Asteroids

A clone of the classic Asteroids arcade game with a modern twist.

Having recently developed a version of the original Taito Space Invaders in raw Z80 machine code for 
the ZX Spectrum [here](https://github.com/skagra/space-invaders), I thought it would be interesting 
to recreate a classic arcade game using modern developments tools - this Asteroids implementation, 
written using the [Godot](https://godotengine.org/) engine, is the result.

<p align="center">
<img src="docs/Screens/Splash.png" width="80%">
</p>

While Asteroids remains the core of the game this version is highly configurable and has some new features, for example:

* You may select either a modern UI theme, or a classic theme to match the original game.
* The gravitational pull of each asteroid on the player ship is modelled.
* There are many small tweaks such as rotating asteroids, UI fade-ins and camera shake.
* Many aspects of the behaviour of the player ship, asteroids and saucers may be customized via difficulty-based presets e.g. *Easy*, *Normal* or *Hard*.  In addition  individual game play settings may be tweaked e.g. the rate of player ship acceleration, the frequency at which saucers spawn and the magnitude of gravitational pull.

Find out [How to Play](docs/howtoplay.md), see the [Credits](docs/credits.md) where I've pulled in 3rd party graphics, fonts and sounds, and consult the [References](docs/references.md) I found useful during development.

# Status

The game is nearing being feature complete with a first beta release being imminent.

# License

This game is open source and licensed under [GPLv3](LICENSE).  Source code is available on [GitHub](https://github.com/skagra/gasteroids).

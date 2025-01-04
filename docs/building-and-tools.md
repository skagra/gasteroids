# Building and Tools

# Tooling

The following tooling was used to develop Asteroids:

* [Godot](https://godotengine.org/) - The Godot game engine, version 4.3
* [VSCode](https://code.visualstudio.com/) - Code editor/IDE. 
* [C#/.Net](https://dotnet.microsoft.com/en-us/languages/csharp) - The development language and framework.
* [rcedit](https://github.com/electron/rcedit) - Used by Godot to edit Windows exe resources as part of the export process.
* [make](https://gnuwin32.sourceforge.net/packages/make.htm) - GNU make compiled for Windows. 
* [Inno](https://jrsoftware.org/isinfo.php) - Installer tool for Windows.
* [Gimp](https://www.gimp.org/) - The GNU image manipulation program.
* [Git](https://git-scm.com/downloads/win) - The Git source code management system.
* [7zip](https://www.7-zip.org/) - File archiving tool.

# Development Set Up

In order to build and further develop the game you'll need:

* Godot (4.3)
* A code editor - VSCode is recommended.
* A C#/.Net development environment.

The other tools listed above are either used as part of the release process or for ancillary functions such as image editing.

With the required programmes installed:

1. Clone the Asteroids repository.

    ```git clone https://github.com/skagra/gasteroids.git```

2. Import the Gadot project.

    Open Gadot and select the `Import` function.  
    
    Browse to the `Game` directory under your cloned copy of the source and click `Select Current Folder`.   Open your newly imported project and so long as your environment is configured correctly you should be good to go.

# Release Set Up

In addition to the tools needed to develop Asteroids described above, to create a release, you'll need to have `rcedit`, `make`, `7Zip` and `Inno` installed and in your path.

1. In a terminal `cd` to the `Installer` directory under your cloned copy of the source.
2. All builds are created via `make` and will be written to the `Bin` directory.  The `Makefile` has the following targets:

    `all` - Build everything.

    `clean` - Delete all builds.

    `win-install` - Create a Windows installer in `Bin\Asteroids-win-setup-{version}.exe`.

    `win-distro` - Create a Zip of a Windows release in `Bin\Asteroids-win-{version}.zip`.

    `linux-distro` - Create a Zip of a Linux release in `Bin\Asteroids-linux-{version}.zip`



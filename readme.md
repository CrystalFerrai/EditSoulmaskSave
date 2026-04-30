Command line program for querying and modifying Soulmask save files.

**Warning**: This is an early prototype with limited testing and limited features available. Make sure to always backup your save files before running any program which may modify them.

## Releases

Releases can be found [here](https://github.com/CrystalFerrai/EditSoulmaskSave/releases). There is no installer, just unzip the contents to a location on your hard drive.

You will need to have the .NET Runtime 8.0 x64 installed. You can find the latest .NET 8 downloads [here](https://dotnet.microsoft.com/en-us/download/dotnet/8.0). Look for ".NET Runtime" or ".NET Desktop Runtime" (which includes .NET Runtime). Download and install the x64 version for your OS.

## Usage

This program can be used on `world.db` or `account.db` save files. See actions list for available features.

Run the program with no parameters to print the usage.
```
Usage: EditSoulmaskSave [save file path] [action [options]]

  [save file path]  Path to a save file (world.db, account.db, etc.)

Actions

  --list-players             Prints details about player accounts.

  --dump-players [dir]       Dumps all player state actors as json to the
                             specified directory.

  --dump-player-blobs [dir]  Decompresses and dumps all player state actor
                             blobs as raw binary to the specified directory.

  --fix-double-compress      Attempts to fix player accounts that have double
                             compressed actor data due to connecting to a
                             cluster with mismatched server versions.
```

## Building

Clone the repository, including submodules.
```
git clone --recursive https://github.com/CrystalFerrai/EditSoulmaskSave.git
```

You can then open and build EditSoulmaskSave.sln.

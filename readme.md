Command line program for querying and modifying Soulmask save files.

**Warning**: This is an early prototype with limited testing and limited features available. Make sure to always backup your save files before running any program which may modify them.

## Releases

Releases can be found [here](https://github.com/CrystalFerrai/EditSoulmaskSave/releases). There is no installer, just unzip the contents to a location on your hard drive.

You will need to have the .NET Runtime 8.0 x64 installed. You can find the latest .NET 8 downloads [here](https://dotnet.microsoft.com/en-us/download/dotnet/8.0). Look for ".NET Runtime" or ".NET Desktop Runtime" (which includes .NET Runtime). Download and install the x64 version for your OS.

## Usage

This program can be used on `world.db` or `account.db` save files. See actions list for available features.

Run the program with no parameters to print the usage.
```
Performs actions on or related to Soulmask save files.
Usage: EditSoulmaskSave [action [options]] [[additional actions]]

Notes

  1. One or more actions must be specified as parameters. Actions will be run
     in the order they are specified.

  2. Actions requiring a [save] paramter should be given a path to a Soulmask
     save file such as world.db, account.db, etc.

  3. Actions requiring [in] or [out] paramters should be given paths to files
     or directories as indicated by the action.

Actions

  --list-players [save]          Prints details about player accounts.

  --export-players [save] [out]  Exports all player state actors as json to the
                                 specified directory.

  --export-all [save] [out]      Exports all actors as json to the specified
                                 directory.

  --fix-double-compress [save]   Attempts to fix player accounts that have
                                 double compressed actor data due to connecting
                                 to a cluster with mismatched server versions.

Debug/Test Actions

  --dump-player-blobs [save] [out]  Decompresses and dumps all player state
                                    actor blobs as raw binary to the specified
                                    directory.

  --dump-all-blobs [save] [out]     Decompresses and dumps all actor blobs as
                                    raw binary to the specified directory.

  --blobs-to-json [in] [out]        Convert a directory of binary actor blobs
                                    to json.

  --json-to-blobs [in] [out]        Convert a directory of json actor blobs to
                                    binary.
```

## Building

Clone the repository, including submodules.
```
git clone --recursive https://github.com/CrystalFerrai/EditSoulmaskSave.git
```

You can then open and build EditSoulmaskSave.sln.

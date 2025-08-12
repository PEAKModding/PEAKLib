# PEAKLib

The community API for PEAK.

Features:

- [Core Module](/src/PEAKLib.Core/README.md)
  - Network Prefab API
  - Content Registry API
    - Easily find out which content belongs to who!
  - Bundle Loader API
  - [Host Mod List Listener API](./src/PEAKLib.Core/Networking.cs)
- [Items Module](/src/PEAKLib.Items/README.md)
  - Items API
  - Item Acceptor API (consumable items can be fed objects)
- [UI Module](/src/PEAKLib.UI/README.md)
  - UI API (see [ModConfig](/src/PEAKLib.ModConfig/README.md) for reference usage)
- [Stats Module](/src/PeakLib.Stats/README.md)
  - Status Effects API

You can use [PEAKLib.Tests](./tests/PEAKLib.Tests/README.md) as reference for how certain APIs can be used.

## Contributing

Feel free to chat in [`#peak-lib`](<https://discord.com/channels/1363179626435707082/1387320203746082866>) in the [PEAK Modding Discord Server](<https://discord.gg/SAw86z24rB>) about what you would like to contribute to this project. This is a community project and we're all together in this!

1. Fork this repository
2. Create a copy of the `Config.Build.user.props.template` file and name it `Config.Build.user.props`
   - This will automate copying your plugin assembly to `BepInEx/plugins/`
   - Configure the paths to point to your game path and your `BepInEx/plugins/`
   - Game assembly references should work if the path to the game is valid
3. Create a new feature branch
4. Do your changes
5. If you're adding a new module, make sure to add tests for it in the test project! *(currently the test project doesn't exist)*
6. Open a PR

### Contributing Guidelines

1. Never break the public API
   1. Public members must never be renamed (add the `[Obsolete]` attribute and create a new e.g. method with the new name instead)
   2. A new optional parameter can not be added to an existing method (create a new overload instead)
2. Your changes should not bring any warnings to the project. Warnings should be properly resolved, unless if it's a very special case where the warning can be ignored.

Ultimately these will be resolved in code review, so no need to worry about this too much.

### Thunderstore Packaging

This template comes with Thunderstore packaging built-in, using [TCLI](<https://github.com/thunderstore-io/thunderstore-cli>).

You can build Thunderstore packages by running:

```sh
dotnet build -c Release -target:PackTS -v d
```

> [!NOTE]  
> You can learn about different build options with `dotnet build --help`.  
> `-c` is short for `--configuration` and `-v d` is `--verbosity detailed`.

The built package will be found at `artifacts/thunderstore/`.

# PEAKLib

The community API for PEAK.

Features:

- [Core Module](/src/PEAKLib.Core/README.md)
  - Network Prefab API
  - Content Registry API
    - Easily find out which content belongs to who!
- [Items Module](/src/PEAKLib.Items/README.md)
  - Items API

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

# PEAKLib

Describe your project here!

## Template Instructions

You can remove this section after you've set up your project.

Next steps:

- Create a copy of the `Config.Build.user.props.template` file and name it `Config.Build.user.props`
  - This will automate copying your plugin assembly to `BepInEx/plugins/`
  - Configure the paths to point to your game path and your `BepInEx/plugins/`
  - Game assembly references should work if the path to the game is valid
- Search `TODO` in the whole project to see what you should configure or modify

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

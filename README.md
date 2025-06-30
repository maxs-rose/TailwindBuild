# TailwindBuild

TailwindBuild is designed to integrate with MSBuild to allow for the usage of Tailwind within projects without needing to use npm.

> [!NOTE]
> Currently we only work for Tailwind >=4.0.0 and < 5.x.x

## Getting Started

`dontet add package TailwindBuild`

## Properties

| MSBuild Property Name   | Default Value                                        | Description                                                                      |
|-------------------------|------------------------------------------------------|----------------------------------------------------------------------------------|
| TailwindVersion         | `latest`                                             | The version tag of the tailwind release to use.                                  |
| TailwindInstallPath     | `$(MSBuildThisFileDirectory)..\cli\`                 | The directory where the tailwindcss cli should be located.                       |
| TailwindWorkingDir      | `$(MSBuildProjectDirectory)\`                        | The directory which will be used as the working dir for tailwind.                |
| TailwindInputFile       | `tailwind.css`                                       | The name of the input css file.                                                  |
| TailwindOutputFile      | `$(MSBuildProjectDirectory)\wwwroot\css\output.css`  | The path where the output css file will be located.                              |
| TailwindMinify          | `false` for Debug builds, `true` for Release builds  | Whether the generated css should be minified or not.                             |
| TailwindWatch           | `false` (Will only work during a dotnet watch build) | Watch for changes.                                                               |
| TailwindGithubAuthToken | ``                                                   | (Optional) Used to authenticate against the GitHub api to prevent rate limiting. |

## Tips

- When using Rider the Jetbrains Language server will not work if a `package.json` containing `tailwlindcss` as a dependency cannot be found. It is recommended to create a simple `package.json` as a
  workaround 
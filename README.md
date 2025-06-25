# TailwindBuild

TailwindBuild is designed to integrate with MSBuild to allow for the usage of Tailwind within projects without needing to use npm.

## Getting Started

`dontet add package TailwindBuild`

Update the properties to use your tailwind config:

```xml
<PropertyGroup Label="Tailwind Properties">
  <TailwindInputFile>$(MSBuildProjectDirectory)\src\tailwind.css</TailwindInputFile>
  <TailwindOutputFile>$(MSBuildProjectDirectory)\wwwroot\css\output.css</TailwindOutputFile>
</PropertyGroup>
```

## Properties

| MSBuild Property Name | Default Value                                        | Description                                                       |
|-----------------------|------------------------------------------------------|-------------------------------------------------------------------|
| TailwindVersion       | `latest`                                             | The version tag of the tailwind release to use.                   |
| TailwindInstallPath   | `$(MSBuildThisFileDirectory)..\cli\`                 | The directory where the tailwindcss cli should be located.        |
| TailwindWorkingDir    | `$(MSBuildProjectDirectory)\`                        | The directory which will be used as the working dir for tailwind. |
| TailwindInputFile     | `tailwind.css`                                       | The name of the input css file.                                   |
| TailwindOutputFile    | `$(MSBuildProjectDirectory)\wwwroot\css\output.css`  | The path where the output css file will be located.               |
| TailwindMinify        | `false` for Debug builds, `true` for Release builds  | Whether the generated css should be minified or not.              |
| TailwindWatch         | `false` (Will only work during a dotnet watch build) | Watch for changes.                                                |
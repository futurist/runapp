# RunApp windows
Run windows application using config file, instead of shortcuts.

## Why

1. Create a shortcut in windows is good, but when you move your application folder, the shortcut becomes broken

2. When run some applications like batch file, you don't want a black "cmd.exe" window to appear

## Requirement

[.NET runtime](https://www.microsoft.com/net/download/dotnet-framework-runtime), any version should work. If you found a problem, please post a issue.

## Usage

The executable `"runapp.exe"` read all configs from file `"config.arg"`, the file content like below:

**config.arg**
```
chrome.exe
--disable-infobars
--user-data-dir=c:\chrome
--disable-plugins
--disable-plugins-discovery
--disable-translate
--start-maximized
--disable-dev-tools
--profile-directory=Default
https://www.google.com
```

The config is simple, the first line is the program to launch, rest lines are arguments passed to it.

Put the `"runapp.exe"` with same folder as `"config.arg"`, then run it.

## Advanced Usage

1. Any empty lines will be ignored.

2. `//` started is a comment line

**config.arg**
```
// this is comment line
chrome.exe
```

3. `:` started is a meta command

**config.arg**
```
// below will run batch file in hidden window
:style: hidden
// below set WorkingDir for the application
:dir: c:/windows

abc.bat
arg1
arg2
```

Thus can run a hidden dos batch silently.


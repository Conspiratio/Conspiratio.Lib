_[🇩🇪 Deutsch](README.md) | 🇬🇧 English_

# Conspiratio.Lib

This is the current state of the C# .NET Standard 2.0 class library containing the gameplay logic of Conspiratio, taken from the [Conspiratio WinForms client](https://github.com/Conspiratio/Conspiratio.WinForms). The library is not yet complete, but it already contains the most important classes and methods and serves as the foundational building block for the [Godot client](https://github.com/Conspiratio/Conspiratio.Godot).

## Package

[![Nuget](https://img.shields.io/nuget/v/Conspiratio.Lib)](https://www.nuget.org/packages/Conspiratio.Lib/) [![GitHub release (latest SemVer)](https://img.shields.io/github/v/release/Conspiratio/Conspiratio.Lib)](https://github.com/Conspiratio/Conspiratio.Lib/releases) 

## Build

[![Push - Build and publish Lib](https://github.com/Conspiratio/Conspiratio.Lib/workflows/Push%20-%20Build%20and%20publish%20Lib/badge.svg)](https://github.com/Conspiratio/Conspiratio.Lib/actions?query=workflow%3A%22Push+-+Build+and+publish+Lib%22)  
[![Pull-request - Build Lib](https://github.com/Conspiratio/Conspiratio.Lib/workflows/Pull-request%20-%20Build%20Lib/badge.svg)](https://github.com/Conspiratio/Conspiratio.Lib/actions?query=workflow%3A%22Pull-request+-+Build+Lib%22)  
[![CodeQL](https://github.com/Conspiratio/Conspiratio.Lib/workflows/CodeQL/badge.svg)](https://github.com/Conspiratio/Conspiratio.Lib/actions?query=workflow%3ACodeQL)

The project was created with: Visual Studio 2019

For a manual build, simply open the solution `Conspiratio.Lib.sln` and compile it.

## System requirements / dependencies
- .NET Standard 2.0 (no dependencies)

# About the game Conspiratio

The fan project called "Conspiratio" is a free early-modern economic simulation that draws heavily on the cult game "Die Fugger 2".

At the start, the player inherits a run-down production site and the modest savings of a relative. With these, they can prove their skill as a merchant by manufacturing and selling goods, making well-considered investments, or establishing themselves as a shrewd exporter. The player can use the newly gained wealth and the influence that comes with it to:

- acquire even more production sites,
- gain titles and privileges,
- send out spies and saboteurs,
- manipulate respected office holders,
- or even become a powerful office holder themselves.

But beware! Some competitors will not shy away from vile measures either ...

# About this repository

The goal is a rewrite of the user interface as well as a port and refactoring of the gameplay logic and the entire architecture from the current Windows Forms version to a Godot game, because there we have far more multimedia and, above all, graphical possibilities, and there is a degree of platform independence. This new [Godot client](https://github.com/Conspiratio/Conspiratio.Godot) will be fully open source; we want to involve other people in the collaboration and co-development as easily as possible, and the hobby project is meant to grow into a community project, by fans for fans.

GitHub Issues are intended to serve for planning and steering development.

# Getting involved

Want to contribute to this project? Great! Just get in touch via [Discord](https://discord.gg/dxkC5DPgRY) or, old-school, by <a href="&#109;&#97;&#105;&#108;&#116;&#111;&#58;%6D%61%69%6C%40%63%6F%6E%73%70%69%72%61%74%69%6F%2E%6E%65%74">e-mail</a> and we'll work out the details.  
_Any help is welcome._

## Git workflow

**Important: we never commit or push directly to the master branch!**  
The reason is simply a lack of transparency and the missing four-eyes principle, i.e. missing review by at least one other developer.

For every change to Conspiratio a new, personal branch must therefore always be created. The branch name should always start with one of the following names, followed by a slash:
- improvement (= improving the code or a game feature, including refactorings)
- fix (= a correction)
- feature (= a new game feature)

_Example:_ fix/crash-on-raid

Umlauts and special characters should be avoided; also, due to technical restrictions, spaces cannot be used in branch names, which is why we use hyphens instead.

Once your own branch is stable and contains all the desired changes/additions, a pull request to merge into the master branch can be opened. It should always be assigned to another developer for review, who does a small code review, gives feedback on the code if necessary, and then merges the branch after any corrections. You should only merge your own branches yourself in exceptional cases (e.g. time pressure).

## Code guidelines

As coding guidelines for C#, we use the following reference in particular for new code, as it has by now established itself as the standard:  
https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/inside-a-program/coding-conventions

Regarding naming and standards, this additional reference is used:  
https://www.dofactory.com/reference/csharp-coding-standards

Please note that we use German as the language for the comments in the code and for most identifiers, because the entire existing codebase is already built in German. Of course, not every keyword in every method has to be entirely German — for example, `GetUmsatzProSpieler` is perfectly legitimate (since `Get` should just be standard for every developer), whereas something like `GetVolumeOfSalesPerPlayer` would be problematic, since we won't find such terms anywhere else, neither in the game interface nor in the existing code, and it can therefore quickly cause confusion about what is meant.

Old code can and should gradually be migrated to these guidelines so that there is no mess later on, but that is not the highest priority for now. If, however, you change or refactor older code, you should make the effort to apply the new guidelines there too, following the scout motto:  
_Always leave a place (the code) in a better state than you found it._

## Documentation

The documentation of extensive features or other interesting methods, classes, etc. in the code is done in the GitHub wiki. The GitHub wiki is meant exclusively for the technical documentation and not for the documentation aimed at players; there will be a separate wiki for that.

## Changelog

First of all: we use several ideas from this concept: https://keepachangelog.com/en/1.0.0/

The changelog is maintained in the file CHANGELOG.md, right here in the root. It is important that every change is documented here, always in the "Unreleased" section. Conversely, this means that every pull request must therefore also always contain a change to the changelog file, otherwise it is not complete.

In the changelog we use the following groups to categorize the changes:

- Erweiterungen (additions)
- Änderungen (changes)
- Korrekturen (fixes)
- Balancing

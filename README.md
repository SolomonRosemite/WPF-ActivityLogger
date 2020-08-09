# WPF-ActivityLogger
Keep track of the programs you use

## Introduction

- The purpose of this Application is to have a good overview of the you use.

## How it Works

- The WPF-Activity-Logger is a Program split in two (Activity Logger) & (My Activity Logs).

- But in short.
- The Activity Logger gets the program by getting the Foreground process of the currently used Window.
- The data then gets saved to a Json where now the My Activity Logs(WPF) reads the Json file and displays the usage of the programs.

## Activity Logger
- The Activity Logger logs each minute what program the user has been using.
- These are getting then saved to the "SavedActivities.json" file.

## My Activity Logs
- The My Activity Logs is a WPF(Windows Presentation Foundation).
-  It's purpose is to view the activities on an desktop application.

![](https://github.com/SolomonRosemite/WPF-ActivityLogger/blob/master/ActivityLogger/assets/Example.gif?raw=true)

## Json File:
![](https://github.com/SolomonRosemite/WPF-ActivityLogger/blob/master/ActivityLogger/assets/Example.PNG?raw=true)

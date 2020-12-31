import 'package:flutter/material.dart';

class ActivityAppearance {
  Color backgroundColor;
  String imagePath;

  ActivityAppearance(String activityName) {
    activityName = activityName.trim();

    switch (activityName) {
      case 'Visual Studio Code':
        this.imagePath = setPath('vscode.png');
        this.backgroundColor = Colors.blueGrey[700];
        return;
      case 'Google Chrome':
        this.imagePath = setPath('google-chrome.jpg');
        this.backgroundColor = Colors.grey[700];
        return;
      case 'Spotify':
        this.backgroundColor = Colors.green[900];
        this.imagePath = setPath('spotify.png');
        return;
      case 'Oracle VM VirtualBox':
        this.imagePath = setPath('virtualbox.png');
        this.backgroundColor = Colors.blueGrey[300];
        return;
      case 'Twitch':
        this.imagePath = setPath('Twitch.png');
        this.backgroundColor = Colors.purple[300];
        return;
      case 'Microsoft Visual Studio':
        this.imagePath = setPath('Microsoft Visual Studio.png');
        this.backgroundColor = Colors.purple[300];
        return;
      case 'IntelliJ IDEA':
        this.imagePath = setPath('IntelliJ IDEA.png');
        this.backgroundColor = Colors.red[300];
        return;
      case 'Android Studio':
        this.imagePath = setPath('Android Studio.png');
        this.backgroundColor = Colors.green[400];
        return;
      case 'GitHub Desktop':
        this.imagePath = setPath('GitHub Desktop.png');
        this.backgroundColor = Colors.pink[200];
        return;
      case 'WhatsApp':
        this.imagePath = setPath('WhatsApp.png');
        this.backgroundColor = Colors.green[300];
        return;
      case 'Photoshop':
        this.imagePath = setPath('Photoshop.png');
        this.backgroundColor = Colors.blue[700];
        return;
      case 'cmd':
        this.imagePath = setPath('cmd.png');
        this.backgroundColor = Colors.grey[500];
        return;
      case 'WINWORD':
        this.imagePath = setPath('word.png');
        this.backgroundColor = Colors.blue[400];
        return;
      case 'VALORANT':
        this.imagePath = setPath('VALORANT.png');
        this.backgroundColor = Colors.red[600];
        return;
      case 'Mozilla Firefox':
        this.imagePath = setPath('Mozilla Firefox.png');
        this.backgroundColor = Colors.deepOrange[700];
        return;
      case 'WindowsTerminal':
        this.imagePath = setPath('cmd.png');
        this.backgroundColor = Colors.grey[500];
        return;
      case 'Overwatch':
        this.imagePath = setPath('Overwatch.png');
        this.backgroundColor = Colors.grey[900];
        return;
      case 'Opera':
        this.imagePath = setPath('Opera.png');
        this.backgroundColor = Colors.red[200];
        return;
      case 'Netflix':
        this.imagePath = setPath('Netflix.png');
        this.backgroundColor = Colors.red[700];
        return;
      case 'csgo':
        this.imagePath = setPath('csgo.png');
        this.backgroundColor = Colors.grey[800];
        return;
    }

    if (activityName.contains('FL Studio')) {
      this.imagePath = setPath('fruity loops.png');
      this.backgroundColor = Colors.grey[900];
      return;
    } else if (activityName.contains('qemu-system')) {
      this.imagePath = setPath('Android Studio.png');
      this.backgroundColor = Colors.green[400];
      return;
    } else if (activityName.contains('Blizzard')) {
      this.imagePath = setPath('Blizzard.png');
      this.backgroundColor = Colors.grey[900];
      return;
    }
    this.imagePath = setPath('undefined.png'); // todo: add random image for unknown items.
    this.backgroundColor = Colors.blue[300];
  }

  String setPath(String filename) => 'assets/images/$filename';
}

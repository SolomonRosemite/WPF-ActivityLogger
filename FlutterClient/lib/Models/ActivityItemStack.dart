import 'package:Activities/Models/ActivityItem.dart';
import 'package:flutter/material.dart';

class ActivityItemStack {
  static List<ActivityItem> activities = [
    new ActivityItem.d(backgroundColor: Colors.green[400], name: "Android Studio", names: [
      "qemu-system"
    ]),
    new ActivityItem.d(backgroundColor: Colors.grey[900], name: "Blizzard", names: [
      "battle.net"
    ]),
    new ActivityItem(backgroundColor: Colors.green[300], name: "Minecraft"),
    new ActivityItem(backgroundColor: Colors.grey[600], name: "Unity"),
    new ActivityItem(backgroundColor: Colors.red[300], name: "Postman"),
    new ActivityItem(backgroundColor: Colors.purple[300], name: "JetBrains Rider"),
    new ActivityItem.d(backgroundColor: Colors.grey[500], name: "Console", names: [
      "WindowsTerminal",
      "Termi",
      "cmd"
    ]),
    new ActivityItem(backgroundColor: Colors.blue[600], name: "Discord"),
    new ActivityItem(backgroundColor: Colors.grey[800], name: "Counter Strike"),
    new ActivityItem(backgroundColor: Colors.grey[900], name: "FL Studio"),
    new ActivityItem.d(backgroundColor: Colors.pink[300], name: "Notepad", names: [
      "note"
    ]),
    new ActivityItem(backgroundColor: Colors.pink[200], name: "GitHub Desktop"),
    new ActivityItem(backgroundColor: Colors.grey[700], name: "Figma"),
    new ActivityItem(backgroundColor: Colors.grey[700], name: "Google Chrome"),
    new ActivityItem(backgroundColor: Colors.green[300], name: "Atom"),
    new ActivityItem.d(backgroundColor: Colors.grey[600], name: "Google", names: [
      "Google keep",
      "Google Calender",
    ]),
    new ActivityItem(backgroundColor: Colors.red[300], name: "IntelliJ IDEA"),
    new ActivityItem.d(backgroundColor: Colors.blueGrey[300], name: "Oracle VM VirtualBox", names: [
      "VirtualBox"
    ]),
    new ActivityItem(backgroundColor: Colors.blueGrey[700], name: "Visual Studio Code"),
    new ActivityItem.d(backgroundColor: Colors.purple[300], name: "Microsoft Visual Studio", names: [
      "Visual Studio"
    ]),
    new ActivityItem.d(backgroundColor: Colors.deepOrange[600], name: "Mozilla Firefox", names: [
      "Firefox"
    ]),
    new ActivityItem(backgroundColor: Colors.red[700], name: "Netflix"),
    new ActivityItem(backgroundColor: Colors.blue[700], name: "Photoshop"),
    new ActivityItem(backgroundColor: Colors.grey[900], name: "Overwatch"),
    new ActivityItem(backgroundColor: Colors.red[200], name: "Opera"),
    new ActivityItem(backgroundColor: Colors.green[900], name: "Spotify"),
    new ActivityItem(backgroundColor: Colors.purple[300], name: "Twitch"),
    new ActivityItem(backgroundColor: Colors.red[600], name: "VALORANT"),
    new ActivityItem(backgroundColor: Colors.green[300], name: "WhatsApp"),
    new ActivityItem(backgroundColor: Colors.blue[400], name: "Word"),
    new ActivityItem.d(backgroundColor: Colors.blue[300], name: "Windows App", names: [
      "windows",
      "microsoft"
    ]),
  ];
}

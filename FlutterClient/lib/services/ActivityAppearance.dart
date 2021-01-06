import 'package:Activities/Models/ActivityItem.dart';
import 'package:Activities/Models/ActivityItemStack.dart';
import 'package:flutter/material.dart';

class ActivityAppearance {
  Color backgroundColor;
  String imagePath;

  ActivityAppearance(String activityName) {
    activityName = activityName.trim();

    var item = ActivityItemStack.activities.firstWhere(
      (element) => element.name.toLowerCase().contains(activityName.toLowerCase()) || activityName.toLowerCase().contains(element.name.toLowerCase()),
      orElse: () {
        return new ActivityItem(name: "UnidentifiedActivity", backgroundColor: null);
      },
    );

    if (item.name == "UnidentifiedActivity") {
      this.imagePath = "assets/images/undefined.png"; // todo: add random image for unknown items.
      this.backgroundColor = Colors.blue[300];
      return;
    }

    this.backgroundColor = item.backgroundColor;
    this.imagePath = item.imagePath;
  }
}

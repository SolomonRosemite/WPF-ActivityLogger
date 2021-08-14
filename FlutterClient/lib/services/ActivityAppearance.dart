import 'package:activities/Models/ActivityItem.dart';
import 'package:activities/Models/ActivityItemStack.dart';
import 'package:flutter/material.dart';

class ActivityAppearance {
  Color backgroundColor;
  String imagePath = "";

  ActivityAppearance(String activityName) {
    activityName = activityName.trim();

    var item = getActivityItem(ActivityItemStack.activities, activityName);

    if (item.name == "UnidentifiedActivity") {
      this.imagePath = "images/undefined.png";
      this.backgroundColor = Colors.blue[300];
      return;
    }

    this.backgroundColor = item.backgroundColor;
    this.imagePath = item.imagePath;
  }

  ActivityItem getActivityItem(List<ActivityItem> items, String activityName) {
    return ActivityItemStack.activities.firstWhere(
      (element) {
        if (element.names != null) {
          for (var item in element.names) {
            var val = item.toLowerCase().contains(activityName.toLowerCase()) || activityName.toLowerCase().contains(item.toLowerCase());

            if (val) {
              return true;
            }
          }
        }

        return element.name.toLowerCase().contains(activityName.toLowerCase()) || activityName.toLowerCase().contains(element.name.toLowerCase());
      },
      orElse: () {
        return new ActivityItem(name: "UnidentifiedActivity", backgroundColor: null);
      },
    );
  }
}

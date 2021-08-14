import 'package:activities/Backend/Backend.dart';
import 'package:activities/Models/Activity.dart';
import 'package:activities/common/ActivityCard.dart';
import 'package:activities/services/HelperUtilityClass.dart';
import 'package:flutter/material.dart';

enum ActivityType {
  today,
  yesterday,
  week,
  month,

  total,
}

class ViewActivities extends StatefulWidget {
  final ActivityType type;

  ViewActivities(this.type);
  @override
  _ViewActivitiesState createState() => _ViewActivitiesState();
}

class _ViewActivitiesState extends State<ViewActivities> {
  List<Activity> activities;

  @override
  void initState() {
    activities = loadData(widget.type);
    super.initState();
  }

  List<Activity> loadData(ActivityType type) {
    switch (type) {
      case ActivityType.today:
        return HelperUtilityClass.getActivityPerDay(DateTime.now());
        break;
      case ActivityType.yesterday:
        return HelperUtilityClass.getActivityPerDay(DateTime.now().subtract(new Duration(days: 1)));
        break;
      case ActivityType.week:
        return HelperUtilityClass.getActivityPerWeek(DateTime.now());
        break;
      case ActivityType.month:
        return HelperUtilityClass.getActivityPerMonth(DateTime.now());
        break;
      // case ActivityType.personalized:
      //   break;
      case ActivityType.total:
        return HelperUtilityClass.getActivityTotal(DateTime.now());
        break;
    }
    Backend.postReport('ActivityType not defined.');
    return HelperUtilityClass.getActivityPerDay(DateTime.now());
  }

  @override
  Widget build(BuildContext context) {
    if (activities.isEmpty) {
      return Container(
        child: Center(
            child: Text(
          'No Activites for Now. 😉',
          style: TextStyle(fontSize: 22),
        )),
      );
    }
    return Container(
      child: ListView.builder(
        itemCount: activities.length + 1,
        padding: EdgeInsets.only(top: 15),
        itemBuilder: (context, index) {
          int i = index - 1;
          if (index == 0) {
            String type = widget.type.toString().split('.')[1];
            return Container(
              padding: EdgeInsets.only(bottom: 10),
              child: Center(
                child: Text(
                  '${type[0].toUpperCase()}${type.substring(1)}',
                  style: TextStyle(
                    fontSize: 25,
                    fontWeight: FontWeight.bold,
                  ),
                ),
              ),
            );
          }
          return Hero(tag: i.toString(), child: ActivityCardWidget(activities[i], i.toString()));
        },
      ),
    );
  }
}

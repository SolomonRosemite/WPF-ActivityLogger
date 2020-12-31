import 'package:flutter/cupertino.dart';

class Activity {
  final DateTime date;
  final String activityName;
  int timeSpent;

  Activity({@required this.activityName, @required this.date, @required this.timeSpent});
}

import 'package:Activities/services/ActivityAppearance.dart';
import 'package:Activities/common/ViewActivityPage.dart';
import 'package:Activities/common/FadePageRoute.dart';
import 'package:Activities/Models/Activity.dart';
import 'package:flutter/material.dart';
import 'package:Activities/main.dart';
import 'package:intl/intl.dart';

bool inHours = false;

class ActivityCardWidget extends StatefulWidget {
  final Activity activity;
  final String tag;

  ActivityCardWidget(this.activity, this.tag);
  @override
  ActivityCardWidgetState createState() => ActivityCardWidgetState();
}

class ActivityCardWidgetState extends State<ActivityCardWidget> {
  ActivityAppearance appearance;
  Activity activity;

  @override
  void initState() {
    activity = widget.activity;
    appearance = new ActivityAppearance(activity.activityName);
    // print(appearance.imagePath);
    super.initState();
  }

  String getTimeSpent(int timeSpent, bool inHours) {
    if (!inHours) {
      return 'Time Spent: $timeSpent Minutes';
    }
    return 'Time Spent: ${(timeSpent / 60).toStringAsFixed(1)} Hours';
  }

  @override
  Widget build(BuildContext context) {
    return WillPopScope(
      onWillPop: () async {
        MyApp.onPage = false;
        return true;
      },
      child: Container(
        padding: const EdgeInsets.symmetric(horizontal: 20.0, vertical: 10.0),
        child: Material(
          color: appearance.backgroundColor,
          elevation: 20,
          borderRadius: BorderRadius.all(Radius.circular(8.0)),
          child: InkWell(
            onTap: () {
              if (MyApp.onPage == false) {
                Navigator.of(context).push(FadePageRoute(widget: ActivityDetail(activity, widget.tag)));
                MyApp.onPage = true;
              }
            },
            child: Container(
              padding: const EdgeInsets.all(16.0),
              child: Row(
                children: <Widget>[
                  Expanded(
                    child: Column(
                      mainAxisSize: MainAxisSize.max,
                      children: <Widget>[
                        Column(
                          children: <Widget>[
                            Row(
                              children: <Widget>[
                                Spacer(flex: 1),
                                Row(
                                  children: <Widget>[
                                    SizedBox(
                                      child: Text(
                                        activity.activityName,
                                        textAlign: TextAlign.left,
                                        style: TextStyle(
                                          color: Colors.white,
                                          fontSize: 18,
                                          fontWeight: FontWeight.bold,
                                        ),
                                      ),
                                      width: 270,
                                    ),
                                    SizedBox(width: 3),
                                  ],
                                ),
                                Spacer(flex: 98),
                                ClipRRect(
                                  borderRadius: BorderRadius.circular(0),
                                  // borderRadius: BorderRadius.circular(20.0),
                                  child: Image.asset(
                                    appearance.imagePath,
                                    width: 48.0,
                                    height: 48.0,
                                    fit: BoxFit.cover,
                                  ),
                                ),
                                Spacer(flex: 1),
                              ],
                            ),
                            Column(
                              children: <Widget>[
                                SizedBox(height: 1.0),
                                Align(
                                  alignment: Alignment.centerLeft,
                                  child: SizedBox(
                                    width: 260,
                                    child: Text(
                                      getTimeSpent(activity.timeSpent, inHours),
                                      textAlign: TextAlign.left,
                                      style: TextStyle(color: Colors.white, fontSize: 16, fontWeight: FontWeight.normal),
                                    ),
                                  ),
                                ),
                              ],
                            ),
                          ],
                        ),
                        SizedBox(height: 5.0),
                        Row(
                          mainAxisSize: MainAxisSize.max,
                          mainAxisAlignment: MainAxisAlignment.start,
                          crossAxisAlignment: CrossAxisAlignment.center,
                          children: <Widget>[
                            Expanded(
                              child: Align(
                                alignment: Alignment.bottomRight,
                                child: Text(
                                  DateFormat('dd.MMM yyyy').format(activity.date),
                                  style: TextStyle(color: Colors.white),
                                ),
                              ),
                            ),
                          ],
                        ),
                      ],
                    ),
                  ),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }
}

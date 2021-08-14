import 'package:activities/Models/Activity.dart';
import 'package:activities/common/ActivityCard.dart';
import 'package:activities/services/ActivityAppearance.dart';
import 'package:activities/services/HelperUtilityClass.dart';
import 'package:flutter/material.dart';

class ActivityDetail extends StatefulWidget {
  final Activity activity;
  final String tag;

  ActivityDetail(this.activity, this.tag);

  @override
  ActivityDetailState createState() => new ActivityDetailState();
}

class ActivityDetailState extends State<ActivityDetail> {
  List<Activity> items;
  Color colorScheme;

  bool viewAll = false;

  @override
  void initState() {
    colorScheme = new ActivityAppearance(widget.activity.activityName).backgroundColor;
    super.initState();
  }

  Widget viewAllActivityCards() {
    return Scaffold(
      backgroundColor: colorScheme,
      appBar: AppBar(
        elevation: 0,
        backgroundColor: colorScheme,
      ),
      body: Column(
        children: <Widget>[
          SizedBox(height: 20.0),
          Text(
            widget.activity.activityName,
            style: TextStyle(
              fontSize: 25,
              color: Colors.white,
              fontWeight: FontWeight.bold,
            ),
            textAlign: TextAlign.center,
          ),
          Text(
            'For each entry',
            style: TextStyle(
              fontSize: 13,
              color: Colors.white,
              fontWeight: FontWeight.normal,
            ),
          ),
          SizedBox(height: 20.0),
          new Expanded(
            child: ListView.builder(
              itemCount: items.length,
              itemBuilder: (context, index) {
                return Hero(tag: (index).toString(), child: ActivityCardWidget(items[index], (index).toString()));
              },
            ),
          ),
        ],
      ),
    );
  }

  Widget viewActivityCards() {
    return Scaffold(
      backgroundColor: colorScheme,
      appBar: AppBar(
        elevation: 0,
        backgroundColor: colorScheme,
      ),
      body: Column(
        children: <Widget>[
          SizedBox(height: 20.0),
          Text(
            widget.activity.activityName,
            style: TextStyle(
              fontSize: 25,
              color: Colors.white,
              fontWeight: FontWeight.bold,
            ),
          ),
          SizedBox(height: 20.0),
          Hero(
            tag: widget.tag,
            child: ActivityCardWidget(widget.activity, widget.tag),
          ),
          Spacer(),
          FlatButton(
            onPressed: () {
              setState(() {
                viewAll = !viewAll;
              });
            },
            child: Text(
              'View all Activities',
              style: TextStyle(color: Colors.white),
            ),
          ),
          SizedBox(height: 20.0),
        ],
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    if (viewAll) {
      if (items == null) {
        items = HelperUtilityClass.getAllActivitiesOf(widget.activity.activityName);
      }
      return viewAllActivityCards();
    }
    return viewActivityCards();
  }
}

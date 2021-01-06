import 'dart:math';

import 'package:Activities/Backend/Backend.dart';
import 'package:Activities/common/ActivityCard.dart';
import 'package:Activities/pages/ViewActivities.dart';
import 'package:Activities/services/HelperUtilityClass.dart';
import 'package:cloud_firestore/cloud_firestore.dart';
import 'package:flutter/material.dart';
import 'package:Activities/main.dart';
import 'package:minimize_app/minimize_app.dart';
import 'package:flutter_phoenix/flutter_phoenix.dart';

class Home extends StatefulWidget {
  @override
  _HomeState createState() => _HomeState();
}

class _HomeState extends State<Home> with TickerProviderStateMixin {
  TabController tabController;
  ActivityType activityType;
  DocumentReference ref;
  Random r;
  Color color;

  BuildContext myContext;
  bool update = true;

  bool expectedUpdate = false;

  final snackBarFetchingMessage = SnackBar(
    duration: const Duration(seconds: 1),
    content: Text('Fetching new data'),
    action: SnackBarAction(label: 'Ok!', onPressed: () {}),
  );

  final snackBarFetchCompleteMessage = SnackBar(
    content: Text('Updated!'),
    backgroundColor: Colors.green,
  );

  @override
  void initState() {
    super.initState();
    r = new Random();

    activityType = ActivityType.today;
    tabController = new TabController(length: 1, initialIndex: 0, vsync: this);
    print(MyApp.activities.length);

    ref = Backend.firestore.collection("users").doc(Backend.uid);

    ref.snapshots().listen((info) {
      if (info.data()['action'] == 'updated') {
        getLatestActivities();
      }
    });

    color = Colors.primaries[r.nextInt(Colors.primaries.length)];
  }

  void getLatestActivities() {
    Backend.getLatestActivityJson(DateTime.now(), Backend.uid).then((data) {
      HelperUtilityClass.activities(data);

      setState(() {
        activityType = activityType;
      });

      if (expectedUpdate == true) {
        Scaffold.of(myContext).showSnackBar(snackBarFetchCompleteMessage);
        expectedUpdate = false;
      }

      ref.update({
        'action': 'waiting'
      });
    });
  }

  Future<void> updateActivityFile() async {
    Scaffold.of(myContext).showSnackBar(snackBarFetchingMessage);

    expectedUpdate = true;

    if ((await ref.get()).data()['action'] == 'updateActivityFile') {
      await ref.set(
        {
          'action': 'waiting'
        },
      );
    }

    await ref.set(
      {
        'action': 'updateActivityFile'
      },
    );
  }

  Widget viewActivities(ActivityType type, bool update) {
    if (update) {
      this.update = false;
      return new ViewActivities(type);
    }
    this.update = true;

    Future.delayed(const Duration(milliseconds: 0), () {
      setState(() {
        this.update = true;
      });
    });

    return Container(
      child: Center(
        child: Text('Updating'),
      ),
    );
  }

  String getCurrentView(ActivityType type) {
    String s = type.toString().split(".")[1];

    return ('${s[0].toUpperCase()}${s.substring(1)}');
  }

  void showAlertDialog() {
    showDialog(
      context: context,
      builder: (BuildContext context) {
        return AlertDialog(
          title: Text('Yikes'),
          content: Text('This functionality is not available yet. Please be patient.'),
          actions: [
            FlatButton(
              textColor: color,
              onPressed: () {
                Navigator.pop(context);
              },
              child: Text('Ok'),
            ),
          ],
        );
      },
    );
  }

  @override
  Widget build(BuildContext context) {
    return new WillPopScope(
      onWillPop: () async {
        MinimizeApp.minimizeApp();
        return false;
      },
      child: Scaffold(
        appBar: AppBar(
          title: Text('My Activities'),
          centerTitle: true,
          actions: [
            IconButton(
              icon: Icon(Icons.timer),
              onPressed: () {
                if (tabController.index != 0) {
                  return;
                }
                setState(() {
                  inHours = !inHours;
                });
              },
            ),
            IconButton(
              icon: Icon(Icons.refresh),
              onPressed: () => updateActivityFile(),
            )
          ],
          backgroundColor: color,
        ),
        drawer: Drawer(
          child: Column(
            // mainAxisAlignment: MainAxisAlignment.start,
            children: <Widget>[
              Container(
                height: 100,
                child: DrawerHeader(
                  child: Align(
                    child: Text(
                      'My Activities 📅',
                    ),
                    alignment: Alignment.centerLeft,
                  ),
                  decoration: BoxDecoration(
                    color: color,
                  ),
                ),
              ),
              Container(
                padding: const EdgeInsets.only(
                  left: 10.0,
                ),
                child: Text(
                  'Viewing ${getCurrentView(activityType)}',
                  style: TextStyle(fontSize: 20, fontWeight: FontWeight.bold),
                ),
              ),
              ListTile(
                title: Text('Today'),
                onTap: () {
                  if (activityType != ActivityType.today) {
                    setState(() {
                      activityType = ActivityType.today;
                    });
                  }
                  Navigator.pop(context);
                },
              ),
              ListTile(
                title: Text('Yesterday'),
                onTap: () {
                  if (activityType != ActivityType.yesterday) {
                    setState(() {
                      activityType = ActivityType.yesterday;
                    });
                  }
                  Navigator.pop(context);
                },
              ),
              ListTile(
                title: Text('This Week'),
                onTap: () {
                  if (activityType != ActivityType.week) {
                    setState(() {
                      activityType = ActivityType.week;
                    });
                  }
                  Navigator.pop(context);
                },
              ),
              ListTile(
                title: Text('This Month'),
                onTap: () {
                  if (activityType != ActivityType.month) {
                    setState(() {
                      activityType = ActivityType.month;
                    });
                  }
                  Navigator.pop(context);
                },
              ),
              ListTile(
                title: Text('Total'),
                onTap: () {
                  if (activityType != ActivityType.total) {
                    setState(() {
                      activityType = ActivityType.total;
                    });
                  }
                  Navigator.pop(context);
                },
              ),
              Container(
                child: Divider(
                  thickness: 5,
                  color: Colors.black26,
                ),
              ),
              // Container(
              //   child: Row(
              //     children: [
              //       Expanded(
              //         child: ListTile(
              //             title: Text('Personalized'),
              //             onTap: () {
              //               Navigator.pop(context);
              //               showAlertDialog();

              //               // if (activityType != ActivityType.personalized) {
              //               //   setState(() {
              //               //     activityType = ActivityType.personalized;
              //               //   });
              //               // }
              //             }),
              //       ),
              //       Align(
              //         alignment: Alignment.centerRight,
              //         child: IconButton(
              //             icon: Icon(Icons.settings),
              //             onPressed: () {
              //               Navigator.pop(context);
              //               showAlertDialog();
              //             }), // todo: go to settings
              //       ),
              //     ],
              //   ),
              // ),
              Expanded(
                child: Align(
                  alignment: Alignment.bottomCenter,
                  child: ListTile(
                    title: Text(
                      'Sign Out',
                      style: TextStyle(color: Colors.red[300]),
                    ),
                    onTap: () async {
                      await Backend.prefs.setString("userSecret", null);
                      Phoenix.rebirth(context);
                      // Navigator.pop(context);
                      // showAlertDialog();
                    },
                  ),
                ),
              ),
            ],
          ),
        ),
        body: Builder(builder: (context) {
          myContext = context;
          return TabBarView(
            controller: tabController,
            children: [
              viewActivities(activityType, update),
              // new Container(
              //   child: Center(
              //     child: RaisedButton(onPressed: () {}),
              //   ),
              // ),
            ],
          );
        }),
        // bottomNavigationBar: new TabBar(
        //   controller: tabController,
        //   tabs: [
        //     Tab(
        //       icon: new Icon(Icons.timelapse),
        //     ),
        //     // Tab(
        //     //   icon: new Icon(Icons.timeline),
        //     // ),
        //   ],
        //   labelColor: Colors.red,
        //   unselectedLabelColor: Colors.blue,
        //   indicatorSize: TabBarIndicatorSize.label,
        //   indicatorPadding: EdgeInsets.all(5.0),
        //   indicatorColor: Colors.blueGrey[700],
        // ),
      ),
    );
  }
}

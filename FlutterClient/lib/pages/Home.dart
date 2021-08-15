import 'dart:math';

import 'package:activities/Backend/Backend.dart';
import 'package:activities/common/ActivityCard.dart';
import 'package:activities/pages/ViewActivities.dart';
import 'package:activities/services/HelperUtilityClass.dart';
import 'package:cloud_firestore/cloud_firestore.dart';
import 'package:flutter/material.dart';
import 'package:activities/main.dart';
import 'package:minimize_app/minimize_app.dart';
import 'package:flutter_phoenix/flutter_phoenix.dart';
import 'package:qr_flutter/qr_flutter.dart';

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

  // Future<void> handleOnRevealClick(BuildContext context) {
  Future<void> handleOnRevealClick() {
    Navigator.pop(context);
    return showDialog<void>(
      context: context,
      builder: (BuildContext ctx2) {
        return AlertDialog(
          title: Text(
            "Your Usersecret",
            textAlign: TextAlign.center,
          ),
          content: Container(
            height: 270,
            width: 300,
            child: Align(
              alignment: Alignment.center,
              child: Column(
                children: [
                  Text(
                    "Share this secret only between your devices.",
                    textAlign: TextAlign.center,
                  ),
                  SizedBox(height: 20),
                  QrImage(
                    data: Backend.prefs.getString("userSecret"),
                    version: QrVersions.auto,
                    size: 150.0,
                  ),
                  SizedBox(height: 20),
                  Text(
                    "Secret: " + Backend.prefs.getString("userSecret"),
                    textAlign: TextAlign.center,
                  ),
                ],
              ),
            ),
          ),
          actions: [
            FlatButton(
              textColor: Colors.blue,
              onPressed: () {
                Navigator.pop(ctx2);
              },
              child: Text('Close'),
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
          title: Text('Activities'),
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
            children: <Widget>[
              Container(
                height: 100,
                child: DrawerHeader(
                  child: Align(
                    child: Text(
                      'Activities 📅',
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
              Expanded(
                child: Column(
                  children: [
                    SizedBox.shrink(),
                    Spacer(),
                    ListTile(
                      title: Text(
                        'Reveal secret',
                        style: TextStyle(color: Colors.red[400]),
                      ),
                      onTap: handleOnRevealClick,
                    ),
                    ListTile(
                      title: Text(
                        'Signout',
                        style: TextStyle(color: Colors.red[400]),
                      ),
                      onTap: () async {
                        await Backend.prefs.setString("userSecret", null);
                        Phoenix.rebirth(context);
                      },
                    ),
                  ],
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
            ],
          );
        }),
      ),
    );
  }
}

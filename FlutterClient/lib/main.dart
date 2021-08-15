import 'dart:developer';

import 'package:activities/Models/IUser.dart';
import 'package:activities/Models/Activity.dart';
import 'package:activities/Backend/Backend.dart';
import 'package:activities/pages/SignIn.dart';
import 'package:activities/services/HelperUtilityClass.dart';
import 'package:firebase_core/firebase_core.dart';
import 'package:flutter/material.dart';
import 'package:activities/pages/Home.dart';

import 'package:flutter_phoenix/flutter_phoenix.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'dart:async';

void main() async {
  runApp(Phoenix(child: MyApp()));
}

class MyApp extends StatelessWidget {
  static Map<String, List<Activity>> activities;
  static bool onPage = false;

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Activities',
      theme: ThemeData(
        primarySwatch: Colors.blue,
        visualDensity: VisualDensity.adaptivePlatformDensity,
      ),
      home: MyHomePage(title: 'Activities'),
    );
  }
}

class MyHomePage extends StatefulWidget {
  MyHomePage({Key key, this.title}) : super(key: key);
  final String title;
  @override
  MyHomePageState createState() => MyHomePageState();
}

class MyHomePageState extends State<MyHomePage> {
  bool _visible = false;
  bool _visibleOffline = false;
  TextStyle textStyle;

  void initState() {
    MyApp.activities = new Map();
    textStyle = new TextStyle(color: Colors.grey[700]);

    initApp();
    new Future.delayed(const Duration(milliseconds: 100), () {
      setState(() {
        _visible = !_visible;
      });
    });
    super.initState();
  }

  Future<void> initApp() async {
    // If User is offline
    if (await Backend.hasInternet() == false) {
      new Future.delayed(const Duration(seconds: 1), () {
        setState(() {
          _visibleOffline = !_visibleOffline;
        });
      });
      return;
    }

    await Firebase.initializeApp();

    Backend.prefs = await SharedPreferences.getInstance();

    String secret = Backend.prefs.getString("userSecret");

    if (secret == null) {
      Navigator.push(
        context,
        MaterialPageRoute(builder: (context) => SignIn()),
      );
      return;
    } else {
      IUser user = await Backend.authenticate(secret);

      try {
        var res = await Backend.auth.signInWithEmailAndPassword(email: user.email, password: user.password);
        Backend.uid = res.user.uid;
      } catch (e) {
        Navigator.push(
          context,
          MaterialPageRoute(builder: (context) => SignIn()),
        );
        return;
      }
    }

    await HelperUtilityClass.setupApp(context);

    Navigator.push(
      context,
      MaterialPageRoute(builder: (context) => Home()),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Container(
        color: Colors.white,
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.center,
          mainAxisAlignment: MainAxisAlignment.center,
          mainAxisSize: MainAxisSize.max,
          children: [
            Spacer(flex: 4),
            Center(
              child: AnimatedOpacity(
                opacity: _visible ? 1.0 : 0.0,
                duration: Duration(milliseconds: 800),
                child: Image.asset(
                  "assets/icons/checked.gif",
                  height: 125.0,
                  width: 125.0,
                ),
              ),
            ),
            Spacer(flex: 4),
            AnimatedOpacity(
              opacity: _visibleOffline ? 1.0 : 0.0,
              duration: Duration(milliseconds: 800),
              child: Text(
                'Seems like you\'re offline. Please try again later',
                style: textStyle,
              ),
            ),
            Spacer(flex: 2),
            AnimatedOpacity(
              opacity: _visibleOffline ? 0.0 : 1.0,
              duration: Duration(milliseconds: 800),
              // child: Text('Loading...'),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.center,
                mainAxisAlignment: MainAxisAlignment.center,
                mainAxisSize: MainAxisSize.max,
                children: [
                  Text(
                    'Loading...',
                    style: textStyle,
                  ),
                  SizedBox(
                    width: 220,
                    child: Container(
                      child: Divider(
                        thickness: 2,
                        color: Colors.black26,
                      ),
                    ),
                  ),
                  Text(
                    'My Activities',
                    style: textStyle,
                  ),
                ],
              ),
            ),
            Spacer(flex: 1),
          ],
        ),
      ),
    );
  }
}

import 'package:Activities/Models/IUser.dart';
import 'package:Activities/Models/InternetStatus.dart';
import 'package:Activities/Models/Activity.dart';
import 'package:Activities/Backend/Backend.dart';
import 'package:Activities/pages/SignIn.dart';
import 'package:Activities/services/HelperUtilityClass.dart';
import 'package:cloud_firestore/cloud_firestore.dart';
import 'package:firebase_core/firebase_core.dart';
import 'package:flutter/material.dart';
import 'package:Activities/pages/Home.dart';

import 'dart:convert';
import 'dart:async';

import 'package:intl/intl.dart';
import 'package:shared_preferences/shared_preferences.dart';

void main() => runApp(MyApp());

class MyApp extends StatelessWidget {
  static Map<String, List<Activity>> activities;
  static Map<String, InternetStatus> statuses;
  static bool onPage = false;

  static DateTime beginDateOfCustom;
  static DateTime lastDateOfCustom;

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Home',
      theme: ThemeData(
        primarySwatch: Colors.blue,
        visualDensity: VisualDensity.adaptivePlatformDensity,
      ),
      home: MyHomePage(title: 'Home'),
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

  void initState() {
    MyApp.activities = new Map();
    MyApp.statuses = new Map<String, InternetStatus>();

    initApp();
    new Future.delayed(const Duration(seconds: 1), () {
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
        await Backend.auth.signInWithEmailAndPassword(email: user.email, password: user.password);
      } catch (e) {
        Navigator.push(
          context,
          MaterialPageRoute(builder: (context) => SignIn()),
        );
        return;
      }
    }

    HelperUtilityClass.setupApp(context);
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Container(
        color: Colors.blue,
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.center,
          mainAxisAlignment: MainAxisAlignment.center,
          mainAxisSize: MainAxisSize.max,
          children: [
            Spacer(flex: 3),
            Center(
              child: AnimatedOpacity(
                opacity: _visible ? 1.0 : 0.0,
                duration: Duration(milliseconds: 800),
                child: Icon(
                  Icons.computer, // todo: add another icon here
                  size: 150,
                  color: Colors.white, // todo: lerp betweet colors back and forth
                ),
              ),
            ),
            Spacer(flex: 3),
            AnimatedOpacity(
              opacity: _visibleOffline ? 1.0 : 0.0,
              duration: Duration(milliseconds: 800),
              child: Icon(
                Icons.note, // todo: add no internet icon here
                size: 70,
                color: Colors.red[400],
              ),
            ),
            Spacer(flex: 2)
          ],
        ),
      ),
    );
  }
}

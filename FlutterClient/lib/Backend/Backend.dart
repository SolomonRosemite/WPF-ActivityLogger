import 'dart:convert';

import 'package:Activities/Models/IUser.dart';
import 'package:cloud_firestore/cloud_firestore.dart';
import 'package:data_connection_checker/data_connection_checker.dart';

import 'package:firebase_storage/firebase_storage.dart' as store;
import 'package:cloud_firestore/cloud_firestore.dart' as fs;
import 'package:firebase_auth/firebase_auth.dart';

import 'package:http/http.dart' as http;
import 'package:intl/intl.dart';
import 'dart:async';

import 'package:shared_preferences/shared_preferences.dart';

class Backend {
  // static store.FirebaseStorage storage = store.FirebaseStorage.instance(app: _app, storageBucket: 'gs://flutter-homebackend.appspot.com');
  static store.FirebaseStorage storage = store.FirebaseStorage.instance;
  static FirebaseFirestore firestore = FirebaseFirestore.instance;
  static FirebaseAuth auth = FirebaseAuth.instance;
  static SharedPreferences prefs;
  static String uid;

  static String _url = "https://wpf-activitylogger-functions.netlify.app/.netlify/functions/app/auth";

  // static core.FirebaseApp _app;

  static Future<bool> hasInternet() async {
    return await DataConnectionChecker().hasConnection;
  }

  static Future<http.Response> _fetchUser(String secret) {
    return http.post(_url, body: {
      'secret': secret
    });
  }

  static Future<IUser> authenticate(String secret) async {
    var res = await _fetchUser(secret);

    var map = jsonDecode(res.body);

    print(map['uuid']);
    print(map['uuid']);
    print(map['uuid']);
    print(map['uuid']);
    print(map['uuid']);
    print(map['uuid']);
    print(map['uuid']);
    print(map['uuid']);
    print(map['uuid']);
    print(map['uuid']);
    print(map['uuid']);
    print(map['uuid']);
    print(map['uuid']);
    print(map['uuid']);
    print(map['uuid']);
    print(map['uuid']);
    print(map['uuid']);
    print(map['uuid']);
    print(map['uuid']);
    print(map['uuid']);
    print(map['uuid']);
    print(map['uuid']);
    print(map['uuid']);
    print(map['uuid']);
    print(map['uuid']);
    print(map['uuid']);

    if (map['error'] != null) {
      return null;
    }

    return IUser.fromJson(jsonDecode(res.body));
  }

  // Todo ...
  static Future<fs.DocumentSnapshot> getPersonalizedDates() async {
    return await (firestore.collection('preferences').doc('Activitiesdata').get());
  }

  static Future<String> getLatestActivityJson(DateTime dateTime, String uid) async {
    String data;

    try {
      String date = DateFormat('dd.MM.yyyy').format(dateTime);

      // Todo: Fix me
      // var ref = storage.ref().child(uid).child(date).child('SavedActivities.json');
      print(uid);
      var ref = storage.ref().child(uid).child("31.12.2020").child('SavedActivities.json');

      final String url = await ref.getDownloadURL();
      data = (await http.get(url)).body;
    } catch (e) {
      print(e);
      // return await getLatestActivityJson(dateTime.subtract(new Duration(days: 1)), uid);
    }

    return data;
  }

  static Future postReport(String issue, DateTime date) async {
    firestore.collection('reports').doc().set({
      'Issue': issue,
      'Date': date
    });
  }
}

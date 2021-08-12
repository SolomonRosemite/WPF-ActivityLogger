import 'dart:convert';
import 'dart:developer';

import 'package:Activities/Models/IUser.dart';
import 'package:cloud_firestore/cloud_firestore.dart';
import 'package:data_connection_checker/data_connection_checker.dart';

import 'package:firebase_storage/firebase_storage.dart' as store;
import 'package:firebase_auth/firebase_auth.dart';
import 'package:flutter/foundation.dart';

import 'package:http/http.dart' as http;
import 'package:intl/intl.dart';
import 'dart:async';
import 'package:flutter/foundation.dart' show kIsWeb;

import 'package:shared_preferences/shared_preferences.dart';

class Backend {
  // static store.FirebaseStorage storage = store.FirebaseStorage.instance(app: _app, storageBucket: 'gs://flutter-homebackend.appspot.com');
  static store.FirebaseStorage storage = store.FirebaseStorage.instance;
  static FirebaseFirestore firestore = FirebaseFirestore.instance;
  static FirebaseAuth auth = FirebaseAuth.instance;
  static SharedPreferences prefs;
  static String uid;
  static String _url = kReleaseMode ? "https://wpf-activitylogger-functions.netlify.app/.netlify/functions/app/auth" : "http://localhost:3000/.netlify/functions/app/auth";

  static Future<bool> hasInternet() async {
    return kIsWeb ?? await DataConnectionChecker().hasConnection;
  }

  static Future<http.Response> _fetchUser(String secret) {
    return http.post(_url,
        headers: {
          'Content-type': 'application/json'
        },
        body: jsonEncode(<String, String>{
          'secret': secret
        }));
  }

  static Future<IUser> authenticate(String secret) async {
    var res = await _fetchUser(secret);

    var map = jsonDecode(res.body);

    if (map['error'] != null) {
      return null;
    }

    return IUser.fromJson(jsonDecode(res.body));
  }

  static Future<String> getLatestActivityJson(DateTime dateTime, String uid) async {
    int tries = 0;
    int threshold = 364;

    try {
      String date = DateFormat('dd.MM.yyyy').format(dateTime);

      var ref = storage.ref("/$uid/$date/SavedActivities.json");

      final String url = await ref.getDownloadURL();
      return (await http.get(url)).body;
    } catch (e) {
      if (++tries > threshold) {
        // Todo: When returning null, handle for every function that uses this function that might expect null.
        return null;
      }

      // If there was nothing for this day. We try to get the previous day.
      return await getLatestActivityJson(dateTime.subtract(new Duration(days: 1)), uid);
    }
  }

  static Future postReport(String issue, {dynamic exception}) async {
    firestore.collection('reports').doc().set({
      'Issue': issue,
      "Exception": exception.toString(),
      'Date': DateTime.now()
    });
  }
}

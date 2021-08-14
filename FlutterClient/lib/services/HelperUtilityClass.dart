import 'dart:convert';

import 'package:activities/Backend/Backend.dart';
import 'package:activities/Models/Activity.dart';
import 'package:activities/main.dart';
import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'dart:collection';

class HelperUtilityClass {
  static void assignCustomDates(DateTime begin, DateTime last, bool fromToday) {
    // MyApp.beginDateOfCustom = begin;
    // MyApp.lastDateOfCustom = (!fromToday) ? last : DateTime.now();
  }

  // For ActivityTypes
  static List<Activity> getActivityPerDay(DateTime dateTime) {
    var date = dateTimeToActivityDate(dateTime);

    if (MyApp.activities[date] != null) {
      return MyApp.activities[date];
    }
    return [];
  }

  static Future setupApp(BuildContext context) async {
    // Get Dates for Personalized tab
    // var datesAsync = Backend.getPersonalizedDates();

    // Get Latest Activity json
    var data = await Backend.getLatestActivityJson(DateTime.now(), Backend.uid);

    // Load activities
    activities(data);

    // var dates = await datesAsync;

    // var beginDate = HelperUtilityClass.stringToActivityDateTime(dates.data()['beginDate']);
    // var lastDate = HelperUtilityClass.stringToActivityDateTime(dates.data()['lastDate']);

    // HelperUtilityClass.assignCustomDates(beginDate, lastDate, dates.data()['fromToday']);
  }

  static void activities(String data) {
    Map<String, List<Activity>> map = new Map();
    MyApp.activities.clear();

    final parsed = json.decode(data);

    for (String key in parsed.keys.toList()) {
      for (var item in parsed[key]) {
        if (map[key] == null) {
          map[key] = [];
        }
        String timespent = item['TimeSpent'];

        map[key]?.add(new Activity(activityName: item['ActivityName'], date: DateFormat('dd.MM.yyyy').parse(key), timeSpent: int.parse(timespent.substring(0, timespent.length - 7).trim())));
      }
    }

    MyApp.activities.addAll(map);
  }

  static List<Activity> getActivityPerWeek(DateTime dateTime) {
    return _getActivityRange(dateTime, dateTime.subtract(new Duration(days: dateTime.weekday.toInt())));
  }

  static List<Activity> getActivityPerMonth(DateTime dateTime) {
    return _getActivityRange(dateTime, dateTime.subtract(new Duration(days: dateTime.day.toInt())));
  }

  static List<Activity> getActivityTotal(DateTime dateTime) {
    var lastDate = _getLatestDate(MyApp.activities.keys.toList());
    return _getActivityRange(DateTime.now(), lastDate.subtract(new Duration(days: 1)));
  }

  // For a Single Activity
  static List<Activity> getAllActivitiesOf(String activityname) {
    List<Activity> list = [];

    DateTime start = DateTime.now();
    DateTime end = _getLatestDate(MyApp.activities.keys.toList()).subtract(new Duration(days: 1));

    while (start.difference(end).inDays != 0) {
      var date = dateTimeToActivityDate(start);

      if (MyApp.activities[date] != null) {
        for (var item in MyApp.activities[date]) {
          if (item.activityName == activityname) {
            list.add(item);
            break;
          }
        }
      }

      start = start.subtract(new Duration(days: 1));
    }

    list.sort((a, b) => b.date.compareTo(a.date));

    return list;
  }

  // Helpers
  static List<Activity> _getActivityRange(DateTime begin, DateTime end) {
    HashMap<String, String> hashTable = new HashMap<String, String>();
    Map<String, Activity> map = new Map();

    while (begin.difference(end).inDays != 0) {
      var date = dateTimeToActivityDate(begin);

      if (MyApp.activities[date] != null) {
        for (var i = 0; i < MyApp.activities[date].length; i++) {
          var ref = MyApp.activities[date][i];
          var activity = new Activity(activityName: ref.activityName, date: ref.date, timeSpent: ref.timeSpent);

          if (hashTable.containsKey(activity.activityName)) {
            map[hashTable[activity.activityName]].timeSpent += activity.timeSpent;
          } else {
            hashTable[activity.activityName] = activity.activityName;
            map[activity.activityName] = activity;
          }
        }
      }

      begin = begin.subtract(new Duration(days: 1));
    }

    var out = map.values.toList();
    out.sort((a, b) => b.timeSpent.compareTo(a.timeSpent));
    return out;
  }

  static DateTime _getLatestDate(List<String> datesAsString) {
    List<DateTime> dates = new List();

    for (var date in datesAsString) {
      dates.add(stringToActivityDateTime(date));
    }

    dates.sort((a, b) => a.compareTo(b));

    return dates.first;
  }

  static String dateTimeToActivityDate(DateTime date) => DateFormat('dd.MM.yyyy').format(date);
  static DateTime stringToActivityDateTime(String date) => DateFormat('dd.MM.yyyy').parse(date);
}

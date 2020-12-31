import 'package:flutter/cupertino.dart';

class InternetStatus {
  final String date;
  final int online;
  final int offline;
  final int total;

  InternetStatus({@required this.date, @required this.online, @required this.offline, @required this.total});
}

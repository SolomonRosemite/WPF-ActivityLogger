import 'package:flutter/material.dart';

class ActivityItem {
  final String name;
  List<String> names;
  final String imagePath;
  final Color backgroundColor;

  ActivityItem({@required this.name, @required this.backgroundColor}) : imagePath = "assets/images/" + name + ".png";
  ActivityItem.d({@required this.name, @required this.backgroundColor, @required this.names}) : imagePath = "assets/images/" + name + ".png";
}

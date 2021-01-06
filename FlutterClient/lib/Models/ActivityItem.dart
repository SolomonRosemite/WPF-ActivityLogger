import 'package:flutter/material.dart';

// Todo: The name should be an array of possible names and the fist one in the list
// Is the one that should be used.
class ActivityItem {
  final String name;
  // List<String> names;
  final String imagePath;
  final Color backgroundColor;

  ActivityItem({@required this.name, @required this.backgroundColor}) : imagePath = "assets/images/" + name + ".png";
  // ActivityItem({@required this.names, @required this.backgroundColor})
  //     : imagePath = names[0] + ".png",
  //       name = names[0];
}

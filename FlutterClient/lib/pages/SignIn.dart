import 'package:Activities/Backend/Backend.dart';
import 'package:Activities/Models/IUser.dart';
import 'package:Activities/services/HelperUtilityClass.dart';
import 'package:flutter/material.dart';
import 'package:permission_handler/permission_handler.dart';
import 'package:qrscan/qrscan.dart' as scanner;

class SignIn extends StatefulWidget {
  @override
  _SignInState createState() => _SignInState();
}

class _SignInState extends State<SignIn> {
  String access = "not set";

  void scan() async {
    if (await Permission.camera.request().isGranted && await Permission.storage.request().isGranted) {
      setState(() {
        access = "Granted";
      });

      String secret = await scanner.scan();

      // Todo:  Sign In and if successful setup the App. Else Tell the User the Secret is invalid.
      IUser user = await Backend.authenticate(secret);

      if (user == null) {
        // Todo: Handle
        return;
      }

      try {
        var res = await Backend.auth.signInWithEmailAndPassword(email: user.email, password: user.password);
        Backend.uid = res.user.uid;
      } catch (e) {
        // Todo: Handle
        return;
      }

      HelperUtilityClass.setupApp(context);
    }

    setState(() {
      access = "Denied";
    });
  }

  @override
  Widget build(BuildContext context) {
    return new WillPopScope(
      onWillPop: () async => false,
      child: Scaffold(
        // appBar: ,
        body: new Container(
          child: Center(
            child: Column(
              children: [
                Text(access),
                RaisedButton(
                  onPressed: scan,
                  child: Text("Scan Secret QR Code"),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}

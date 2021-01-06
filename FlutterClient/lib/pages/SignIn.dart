import 'package:Activities/Backend/Backend.dart';
import 'package:Activities/Models/IUser.dart';
import 'package:Activities/services/HelperUtilityClass.dart';
import 'package:flutter/material.dart';
import 'package:permission_handler/permission_handler.dart';
import 'package:qrscan/qrscan.dart' as scanner;
import 'package:Activities/pages/Home.dart';
import 'package:url_launcher/url_launcher.dart';

import '../Backend/Backend.dart';

class SignIn extends StatefulWidget {
  @override
  _SignInState createState() => _SignInState();
}

class _SignInState extends State<SignIn> {
  String userSecret = "";
  bool progressIndicatorActive = false;
  GlobalKey _dialogKey = GlobalKey();

  void scan() async {
    if (await Permission.camera.request().isGranted && await Permission.storage.request().isGranted) {
      String secret = await scanner.scan();

      if (secret == null) {
        return;
      }

      progressIndicatorActive = true;
      showLoadingDialog(context, _dialogKey);

      signUserIn(secret);
    } else {
      showAlertDialog("Insufficient Permissions", "Both Permissions are requied to use the app.");
    }
  }

  launchURL() async {
    const url = 'https://rosemitedocs.web.app/docs/WPF-ActivityLogger-Installation#mobile-installation';
    if (await canLaunch(url)) {
      await launch(url);
    } else {
      Backend.postReport("The Installation mobile-installation Url couldn't be launched");
      showAlertDialog("Somethink went wrong", "Please try again Later...");
    }
  }

  Future<void> signUserIn(String secret) async {
    IUser user = await Backend.authenticate(secret);

    if (user == null) {
      showAlertDialog("Invalid Secret", "The Scaned Secret doesn't seem to be valid. Please make sure to scan the correct QR-Code");
      return;
    }

    try {
      var res = await Backend.auth.signInWithEmailAndPassword(email: user.email, password: user.password);
      Backend.uid = res.user.uid;
    } catch (e) {
      showAlertDialog("Authentication Error", "The Authentication process failed. Try to Scan the Secret once again.");
      Backend.postReport("Authentication Error", exception: e);
      return;
    }

    await Backend.prefs.setString("userSecret", secret);
    await HelperUtilityClass.setupApp(context);

    progressIndicatorActive = false;
    Navigator.of(_dialogKey.currentContext, rootNavigator: true).pop();

    Navigator.push(
      context,
      MaterialPageRoute(builder: (context) => Home()),
    );
  }

  void showAlertDialog(String title, String errorMesage) {
    if (progressIndicatorActive) {
      Navigator.of(_dialogKey.currentContext, rootNavigator: true).pop();
      progressIndicatorActive = false;
    }
    showDialog(
      context: context,
      builder: (BuildContext context) {
        return AlertDialog(
          title: Text(title),
          content: Text(errorMesage),
          actions: [
            FlatButton(
              textColor: Colors.blue,
              onPressed: () {
                Navigator.pop(context);
              },
              child: Text('Ok'),
            ),
          ],
        );
      },
    );
  }

  showLoadingDialog(BuildContext context, GlobalKey _key) {
    return showDialog(
      context: context,
      barrierDismissible: false,
      builder: (BuildContext context) {
        return SimpleDialog(
          key: _key,
          children: [
            Center(
              child: Container(
                child: Row(
                  children: [
                    SizedBox(width: 20),
                    CircularProgressIndicator(
                      backgroundColor: Colors.blue[100],
                      valueColor: AlwaysStoppedAnimation(Colors.blue[800]),
                      strokeWidth: 4,
                    ),
                    SizedBox(
                      height: 10,
                      width: 10,
                    ),
                    Text("Please Wait"),
                  ],
                ),
              ),
            ),
          ],
        );
      },
    );
  }

  @override
  Widget build(BuildContext context) {
    return new WillPopScope(
      onWillPop: () async => false,
      child: Scaffold(
        appBar: AppBar(
          leading: new SizedBox.shrink(),
          title: Text('Sign in'),
          centerTitle: true,
          backgroundColor: Colors.blue,
        ),
        body: new Container(
          alignment: Alignment.center,
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.center,
            children: [
              Spacer(flex: 10),
              Image.asset(
                "assets/icons/checked.png",
                height: 110.0,
                width: 110.0,
              ),
              Spacer(flex: 8),
              Text(
                'Welcome to My Activities',
                style: TextStyle(fontWeight: FontWeight.bold, fontSize: 20),
              ),
              Text('View your Activites anywhere you go!'),
              Spacer(flex: 10),
              Text('To use My Activites on your moible device:\n'),
              Text('1. Open My Activites on your desktop device'),
              Text('2. Navigate to the Settings tab'),
              Text('3. Scan the QR-Code'),
              Spacer(flex: 10),
              Center(
                child: RaisedButton(
                  onPressed: scan,
                  color: Colors.blue,
                  child: Text(
                    "Scan Secret QR Code",
                    style: TextStyle(color: Colors.white),
                  ),
                ),
              ),
              Spacer(flex: 8),
              Center(
                child: FlatButton(
                  onPressed: () => {
                    launchURL()
                  },
                  child: Text(
                    "Need a guide?",
                    style: TextStyle(color: Colors.green),
                  ),
                ),
              ),
              Spacer(flex: 20),
            ],
          ),
        ),
      ),
    );
  }
}

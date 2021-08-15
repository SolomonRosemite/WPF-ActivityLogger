import 'package:activities/Backend/Backend.dart';
import 'package:activities/Models/IUser.dart';
import 'package:activities/services/HelperUtilityClass.dart';
import 'package:flutter/material.dart';
import 'package:permission_handler/permission_handler.dart';
import 'package:qrscan/qrscan.dart' as scanner;
import 'package:activities/pages/Home.dart';
import 'package:url_launcher/url_launcher.dart';

import 'package:flutter/foundation.dart' show kIsWeb;

import '../Backend/Backend.dart';

class SignIn extends StatefulWidget {
  @override
  _SignInState createState() => _SignInState();
}

class _SignInState extends State<SignIn> {
  String userSecret = "";
  bool progressIndicatorActive = false;
  GlobalKey _dialogKey = GlobalKey();

  final double imageSize = 110;

  void scanOrSubmit(BuildContext ctx) {
    if (!kIsWeb) {
      scan(ctx);
      return;
    }

    progressIndicatorActive = true;
    showLoadingDialog(context, _dialogKey);

    signUserIn(userSecret, ctx);
  }

  void scan(BuildContext ctx) async {
    if (await Permission.camera.request().isGranted &&
        await Permission.storage.request().isGranted) {
      String secret = await scanner.scan();

      if (secret == null) {
        return;
      }

      progressIndicatorActive = true;
      showLoadingDialog(context, _dialogKey);

      signUserIn(secret, ctx);
    } else {
      showAlertDialog("Insufficient Permissions",
          "Both Permissions are requied to use the app.", ctx);
    }
  }

  launchURL(BuildContext ctx) async {
    const url =
        'https://rosemitedocs.web.app/docs/WPF-ActivityLogger-Installation#mobile-installation';
    if (await canLaunch(url)) {
      await launch(url);
    } else {
      Backend.postReport(
          "The Installation mobile-installation Url couldn't be launched");
      showAlertDialog("Somethink went wrong", "Please try again Later...", ctx);
    }
  }

  Future<void> signUserIn(String secret, BuildContext ctx) async {
    IUser user = await Backend.authenticate(secret);

    if (user == null) {
      showAlertDialog(
        "Invalid Secret",
        "The Secret doesn't seem to be valid. Please make sure to scan (or on web type) the correct QR-Code or (Secret)",
        ctx,
      );
      return;
    }

    try {
      var res = await Backend.auth.signInWithEmailAndPassword(
        email: user.email,
        password: user.password,
      );
      Backend.uid = res.user.uid;
    } catch (e) {
      showAlertDialog(
        "Authentication Error",
        "The Authentication process failed. Try please try again later.",
        ctx,
      );
      Backend.postReport("Authentication Error", exception: e);
      return;
    }

    await Backend.prefs.setString("userSecret", secret);
    await HelperUtilityClass.setupApp(ctx);

    progressIndicatorActive = false;
    Navigator.of(_dialogKey.currentContext, rootNavigator: true).pop();

    Navigator.push(
      context,
      MaterialPageRoute(builder: (ctx) => Home()),
    );
  }

  void handleSecretInputChange(String secret) {
    this.userSecret = secret;
  }

  void showAlertDialog(String title, String errorMesage, BuildContext ctx) {
    if (progressIndicatorActive) {
      Navigator.of(_dialogKey.currentContext, rootNavigator: true).pop();
      progressIndicatorActive = false;
    }
    showDialog(
      context: ctx,
      builder: (BuildContext ctx2) {
        return AlertDialog(
          title: Text(title),
          content: Text(errorMesage),
          actions: [
            FlatButton(
              textColor: Colors.blue,
              onPressed: () {
                Navigator.pop(ctx2);
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

  inputWiget() {
    if (kIsWeb) {
      return Container(
        width: imageSize * 3,
        child: TextField(
          onChanged: handleSecretInputChange,
          textAlign: TextAlign.center,
          decoration: InputDecoration(
              border: OutlineInputBorder(), hintText: 'Enter your secret here'),
        ),
      );
    }

    return SizedBox.shrink();
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
                height: imageSize,
                width: imageSize,
              ),
              Spacer(flex: 8),
              Text(
                'Welcome to My Activities',
                style: TextStyle(fontWeight: FontWeight.bold, fontSize: 20),
              ),
              Text('View your Activites now anywhere you go!'),
              Spacer(flex: 10),
              Text('To use My Activites on your moible device or Web:\n'),
              Text('1. Open My Activites on your desktop device'),
              Text('2. Navigate to the Settings tab'),
              Text(kIsWeb
                  ? '3. Click on Reveal secret and paste it below'
                  : '3. Scan the QR-Code'),
              Spacer(flex: 10),
              inputWiget(),
              Spacer(flex: 5),
              Center(
                child: RaisedButton(
                  onPressed: () => scanOrSubmit(context),
                  color: Colors.blue,
                  child: Text(
                    kIsWeb ? "Sumbit secret" : "Scan Secret QR Code",
                    style: TextStyle(color: Colors.white),
                  ),
                ),
              ),
              Spacer(flex: 8),
              Center(
                child: FlatButton(
                  onPressed: () => {launchURL(context)},
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

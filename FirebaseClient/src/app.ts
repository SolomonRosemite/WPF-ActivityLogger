import {
  Collections,
  IConfig,
  IUpdateResult,
  IUserAuth,
  path,
  pathToConfig,
  url,
} from "./common/constants";
import * as fs from "fs";

import axios from "axios";

import { firebaseConfig } from "./credentials";
import firebase from "firebase/app";

// Firebase services
import "firebase/auth";
import "firebase/firestore";
import "firebase/storage";

const project = firebase.initializeApp(firebaseConfig);

const auth = project.auth();
const firestore = project.firestore();
const storage = project.storage();

async function main() {
  const userAuth = await getUser(getUserSecret(pathToConfig), pathToConfig);

  if (!userAuth) {
    // Todo: Guess what... report another Error
    return;
  }

  const result = await authenticate(userAuth).catch((err) => {
    reportError({
      message: err,
    });
  });

  if (!result || result.user === null) {
    // Todo: Report Error: "User Authentication Failed, when it shouldn't"
    return;
  }

  const user = result.user;

  const docRef = firestore.collection(Collections.Users).doc(user.uid);

  let firstRun = true;

  docRef.onSnapshot((item) => {
    if (firstRun) {
      repeat(docRef, path, user.uid);
      firstRun = false;
      return;
    }

    let state: string = item.data()!.action;

    switch (state) {
      // Ignore this states
      case "waiting":
      case "updated":
        break;

      // Upload latest Activities json
      case "updateActivityFile":
        uploadActivity(new Date(), path, user.uid).then((res) => {
          if (res.successful) {
            docRef.set({
              action: "updated",
            });
          } else {
            // TODO:reportError()
          }
        });
        break;

      // If State is Unexpected
      default:
        reportError({
          message: `The action: ${state} is Invalid.`,
        });
        break;
    }
  });
}

// Upload the updated Activity
async function uploadActivity(
  today: Date,
  pathToFile: string,
  uid: string
): Promise<IUpdateResult> {
  try {
    var day = String(today.getDate()).padStart(2, "0");
    var month = String(today.getMonth() + 1).padStart(2, "0");
    var year = today.getFullYear();

    const destination = `${uid}/${day}.${month}.${year}/SavedActivities.json`;

    const file = fs.readFileSync(pathToFile);

    await storage.ref(destination).put(file);
  } catch (err) {
    return {
      successful: false,
      err: err,
    };
  }

  return {
    successful: true,
    err: undefined,
  };
}

// Uploads every 5 Minutes the updated Activities.
async function repeat(docRef: any, path: string, uid: string) {
  const res = await uploadActivity(new Date(), path, uid);

  if (res.successful == true) {
    docRef.set({
      action: "updated",
    });
  } else {
    // Todo: Report Error
  }

  delay(1000 * 300).then(() => repeat(docRef, path, uid));
}

function delay(ms: number) {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

// Todo: On Error report to server. (If client has internet)
function reportError(error: { message: string | any }) {
  // Todo: Save with a date
  console.log(error);
}

// Acquires User Secret
function getUserSecret(pathToConfig: string): string | undefined {
  try {
    const config: IConfig = require(pathToConfig);
    if (config && config.userSecret) {
      return config.userSecret;
    }
  } catch (err) {
    // Todo: Err
  }

  return undefined;
}

// Gets a User by the secret
async function getUser(
  secret: string | undefined,
  pathToConfig: string
): Promise<IUserAuth | undefined> {
  if (!secret) {
    const body = { secret: null };

    const response = await axios.post(`${url}/auth`, body);

    const { uuid, email, password } = await response.data.user;

    let config: any = {};

    try {
      config = require(pathToConfig);
    } catch {}

    config.userSecret = uuid;

    fs.writeFileSync(pathToConfig, JSON.stringify(config, undefined, 2));

    return { email, password };
  }

  const body = { secret: secret };

  const response = await axios.post(`${url}/auth`, body);

  if (response.data.error) {
    return await getUser(undefined, pathToConfig);
  } else if (response.status === 500) {
    // TODO: Handle...
    return;
  }

  const { email, password } = response.data.user;

  return { email, password };
}

function signInWithEmail(user: IUserAuth) {
  return auth.signInWithEmailAndPassword(user.email, user.password);
}

async function authenticate(
  user: IUserAuth
): Promise<firebase.auth.UserCredential> {
  return await signInWithEmail(user);
}

// Start App
try {
  main();
} catch (err) {
  reportError({
    message: err,
  });
}

global.XMLHttpRequest = require("xhr2");

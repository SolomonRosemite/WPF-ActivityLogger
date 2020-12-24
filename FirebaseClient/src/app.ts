import {
  Collections,
  IConfig,
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

  console.log(userAuth);

  const user = await authenticate(userAuth)
  .catch((err) => {
    console.log(err)
  });

  console.log(user)
  return;

  // const docRef = firestore.collection("actions").doc("SavedActivities");

  const docRef = firestore
    .collection(Collections.Users)
    .doc("ztP60H1v3xesHO6PMM2j");

  let firstRun = true;

  docRef.onSnapshot((item) => {
    if (firstRun) {
      repeat(docRef, path);
      firstRun = false;
      return;
    }

    let value: string = item.data()!.action;

    switch (value) {
      case "waiting":
      case "updated":
        break;
      case "updateActivityFile":
        uploadActivity(new Date(), path).then(() => {
          docRef.set({
            action: "updated",
          });
        });
        break;
      default:
        if (value.length === 0) {
          break;
        }
        reportError({
          error: `The action: ${value} is Invalid.`,
        });
        break;
    }
  });
}

// Upload the updated Activity
async function uploadActivity(
  today: Date,
  pathToFile: string
): Promise<boolean> {
  // Todo: Fix me
  //   try {
  //     // var bucket = storage.bucket();

  //     var day = String(today.getDate()).padStart(2, "0");
  //     var month = String(today.getMonth() + 1).padStart(2, "0");
  //     var year = today.getFullYear();

  //     await bucket.upload(pathToFile, {
  //       destination: `${day}.${month}.${year}/SavedActivities.json`,
  //     });
  //     return true;
  //   } catch (err) {
  //     return false;
  //   }
  return true;
}

// Uploads every 5 Minutes the updated Activities.
async function repeat(docRef: any, path: string) {
  const success = await uploadActivity(new Date(), path);

  if (success == true) {
    docRef.set({
      action: "updated",
    });
  }

  delay(1000 * 300).then(() => repeat(docRef, path));
}

function delay(ms: number) {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

// Todo: On Error report to server. (If client has internet)
function reportError(err: any) {}

// Acquires User Secret
function getUserSecret(pathToConfig: string): string | undefined {
  try {
    const config: IConfig = require(pathToConfig);
    if (config && config.userSecret) {
      return config.userSecret;
    }
  } catch {}

  return undefined;
}

// Gets a User by the secret
async function getUser(
  secret: string | undefined,
  pathToConfig: string
): Promise<IUserAuth> {
  if (!secret) {
    const body = { secret: null };

    const response = await axios.post(`${url}/auth`, body);

    const { uuid, email, password } = await response.data.user;

    let config: any = {};

    try {
      config = require(pathToConfig);
    } catch {  }

    config.userSecret = uuid;

    fs.writeFileSync(pathToConfig, JSON.stringify(config, undefined, 2));

    return { email, password };
  }

  const body = { secret: secret };

  const response = await axios.post(`${url}/auth`, body);

  if (response.data.error) {
    return await getUser(undefined, pathToConfig);
  } else if(response.status === 500) {
    // TODO: Handle...
    return
  }

  const { email, password } = response.data.user;

  return { email, password };
}


function signInWithEmail(user: IUserAuth) {
  console.log(user)
  return auth.signInWithEmailAndPassword(user.email, user.password);
}

async function authenticate(user: IUserAuth): Promise<firebase.auth.UserCredential> {
    return await signInWithEmail(user)
}

// Start App
try {
  main();
} catch (err) {
  reportError(err);
}

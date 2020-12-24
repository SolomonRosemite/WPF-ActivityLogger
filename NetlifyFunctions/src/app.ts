const serverless = require("serverless-http");

import { createNewUser, IUser, randomString, uuidExists } from './common';
import * as bodyParser from "body-parser";
import * as express from "express";

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

const appname = "app";

const app = express();
const router = express.Router();

const users: IUser[] = [];

app.use(bodyParser.json());
app.use(`/.netlify/functions/${appname}`, router);

router.get("/", (req, res) => {
  res.json({
     message: "Hi"
   });
});

router.post("/auth", async (req, res) => {
  const { secret } = req.body;

  if(secret === undefined || secret === null) {
    const id = createUUID(randomString(20), users);
    const user = createNewUser(id);
    users.push(user);

    const result = await signUpUser(user).catch((err) => {
      console.log(err)
      // TODO: Report Error
    })

    if (!result || !result.user) {
      // The Email either already in use or something else went wrong
      res.status(500).json({
        message: "Something when wrong. Try to regenerate a new secret"
      })
      return;
    }

    await firestore.doc("/users/" + result.user.uid).set({
      action: "waiting"
    })

    res.json({
      user: user
    });
    return;
  }

  const user = uuidExists(secret, users)

  if (!user) {
    res.json({
      error: "User with this Secret does not exist."
    });
    return;
  }

  res.json({
    user: user
  });
})

function createUUID(id: string, list: IUser[]): string {
  const res = uuidExists(id, list)
  if (res) {
    return createUUID(randomString(20), list);
  }

  return id;
}

function signUpUser(user: IUser) {
  return auth.createUserWithEmailAndPassword(user.email, user.password);
}

module.exports.handler = serverless(app);
export { app };

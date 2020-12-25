const serverless = require("serverless-http");
require("dotenv").config();

import { createNewUser, IUser, randomString, uuidExists } from "./common";
import * as bodyParser from "body-parser";
import * as express from "express";
//
// const json = JSON.parse(process.env.cleanJson as any);
// const serviceAccount = JSON.parse(process.env.serviceAccount as any);
import * as admin from "firebase-admin";

const serviceAccount = {
  type: "service_account",
  project_id: "rosemite-activities",
  private_key_id: "d170c9df2a1893a64e501ada89dfb12de1478276",
  private_key:
    "-----BEGIN PRIVATE KEY-----\nMIIEvgIBADANBgkqhkiG9w0BAQEFAASCBKgwggSkAgEAAoIBAQCw06vvkoRPlMuz\nqjHRosa/aCj3HChE2U0jzQj7UkVRard7D2kMaFkFOqEU14XGC5IKjcO+2J4o6p4S\nzoQkOF6AGKK6QpfPmhgKLc1igd5r3a6s6cvdbPkG/q8c7eOys6yPzVA6fB1qWpK5\nlfaEoETmlYjVFMmR2uDnVlu/GYvyrOZuIxlVf8MI84S6foBTtDDY+TQsgvgqwaSa\nlc9UqD9lTteJL3X/0Rrtrf8QrQYKwNW3g2hwYRwNbIIyN0IrRmZ8RlyPSkQWYEPo\nrRn0aiOj7Oxq028Ln2LwE7ICUZkXVClb1fgIwsqDGRRJT1sPp8E4dvMEDw9F6f/G\ns6kc3S9dAgMBAAECggEAFEo1kGffhsIsHZGJ5eQnqg7ppHE2ra/Bijocrbni+lSk\neRprk2Dl1hPEKAAS+YaaW1uo+l0gyNEjEkVgJEfTkB9DjNvXT5r+5ywaRRNh0j69\nrFmnauD2MjdHIKqrVfYH9vg4HH3hYjLCY0kx4EYDofHGoayg9HuJEwJ1xJfJ1y6O\nxgsfbrB+GEckWDPR+QYesVNUnBroifUFQ6J9tDsJG1PJmA+OMtIsNtPk9HeB8OZf\nK4BgEHfJaWD+RMeyBIqfgmgmVo/pq4kCLZF6RCxH+5jq+rtXqyJ9y/qlop/DRsGS\np/dlJYAklwKCoQ95HL1pj8BvonhTMOt6WdYA7yCkAQKBgQDmmMEMPGW88tO/8qji\nLOnIeUdGDvM0QEMr31+nN3e5OygFVXu/0Zv+Lo7nb7x2O0Dt60vTKFt4dQxMYG9h\n1xF4UCocowhEM8XfxqMN0jyAay8WEryLIAaFdYj19t+d/BAVidZBAD3GEWNDqqWT\nlXf/crkndJhpLHhCU2NxbLoVmQKBgQDEToKLrLZ05xpv1Sxrmog9Fy8BMtIO6pO9\nnHIGqOAAgFEjgDDGcKVZccXogGEXsc3K3amSh3t/m0opo9MVSoRQ3FOCavSln4Qj\nF5Dcjc1cJRo1ifwehnw82ZLZjOVAmo5g1xqAq75o93XCViYqMYWaG3XrDY73fJ8c\nC0Rpn746ZQKBgAHmaFDNfpkb9xqxySpz7Ek2lpvBK8fWb7dA0+zwzA++qQnCNo7S\nD1L81ziY9xiSTGqpcap/vqEzdbJ7aO+Jch9nqbEqtrq5InxBir5maRE10OGBrgQH\nc4ZN/Z7pNdZ7aTaEODu44MltA0Bfe5XHuYFlVJk6oLbdIvCI91zL5IyhAoGBAJFe\n4mW03NjtWzJ91otIwKMcy/5DODM7m3TKqdYssNUuMOftQjFxscDu8/QdPcliLleQ\nlsaf9eCliuITI3mc2SdYQa9OrSUJX7zs8qWhjPYzs5j6Oe4RAWe+Z3UbVZnl2pH0\nOPgYNsVcm6PyUAjm610YANa0D69OZjVKUrs1RJXRAoGBALRxZ9DnDYDFM4xV9FDH\n2biUX3VGn1Wv5Oc25p6SR0qjiJshjyBl0Z6TbfGVHWHuY4hZ5ztDwcTR3hp4eI7i\nMb8xeuW1vVgCYjqkgMKCDiNJg9OyXPPPz8lDK0jGepQewgAZekHQN9g47NOjy+Wm\nEWNUr/REkt1cTWKwMULWguBf\n-----END PRIVATE KEY-----\n",
  client_email:
    "firebase-adminsdk-9awn7@rosemite-activities.iam.gserviceaccount.com",
  client_id: "107060024742832293872",
  auth_uri: "https://accounts.google.com/o/oauth2/auth",
  token_uri: "https://oauth2.googleapis.com/token",
  auth_provider_x509_cert_url: "https://www.googleapis.com/oauth2/v1/certs",
  client_x509_cert_url:
    "https://www.googleapis.com/robot/v1/metadata/x509/firebase-adminsdk-9awn7%40rosemite-activities.iam.gserviceaccount.com",
} as any;

admin.initializeApp({
  credential: admin.credential.cert(serviceAccount),
  storageBucket: "rosemite-activities.appspot.com",
});

const auth = admin.auth();
const firestore = admin.firestore();
const storage = admin.storage();

const appname = "app";

const app = express();
const router = express.Router();

app.use(bodyParser.json());
app.use(`/.netlify/functions/${appname}`, router);

router.get("/", (req, res) => {
  res.json({
    message: "Hey",
  });
});

router.post("/auth", async (req, res) => {
  const { secret } = req.body;

  if (secret === undefined || secret === null) {
    const id = await createUUID(randomString(20));
    const user = createNewUser(id);

    const result = await signUpUser(user).catch((err) => {
      console.log(err);
      // TODO: Report Error
    });

    if (!result) {
      // The Email either already in use or something else went wrong
      res.status(500).json({
        message: "Something when wrong. Try to regenerate a new secret",
      });
      return;
    }

    await firestore.doc("/users/" + result.uid).set({
      action: "waiting",
    });

    await firestore.doc("/secrets/" + result.uid).set({
      secret: user.uuid,
      email: user.email,
      password: user.password,
      uid: result.uid,
    });

    res.json({
      user: user,
    });
    return;
  }

  const user = await uuidExists(secret, firestore);

  if (!user) {
    res.json({
      error: "User with this Secret does not exist.",
    });
    return;
  }

  res.json({
    user: {
      email: user.email,
      password: user.password,
    },
  });
});

async function createUUID(id: string): Promise<string> {
  const res = await uuidExists(id, firestore);
  if (res) {
    return await createUUID(randomString(20));
  }

  return id;
}

function signUpUser(user: IUser) {
  return auth.createUser({
    email: user.email,
    password: user.password,
  });
}

module.exports.handler = serverless(app);
export { app };

const serverless = require("serverless-http");

import {
  createNewUser,
  IUser,
  randomString,
  serviceAccountJson,
  uuidExists,
} from "./common";
import * as bodyParser from "body-parser";
import * as express from "express";
import * as cors from "cors";

const serviceAccount = JSON.parse(serviceAccountJson);
import * as admin from "firebase-admin";

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

app.use(cors());
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
      const data = {
        ...err,
        user: user,
        date: new Date(),
      };
      firestore.collection("/serverErrors").add({
        error: JSON.stringify(err),
      });
    });

    if (!result) {
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

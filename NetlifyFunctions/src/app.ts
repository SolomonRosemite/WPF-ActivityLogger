const serverless = require("serverless-http");

import * as bodyParser from "body-parser";
import * as express from "express";
import { createNewUser, IUser, randomString, uuidExists } from './common';

const appname = "app";

const app = express();
const router = express.Router();

const users: IUser[] = [];

app.use(bodyParser.json());
app.use(`/.netlify/functions/${appname}`, router);

router.get("/", (req, res) => {
  res.json({ message: "Hi" });
});

router.post("/auth", (req, res) => {
  const { secret } = req.body;

  if(secret === undefined || secret === null) {
    const id = createUUID(randomString(20), users);
    const user = createNewUser(id);
    users.push(user);


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

  res.json({user: user});
})

function createUUID(id: string, list: IUser[]): string {
  const res = uuidExists(id, list)

  if (res) {
    return createUUID(randomString(20), list);
  }

  return id;
}

module.exports.handler = serverless(app);
export { app };

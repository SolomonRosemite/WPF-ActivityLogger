const url =
  process.env.NODE_ENV === "production"
    ? "https://wpf-activitylogger-functions.netlify.app/.netlify/functions/app"
    : "http://localhost:3000/.netlify/functions/app";

const homedir = require("os").homedir() + "\\TMRosemite\\ActivityLogger\\";
const path = homedir + "SavedActivities.json";
const pathToConfig = homedir + "Config.json";

enum Collections {
  Users = "users",
}

interface IUpdateResult {
  successful: boolean;
  err: any;
}

interface IConfig {
  UserSecret: string;
}

interface IUserAuth {
  email: string;
  password: string;
}

export {
  Collections,
  IUserAuth,
  IConfig,
  url,
  path,
  pathToConfig,
  IUpdateResult,
};

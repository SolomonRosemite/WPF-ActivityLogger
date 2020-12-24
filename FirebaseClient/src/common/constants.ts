// Todo: Add production url
const url =
  process.env.NODE_ENV === "production"
    ? ""
    : "http://localhost:3000/.netlify/functions/app";

const homedir = require("os").homedir() + "\\TMRosemite\\ActivityLogger\\";
const path = homedir + "SavedActivities.json";
const pathToConfig = homedir + "Config.json";

enum Collections {
  Users = "users",
}

interface IConfig {
  userSecret: string;
}

interface IUserAuth {
  email: string;
  password: string;
}

export { Collections, IUserAuth, IConfig, url, path, pathToConfig };

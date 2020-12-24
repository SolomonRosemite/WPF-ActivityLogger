interface IUser {
  uuid: string;
  email: string;
  password: string;
}

function uuidExists(uuid: string, users: IUser[]): IUser | undefined {
  const results = users.filter(u => u.uuid == uuid);

  if (results.length === 0) {
    return undefined;
  }

  return results[0];
}

function createNewUser(id: string): IUser {
  const email = `${id}@${randomString(10)}.com`
  const password = randomString(12);

  return {
    email: email,
    password: password,
    uuid: id
  }
}

function randomString(length: number) {
   let result = '';
   const characters = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
   const charactersLength = characters.length;

   for ( var i = 0; i < length; i++ ) {
      result += characters.charAt(Math.floor(Math.random() * charactersLength));
   }

   return result;
}

export { IUser, createNewUser, uuidExists, randomString }

interface IUser {
  uuid: string;
  email: string;
  password: string;
}

async function uuidExists(uuid: string, firestore: FirebaseFirestore.Firestore): Promise<IUser | undefined> {
  const results = await firestore.collection("/secrets")
                    .where("secret", "==", uuid)
                    .get()
                    .catch((err) => {
                      console.log(err)
                    });


  if (results && !results.empty) {
    return results.docs[0].data() as IUser;
  }

  return undefined;
}

function createNewUser(id: string): IUser {
  const email = `${id}@${randomString(10)}.com`
  const password = randomString(20);

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

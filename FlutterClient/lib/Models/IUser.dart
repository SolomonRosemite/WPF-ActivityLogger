class IUser {
  final String email;
  final String password;

  IUser({this.email, this.password});

  factory IUser.fromJson(Map<String, dynamic> json) {
    return IUser(
      email: json['user']['email'],
      password: json['user']['password'],
    );
  }
}

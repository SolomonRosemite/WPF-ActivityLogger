#!/bin/sh
mkdir ./flutter-sdk && cd ./flutter-sdk
git clone https://github.com/flutter/flutter.git

export PATH="$PATH:`pwd`/flutter/bin"

cd ../

ls

flutter build web --release
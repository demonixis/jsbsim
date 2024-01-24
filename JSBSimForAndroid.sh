#!/bin/bash

rm -rf build
mkdir -p build
cd build

cmake .. \
    -DBUILD_SHARED_LIBS=ON \
    -DCMAKE_C_COMPILER=clang \
    -DCMAKE_CXX_COMPILER=clang++ \
    -DCMAKE_TOOLCHAIN_FILE=/Users/yann/Library/Android/sdk/ndk/25.1.8937393/build/cmake/android.toolchain.cmake \
    -DANDROID_ABI=arm64-v8a \
    -DANDROID_PLATFORM=android-29

make -j8
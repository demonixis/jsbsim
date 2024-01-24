#!/bin/bash

rm -rf build
mkdir -p build
cd build

cmake .. \
    -B_builds -GXcode \
    -DBUILD_SHARED_LIBS=ON \
    -DCMAKE_SYSTEM_NAME=iOS \
    #-DCMAKE_C_COMPILER=clang -DCMAKE_CXX_COMPILER=clang++ \
    #-DCMAKE_CXX_FLAGS="-stdlib=libc++" \
    "-DCMAKE_OSX_ARCHITECTURES=arm64;x86_64" \
    -DCMAKE_OSX_DEPLOYMENT_TARGET=17 \
    -DCMAKE_INSTALL_PREFIX=`pwd`/_install \
    -DCMAKE_XCODE_ATTRIBUTE_ONLY_ACTIVE_ARCH=NO \
    -DCMAKE_IOS_INSTALL_COMBINED=YES
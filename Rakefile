# coding: utf-8
task :default do
  sh 'rake -T'
end


desc "lib win 64"
task :lib_win_64 do
  # https://github.com/ValveSoftware/GameNetworkingSockets/blob/4ea37fb89b41d769c2f12178c9e3a2e1321b024b/BUILDING.md#msys2

  #======== Install OpenSSL
  # https://slproweb.com/products/Win32OpenSSL.html
  # Win64 OpenSSL v1.1.1c # C:\OpenSSL-Win64 - check The OpenSSL binaries (/bin) directory

  #======== Install ninja
  # scoop install ninja

  #======== Install CMake
  # https://cmake.org/download/

  #> dumpbin /dependents GameNetworkingSockets.dll
  #Microsoft (R) COFF/PE Dumper Version 14.16.27027.1
  #Copyright (C) Microsoft Corporation.  All rights reserved.
  #
  #
  #Dump of file GameNetworkingSockets.dll
  #
  #File Type: DLL
  #
  #  Image has the following dependencies:
  #
  #    libprotobuf.dll
  #    libcrypto-1_1-x64.dll
  #    WS2_32.dll
  #    KERNEL32.dll
  #    MSVCP140D.dll
  #    VCRUNTIME140D.dll
  #    ucrtbased.dll

  if not ENV['DevEnvDir']
    puts 'Before run this task, execute follow command: "C:\\Program Files (x86)\\Microsoft Visual Studio\\2017\\Community\\Common7\\Tools\\vsdevcmd" -arch=x64'
    exit 1
  end

  build_dir = 'build/win_64'
  lib_dir = 'lib/win_64'
  FileUtils.mkdir_p(build_dir) unless File.directory?(build_dir)
  FileUtils.mkdir_p(lib_dir) unless File.directory?(lib_dir)
  dst_libgamenetworkingsockets = File.join(`pwd`.strip, lib_dir, 'GameNetworkingSockets.dll')
  dst_libcrypto = File.join(`pwd`.strip, lib_dir, 'libcrypto-1_1-x64.dll')
  dst_libprotobuf = File.join(`pwd`.strip, lib_dir, 'libprotobuf.dll')

  Dir.chdir(build_dir) do
    protobuf_release_dir = "#{`pwd`.strip}\\protobuf-amd64"
    sh 'git clone https://github.com/google/protobuf'
    Dir.chdir('protobuf') do
      sh 'git checkout -t origin/3.5.x'
      sh 'mkdir cmake_build'
      Dir.chdir('cmake_build') do
        sh "cmake -G Ninja -DCMAKE_BUILD_TYPE=Release -Dprotobuf_BUILD_TESTS=OFF -Dprotobuf_BUILD_SHARED_LIBS=ON -DCMAKE_INSTALL_PREFIX=#{protobuf_release_dir} ..\\cmake"
        sh 'ninja'
        sh 'ninja install'
      end
    end

    sh 'git clone https://github.com/ValveSoftware/GameNetworkingSockets.git'
    Dir.chdir('GameNetworkingSockets') do
      ENV['PATH'] = "#{protobuf_release_dir}\\bin;#{ENV['PATH']}"
      sh 'mkdir build'
      Dir.chdir('build') do
        sh 'cmake -G Ninja ..'
        sh 'ninja'
      end
    end

    sh "cp GameNetworkingSockets/build/src/GameNetworkingSockets.dll #{dst_libgamenetworkingsockets}"
    sh "cp C:\\OpenSSL-Win64\\bin\\libcrypto-1_1-x64.dll #{dst_libcrypto}"
    sh "cp #{protobuf_release_dir}\\bin\\libprotobuf.dll #{dst_libprotobuf}"
  end
end


desc 'lib macOs 64'
task :lib_mac_64 do
  # $ otool -L libGameNetworkingSockets.dylib
  # libGameNetworkingSockets.dylib:
  #	@rpath/libGameNetworkingSockets.dylib (compatibility version 0.0.0, current version 0.0.0)
  #	/usr/local/opt/openssl@1.1/lib/libcrypto.1.1.dylib (compatibility version 1.1.0, current version 1.1.0)
  #	/usr/local/opt/protobuf/lib/libprotobuf.18.dylib (compatibility version 19.0.0, current version 19.1.0)
  #	/usr/lib/libc++.1.dylib (compatibility version 1.0.0, current version 400.9.4)
  #	/usr/lib/libSystem.B.dylib (compatibility version 1.0.0, current version 1252.250.1)

  build_dir = 'build/mac_64'
  lib_dir = 'lib/mac_64'
  FileUtils.mkdir_p(build_dir) unless File.directory?(build_dir)
  FileUtils.mkdir_p(lib_dir) unless File.directory?(lib_dir)
  # dst_static_libgamenetworkingsockets = File.join(`pwd`.strip, lib_dir, 'libGameNetworkingSockets.a')
  dst_dynamic_libgamenetworkingsockets = File.join(`pwd`.strip, lib_dir, 'libGameNetworkingSockets.dylib')
  dst_libcrypto = File.join(`pwd`.strip, lib_dir, 'libcrypto.1.1.dylib')
  dst_libprotobuf = File.join(`pwd`.strip, lib_dir, 'libprotobuf.18.dylib')

  Dir.chdir(build_dir) do
    sh 'brew install openssl@1.1'
    sh 'brew install protobuf'
    sh 'brew install ninja'
    sh 'brew install meson'
    ENV['PKG_CONFIG_PATH'] = "/usr/local/opt/openssl@1.1/lib/pkgconfig:#{ENV['PKG_CONFIG_PATH']}"

    sh 'git clone https://github.com/ValveSoftware/GameNetworkingSockets.git'
    Dir.chdir('GameNetworkingSockets') do
      sh 'meson . build'
      sh 'ninja -C build'
    end

    # cp 'GameNetworkingSockets/build/src/libGameNetworkingSockets.a', dst_static_libgamenetworkingsockets
    cp 'GameNetworkingSockets/build/src/libGameNetworkingSockets.dylib', dst_dynamic_libgamenetworkingsockets
    cp '/usr/local/opt/openssl@1.1/lib/libcrypto.1.1.dylib', dst_libcrypto
    cp '/usr/local/opt/protobuf/lib/libprotobuf.18.dylib', dst_libprotobuf

    puts `otool -L GameNetworkingSockets/build/src/libGameNetworkingSockets.dylib`
  end
end

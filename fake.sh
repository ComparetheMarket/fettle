#!/bin/sh

error() {
  echo ">>>>>> Failed to build <<<<<<<<<"
  echo ""

  exit 1
}

trap error ERR


CURDIR=`pwd`

docker run --rm \
           -v "$CURDIR/:/fettle" \
           --workdir /fettle \
           mono:5.4.1 mono ./.paket/paket.exe restore && \
                      mono ./tools/FAKE.4.61.2/tools/FAKE.exe "$@"
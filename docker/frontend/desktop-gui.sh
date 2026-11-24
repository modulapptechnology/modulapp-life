#!/bin/sh
# Runs the Tauri window on a virtual screen that is shown in the browser (noVNC, port 6080).
set -e

export DISPLAY=:99

Xvfb "$DISPLAY" -screen 0 1280x800x24 &
sleep 1
fluxbox >/dev/null 2>&1 &
x11vnc -display "$DISPLAY" -forever -shared -nopw -quiet -rfbport 5900 &
websockify --web /usr/share/novnc 6080 localhost:5900 &

exec dbus-run-session -- npm run desktop

for %%a in (*.mov) do "C:\Program Files (x86)\VideoLAN\VLC\vlc" -I dummy -vvv %%a --sout=#transcode{vcodec=h264,vb=700,acodec=aac,ab=64,channels=2}:standard{access=file,mux=mp4,dst=%%~a.mp4} vlc://quit
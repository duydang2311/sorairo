# sorairo

A minimal, local, cross-platform music player built with Avalonia and miniaudio.

[Sorairo Overview](docs/demo.gif)

## Roadmap

I've pulled the first usable cross-platform version recently. And there are lots of work to be done still.

- [x] Responsive GUI with Avalonia
- [x] Cross-platform playback with miniaudio
- [x] File metadata read (ATL.NET)
- [ ] File metadata write utilities (ATL.NET)
- [ ] Playback modes: shuffle, repeat.
- [ ] Adding sources from YouTube (yt-dlp on-demand)
- [ ] Basic audio visualization
- [ ] Discord RPC option
- [ ] Drag and drop songs
- [ ] Release packaging (Velopack)

## Performance

Ran on my win-x64 with the following results:

| Metric                   | Value   |
|:-------------------------|:--------|
| Startup time             | ~1s     |
| Memory usage (startup)   | ~35 MB  |
| Memory usage (20 songs)  | ~43 MB  |
| Memory usage (playback)  | ~60 MB  |
| CPU usage (idle)         | ~0 %    |
| CPU usage (playback)     | ~0.8 %  |

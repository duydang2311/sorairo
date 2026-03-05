use std::{
    fs::File,
    io::{Read, Seek},
    num::{NonZero, NonZeroU32},
    path::Path,
    sync::{
        Arc,
        atomic::{AtomicU64, Ordering},
    },
    time::Duration,
};

use lofty::{file::TaggedFileExt, read_from_path, tag::ItemKey};
use rodio::{
    ChannelCount, Decoder, MixerDeviceSink, Player, SampleRate, Source, source::SeekError,
};

#[derive(Default)]
pub struct SorairoPlayer {
    pub state: PlayerState,
    pub playlist: Vec<PlaylistItem>,
    current_index: Option<usize>,
}

pub struct PlaylistItem {
    pub path: String,
    pub file_name: String,
    pub artist: Option<String>,
    pub title: Option<String>,
}

pub enum PlayerState {
    Idle,
    Loaded(LoadedState),
}

pub struct LoadedState {
    pub sink: MixerDeviceSink,
    pub player: Player,
    pub sample_rate: SampleRate,
    pub channels: NonZero<u16>,
    pub progress: Arc<AtomicU64>,
    pub total_duration: Duration,
}

struct ProgressTracked<I> {
    inner: I,
    counter: Arc<AtomicU64>,
}

impl<I: Iterator> Iterator for ProgressTracked<I> {
    type Item = I::Item;
    fn next(&mut self) -> Option<Self::Item> {
        let n = self.inner.next();
        if n.is_some() {
            self.counter.fetch_add(1, Ordering::Relaxed);
        }
        n
    }
}

impl Default for PlayerState {
    fn default() -> Self {
        PlayerState::Idle
    }
}

impl<R> Source for ProgressTracked<Decoder<R>>
where
    R: Read + Seek,
{
    #[inline]
    fn current_span_len(&self) -> Option<usize> {
        self.inner.current_span_len()
    }

    #[inline]
    fn channels(&self) -> ChannelCount {
        self.inner.channels()
    }

    fn sample_rate(&self) -> SampleRate {
        self.inner.sample_rate()
    }

    #[inline]
    fn total_duration(&self) -> Option<Duration> {
        self.inner.total_duration()
    }

    // #[inline]
    fn try_seek(&mut self, pos: Duration) -> Result<(), SeekError> {
        self.inner.try_seek(pos)?;
        let sample_rate = u32::from(self.sample_rate()) as f64;
        let channels = u16::from(self.inner.channels()) as f64;
        let new_sample_pos = (pos.as_secs_f64() * sample_rate * channels) as u64;
        self.counter.store(new_sample_pos, Ordering::Relaxed);
        Ok(())
    }
}

impl SorairoPlayer {
    pub fn add_file(&mut self, path: String) {
        let tagged_file = read_from_path(&path).unwrap();
        let primary_tag = tagged_file.primary_tag();
        let file_name = Path::new(&path)
            .file_name()
            .and_then(|a| a.to_str())
            .expect("path must be valid")
            .to_owned();
        self.playlist.push(PlaylistItem {
            path,
            file_name,
            artist: primary_tag
                .and_then(|a| a.get_string(ItemKey::TrackArtist).map(|a| a.to_owned())),
            title: primary_tag
                .and_then(|a| a.get_string(ItemKey::TrackTitle).map(|a| a.to_owned())),
        });
    }

    pub fn set_current(&mut self, index: Option<usize>) {
        if let Some(i) = index {
            if i >= self.playlist.len() {
                return;
            }
        }
        self.current_index = index;
    }

    pub fn get_current(&self) -> Option<usize> {
        self.current_index
    }

    pub fn play(&mut self) {
        let Some(item) = self
            .current_index
            .and_then(|index| self.playlist.get(index))
        else {
            return;
        };
        let file = File::open(&item.path).unwrap();
        let tagged = read_from_path(&item.path).unwrap();
        if let Some(tag) = tagged.primary_tag() {
            let artist = tag.get_string(ItemKey::TrackArtist);
            let title = tag.get_string(ItemKey::TrackTitle);

            println!("Artist: {:?}", artist);
            println!("Title: {:?}", title);
        }
        let source = Decoder::try_from(file).unwrap();
        let progress = Arc::new(AtomicU64::new(0));
        let loaded = match std::mem::replace(&mut self.state, PlayerState::Idle) {
            PlayerState::Idle => {
                let sink = rodio::DeviceSinkBuilder::open_default_sink()
                    .expect("open default audio stream");
                let player = rodio::Player::connect_new(sink.mixer());
                LoadedState {
                    sink,
                    player,
                    sample_rate: source.sample_rate(),
                    channels: source.channels(),
                    progress: progress.clone(),
                    total_duration: source
                        .total_duration()
                        .expect("source must have total_duration"),
                }
            }
            PlayerState::Loaded(mut loaded) => {
                loaded.sample_rate = source.sample_rate();
                loaded.channels = source.channels();
                loaded.progress = progress.clone();
                loaded.total_duration = source
                    .total_duration()
                    .expect("source must have total_duration");
                loaded
            }
        };

        loaded.player.append(ProgressTracked {
            inner: source,
            counter: progress,
        });
        loaded.player.play();
        self.state = PlayerState::Loaded(loaded);
    }

    pub fn clear(&mut self) {
        self.playlist.clear();
        self.current_index = None;
        self.state = PlayerState::Idle;
    }

    pub fn get_elapsed_secs(&self) -> f64 {
        match &self.state {
            PlayerState::Idle => 0.0,
            PlayerState::Loaded(loaded) => {
                let played_samples = loaded.progress.load(Ordering::Relaxed);
                let elapsed = played_samples as f64
                    / (u32::from(loaded.sample_rate) as f64 * u16::from(loaded.channels) as f64);
                elapsed.min(loaded.total_duration.as_secs_f64())
            }
        }
    }

    pub fn get_total_secs(&self) -> f64 {
        match &self.state {
            PlayerState::Idle => 0.0,
            PlayerState::Loaded(loaded) => loaded.total_duration.as_secs_f64(),
        }
    }

    pub fn get_volume(&self) -> f32 {
        match &self.state {
            PlayerState::Idle => 0.0,
            PlayerState::Loaded(loaded) => loaded.player.volume(),
        }
    }

    pub fn resume(&self) {
        match &self.state {
            PlayerState::Idle => {}
            PlayerState::Loaded(loaded) => loaded.player.play(),
        }
    }

    pub fn stop(&self) {
        match &self.state {
            PlayerState::Idle => {}
            PlayerState::Loaded(loaded) => loaded.player.stop(),
        }
    }

    pub fn pause(&self) {
        match &self.state {
            PlayerState::Idle => {}
            PlayerState::Loaded(loaded) => loaded.player.pause(),
        }
    }

    pub fn seek(&self, secs: f64) {
        match &self.state {
            PlayerState::Idle => {}
            PlayerState::Loaded(loaded) => {
                _ = loaded
                    .player
                    .try_seek(Duration::from_secs_f64(secs))
                    .inspect_err(|e| {
                        eprintln!("{}", e);
                    });
            }
        }
    }

    pub fn empty(&self) -> bool {
        match &self.state {
            PlayerState::Idle => true,
            PlayerState::Loaded(loaded) => loaded.player.empty(),
        }
    }
}

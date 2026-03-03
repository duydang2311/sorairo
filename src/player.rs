use std::{
    fs::File,
    io::{Read, Seek},
    num::NonZero,
    sync::{
        Arc,
        atomic::{AtomicU64, Ordering},
    },
    time::Duration,
};

use rodio::{
    ChannelCount, Decoder, MixerDeviceSink, Player, SampleRate, Source, source::SeekError,
};

#[derive(Default)]
pub struct SorairoPlayer {
    state: PlayerState,
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
    pub fn get_state(&self) -> &PlayerState {
        &self.state
    }

    pub fn play(&mut self, path: &str) {
        let file = File::open(path).unwrap();
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
        match &self.state {
            PlayerState::Idle => {}
            PlayerState::Loaded(loaded) => {
                loaded.player.clear();
            }
        };
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
}

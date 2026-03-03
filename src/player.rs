use std::{
    fs::File,
    num::NonZero,
    sync::{
        Arc,
        atomic::{AtomicU64, Ordering},
    },
    time::Duration,
};

use rodio::{Decoder, MixerDeviceSink, Player, SampleRate, Source};

#[derive(Default)]
pub struct SorairoPlayer {
    state: PlayerState,
}

pub enum PlayerState {
    Idle,
    Ready(ReadyState),
    Playing(PlayingState),
}

pub struct ReadyState {
    sink: MixerDeviceSink,
    player: Player,
}

pub struct PlayingState {
    sink: MixerDeviceSink,
    player: Player,
    sample_rate: SampleRate,
    channels: NonZero<u16>,
    progress: Arc<AtomicU64>,
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
            // Every time a sample is played, increment the counter
            self.counter.fetch_add(1, Ordering::Relaxed);
        }
        n
    }
}

impl<I: Source> Source for ProgressTracked<I> {
    fn current_span_len(&self) -> Option<usize> {
        self.inner.current_span_len()
    }
    fn channels(&self) -> NonZero<u16> {
        self.inner.channels()
    }
    fn sample_rate(&self) -> SampleRate {
        self.inner.sample_rate()
    }
    fn total_duration(&self) -> Option<Duration> {
        self.inner.total_duration()
    }
}

impl Default for PlayerState {
    fn default() -> Self {
        PlayerState::Idle
    }
}

impl From<PlayingState> for ReadyState {
    fn from(playing: PlayingState) -> Self {
        ReadyState {
            sink: playing.sink,
            player: playing.player,
        }
    }
}

impl SorairoPlayer {
    pub fn get_state(&self) -> &PlayerState {
        &self.state
    }

    pub fn play(&mut self, path: &str) {
        let ready = match std::mem::replace(&mut self.state, PlayerState::Idle) {
            PlayerState::Idle => {
                let sink = rodio::DeviceSinkBuilder::open_default_sink()
                    .expect("open default audio stream");
                let player = rodio::Player::connect_new(sink.mixer());
                ReadyState { sink, player }
            }
            PlayerState::Ready(ready) => ready,
            PlayerState::Playing(playing) => {
                playing.player.clear();
                ReadyState {
                    sink: playing.sink,
                    player: playing.player,
                }
            }
        };
        let file = File::open(path).unwrap();
        let source = Decoder::try_from(file).unwrap();

        let progress = Arc::new(AtomicU64::new(0));

        let playing = PlayingState {
            sample_rate: source.sample_rate(),
            channels: source.channels(),
            progress: progress.clone(),
            sink: ready.sink,
            player: ready.player,
        };

        playing.player.append(ProgressTracked {
            inner: source,
            counter: progress,
        });
        playing.player.play();
        // playing.sink.mixer().add(ProgressTracked {
        //     inner: source,
        //     counter: progress,
        // });

        self.state = PlayerState::Playing(playing);
    }

    pub fn clear(&mut self) {
        self.state = match std::mem::replace(&mut self.state, PlayerState::Idle) {
            PlayerState::Idle => PlayerState::Idle,

            PlayerState::Ready(ready) => {
                ready.player.clear();
                PlayerState::Ready(ready)
            }

            PlayerState::Playing(playing) => {
                playing.player.clear();

                PlayerState::Ready(playing.into())
            }
        };
    }

    pub fn get_elapsed_secs(&self) -> f64 {
        match &self.state {
            PlayerState::Playing(playing) => {
                let samples = playing.progress.load(std::sync::atomic::Ordering::Relaxed);
                samples as f64
                    / (u32::from(playing.sample_rate) as f64 * u16::from(playing.channels) as f64)
            }
            _ => 0.0,
        }
    }

    pub fn get_volume(&self) -> f32 {
        match &self.state {
            PlayerState::Ready(state) => state.player.volume(),
            PlayerState::Playing(state) => state.player.volume(),
            PlayerState::Idle => 0.0,
        }
    }
}

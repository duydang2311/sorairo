use std::{
    cell::RefCell,
    path::PathBuf,
    rc::Rc,
    sync::atomic::{AtomicUsize, Ordering},
};

use crate::{
    common::{EventBus, audio::service::AudioService},
    error::AppError,
};

static PLAYLIST_ITEM_NEXT_ID: AtomicUsize = AtomicUsize::new(1);

#[derive(Clone)]
pub struct PlaylistService {
    state: PlaylistState,
    event_bus: EventBus,
    audio_service: AudioService,
}

#[derive(Debug, Clone, Default)]
pub struct PlaylistState {
    pub inner: Rc<RefCell<PlaylistStateInner>>,
}

#[derive(Debug, Clone, Default)]
pub struct PlaylistStateInner {
    pub tracks: Vec<Track>,
    pub current_track: Option<Track>,
}

#[derive(Debug, Clone, Default)]
pub struct Track {
    pub id: usize,
    pub path: PathBuf,
    pub title: Option<String>,
    pub artist: Option<String>,
}

pub struct PlaylistCurrentTrackChanged {
    track: Option<Track>,
}

pub struct TrackPlayed {
    track: Track,
}

impl Track {
    pub fn new(path: PathBuf) -> Self {
        Track {
            id: PLAYLIST_ITEM_NEXT_ID.fetch_add(1, Ordering::Relaxed),
            path,
            ..Default::default()
        }
    }
}

impl PlaylistService {
    pub fn new(event_bus: EventBus, audio_service: AudioService) -> Self {
        Self {
            state: PlaylistState::default(),
            event_bus,
            audio_service,
        }
    }

    pub fn add_track(&self, track: Track) {
        let mut inner = self.state.inner.borrow_mut();
        inner.tracks.push(track);
    }

    pub fn set_current_item(&self, track: Track) {
        let mut inner = self.state.inner.borrow_mut();
        inner.current_track = Some(track);
        self.event_bus.publish(PlaylistCurrentTrackChanged {
            track: inner.current_track.clone(),
        });
    }

    pub fn play(&self) -> Result<(), AppError> {
        let inner = self.state.inner.borrow();
        let track = inner
            .current_track
            .as_ref()
            .expect("failed to get current track");
        self.audio_service
            .play_sound(&track.path.to_string_lossy())?;
        self.event_bus.publish(TrackPlayed {
            track: track.clone(),
        });
        Ok(())
    }
}

use crate::common::{audio::service::AudioService, event::EventBus};

#[derive(Debug, Clone)]
pub struct AppContext {
    pub bus: EventBus,
    pub audio: AudioService,
}

#[derive(Debug, Clone)]
pub struct PlaylistState {
    pub items: Vec<PlaylistItem>,
}

#[derive(Debug, Clone)]
pub struct PlaylistItem {
    pub path: String,
    pub title: Option<String>,
    pub artist: Option<String>,
}

impl AppContext {
    pub fn new(bus: EventBus, audio: AudioService) -> Self {
        AppContext { bus, audio }
    }
}

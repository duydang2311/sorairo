use crate::common::{PlaylistService, audio::service::AudioService, event::EventBus};

#[derive(Clone)]
pub struct AppContext {
    pub bus: EventBus,
    pub audio: AudioService,
    pub playlist: PlaylistService,
}

use crate::common::{audio::service::AudioService, event::EventBus};

#[derive(Clone)]
pub struct AppContext {
    pub bus: EventBus,
    pub audio: AudioService,
}

impl AppContext {
    pub fn new(bus: EventBus, audio: AudioService) -> Self {
        AppContext { bus, audio }
    }
}

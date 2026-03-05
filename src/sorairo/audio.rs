use crossbeam_channel::{Receiver, Sender};
use std::thread::JoinHandle;

pub enum AudioEvent {
    DeviceChanged,
}

pub struct AudioHandle {
    pub thread: JoinHandle<()>,
    pub tx: Sender<AudioEvent>,
    pub rx: Receiver<AudioEvent>,
}

use crate::sorairo;

#[cfg(target_os = "windows")]
use crossbeam_channel::Receiver;

#[cfg(target_os = "windows")]
use sorairo::{app::AppEvent, audio::AudioHandle};

#[cfg(target_os = "windows")]
pub fn spawn_audio_thread(app_rx: Receiver<AppEvent>) -> Option<AudioHandle> {
    let (thread, tx, rx) = sorairo::windows::spawn_audio_thread(app_rx);

    Some(AudioHandle {
        thread: thread,
        tx: tx,
        rx: rx,
    })
}

#[cfg(not(target_os = "windows"))]
pub fn spawn_audio_thread(_app_rx: Receiver<AppEvent>) -> Option<AudioHandle> {
    None
}

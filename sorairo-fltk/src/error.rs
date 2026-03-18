use thiserror::Error;

use crate::common::audio::sys::MaResult;

#[derive(Error, Debug)]
pub enum AppError {
    #[error(transparent)]
    Audio(#[from] AudioError),
}

#[derive(Error, Debug)]
pub enum AudioError {
    #[error("failed to initialize audio engine: {0}")]
    EngineInit(MaResult),
    #[error("failed to play sound: {0}")]
    PlaySound(MaResult),
    #[error("failed to init sound: {0}")]
    InitSound(MaResult),
    #[error("failed to start sound: {0}")]
    StartSound(MaResult),
}

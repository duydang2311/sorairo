use std::{cell::RefCell, rc::Rc};

use crate::{
    common::{
        EventBus,
        audio::{
            sys::{
                MaResult, ma_engine, ma_engine_uninit, ma_sound, ma_sound_start, ma_sound_stop,
                ma_sound_uninit,
            },
            wrapper::{init_audio_engine, init_sound_from_file},
        },
    },
    error::{AppError, AudioError},
};

#[derive(Clone)]
pub struct AudioService {
    inner: Rc<RefCell<AudioServiceInner>>,
    event_bus: EventBus,
}

#[derive(Default)]
struct AudioServiceInner {
    engine: Option<Box<ma_engine>>,
    sound: Option<Box<ma_sound>>,
    playback_status: PlaybackStatus,
}

#[derive(Default, Clone, Copy, PartialEq)]
pub enum PlaybackStatus {
    #[default]
    Stopped,
    Playing,
    Paused,
}

pub struct PlaybackStatusChanged {
    pub status: PlaybackStatus,
}

impl AudioService {
    pub fn new(event_bus: EventBus) -> Self {
        AudioService {
            event_bus,
            inner: Rc::new(RefCell::new(AudioServiceInner::default())),
        }
    }

    pub fn play(&self, path: &str) -> Result<(), AppError> {
        {
            let mut inner = self.inner.borrow_mut();
            if inner.engine.is_none() {
                inner.engine = Some(init_audio_engine()?);
            }
        }

        let mut inner = self.inner.borrow_mut();
        if let Some(mut sound) = inner.sound.take() {
            unsafe {
                ma_sound_uninit(sound.as_mut());
            }
        }
        let engine = inner.engine.as_mut().unwrap();
        let mut sound = init_sound_from_file(engine, path)?;
        unsafe {
            match ma_sound_start(sound.as_mut()) {
                MaResult::Success => {
                    inner.playback_status = PlaybackStatus::Playing;
                    inner.sound = Some(sound);
                    self.event_bus.publish(PlaybackStatusChanged {
                        status: PlaybackStatus::Playing,
                    });
                    Ok(())
                }
                result => Err(AudioError::StartSound(result).into()),
            }
        }
    }

    pub fn pause(&self) -> Result<(), AppError> {
        let mut inner = self.inner.borrow_mut();
        if let Some(ref mut sound) = inner.sound {
            unsafe {
                match ma_sound_stop(sound.as_mut()) {
                    MaResult::Success => {
                        inner.playback_status = PlaybackStatus::Paused;
                        self.event_bus.publish(PlaybackStatusChanged {
                            status: PlaybackStatus::Paused,
                        });
                        Ok(())
                    }
                    result => Err(AudioError::StopSound(result).into()),
                }
            }
        } else {
            Ok(())
        }
    }

    pub fn resume(&self) -> Result<(), AppError> {
        let mut inner = self.inner.borrow_mut();
        if let Some(ref mut sound) = inner.sound {
            unsafe {
                match ma_sound_start(sound.as_mut()) {
                    MaResult::Success => {
                        inner.playback_status = PlaybackStatus::Playing;
                        self.event_bus.publish(PlaybackStatusChanged {
                            status: PlaybackStatus::Playing,
                        });
                        Ok(())
                    }
                    result => Err(AudioError::StartSound(result).into()),
                }
            }
        } else {
            Ok(())
        }
    }

    pub fn get_playback_status(&self) -> PlaybackStatus {
        let inner = self.inner.borrow();
        inner.playback_status
    }
}

impl Drop for AudioServiceInner {
    fn drop(&mut self) {
        unsafe {
            if let Some(mut sound) = self.sound.take() {
                ma_sound_uninit(sound.as_mut());
            }
            if let Some(mut engine) = self.engine.take() {
                ma_engine_uninit(engine.as_mut());
            }
        }
    }
}

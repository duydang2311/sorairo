use std::{cell::RefCell, rc::Rc};

use crate::{
    common::audio::{
        sys::{MaResult, ma_engine, ma_engine_uninit, ma_sound, ma_sound_start, ma_sound_uninit},
        wrapper::{init_audio_engine, init_sound_from_file},
    },
    error::{AppError, AudioError},
};

#[derive(Clone)]
pub struct AudioService {
    inner: Rc<RefCell<AudioServiceInner>>,
}

#[derive(Default)]
struct AudioServiceInner {
    engine: Option<Box<ma_engine>>,
    sound: Option<Box<ma_sound>>,
}

impl AudioService {
    pub fn new() -> Self {
        AudioService {
            inner: Rc::new(RefCell::new(AudioServiceInner::default())),
        }
    }

    pub fn play_sound(&self, path: &str) -> Result<(), AppError> {
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
            let result = ma_sound_start(sound.as_mut());
            match result {
                MaResult::Success => {
                    inner.sound = Some(sound);
                    Ok(())
                }
                _ => Err(AudioError::StartSound(result).into()),
            }
        }
    }
}

impl Drop for AudioService {
    fn drop(&mut self) {
        unsafe {
            let mut inner = self.inner.take();
            if let Some(mut sound) = inner.sound.take() {
                ma_sound_uninit(sound.as_mut());
            }
            if let Some(mut engine) = inner.engine.take() {
                ma_engine_uninit(engine.as_mut());
            }
        }
    }
}

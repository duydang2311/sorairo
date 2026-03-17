use std::{cell::RefCell, path::PathBuf, rc::Rc};

use crate::{
    common::audio::{
        engine::init_audio_engine,
        sys::{ma_engine, ma_engine_init},
    },
    error::AppError,
};

#[derive(Clone)]
pub struct AudioService {
    inner: Rc<RefCell<AudioServiceInner>>,
}

struct AudioServiceInner {
    engine: Option<Box<ma_engine>>,
}

impl AudioService {
    pub fn new() -> Self {
        AudioService {
            inner: Rc::new(RefCell::new(AudioServiceInner { engine: None })),
        }
    }

    pub fn play_sound(&self, path: &str) -> Result<(), AppError> {
        let mut inner = self.inner.borrow_mut();

        if inner.engine.is_none() {
            inner.engine = Some(init_audio_engine()?);
        }

        let engine = inner.engine.as_mut().unwrap();
        engine.play_sound(path);
        Ok(())
    }
}

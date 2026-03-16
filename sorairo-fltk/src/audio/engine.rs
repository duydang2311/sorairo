use std::ffi::CString;

use crate::{
    audio::sys::{MaResult, ma_engine, ma_engine_init, ma_engine_play_sound, ma_engine_uninit},
    error::{AppError, AudioError},
};

pub fn init_audio_engine() -> Result<Box<ma_engine>, AppError> {
    unsafe {
        let mut engine = Box::<ma_engine>::new(std::mem::zeroed());
        let result = ma_engine_init(std::ptr::null(), &mut *engine);
        match result {
            MaResult::Success => Ok(engine),
            result => Err(AudioError::EngineInit(result).into()),
        }
    }
}

impl ma_engine {
    pub fn play_sound(&mut self, path: &str) -> Result<(), AppError> {
        unsafe {
            let path = CString::new(path).unwrap();
            let result = ma_engine_play_sound(self, path.as_ptr(), std::ptr::null_mut());
            match result {
                MaResult::Success => Ok(()),
                result => Err(AudioError::PlaySound(result).into()),
            }
        }
    }

    pub fn uninit(&mut self) {
        unsafe { ma_engine_uninit(self) }
    }
}

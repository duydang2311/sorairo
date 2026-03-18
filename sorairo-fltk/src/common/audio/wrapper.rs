use std::{
    env::{self, consts::OS},
    ffi::CString,
};

use crate::{
    common::audio::sys::{
        MA_SOUND_FLAG_ASYNC, MA_SOUND_FLAG_NO_PITCH, MA_SOUND_FLAG_NO_SPATIALIZATION,
        MA_SOUND_FLAG_STREAM, MaResult, ma_engine, ma_engine_init, ma_engine_play_sound,
        ma_engine_uninit, ma_sound, ma_sound_init_from_file, ma_sound_init_from_file_w,
    },
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

pub fn init_sound_from_file(engine: &mut ma_engine, path: &str) -> Result<Box<ma_sound>, AppError> {
    unsafe {
        let mut sound = Box::<ma_sound>::new(std::mem::zeroed());
        let flags = MA_SOUND_FLAG_STREAM | MA_SOUND_FLAG_NO_PITCH | MA_SOUND_FLAG_NO_SPATIALIZATION;
        let result = match env::consts::OS {
            "windows" => {
                let mut path: Vec<u16> = path.encode_utf16().collect();
                path.push(0);
                ma_sound_init_from_file_w(
                    engine,
                    path.as_ptr(),
                    flags,
                    std::ptr::null_mut(),
                    std::ptr::null_mut(),
                    sound.as_mut(),
                )
            }
            _ => ma_sound_init_from_file(
                engine,
                CString::new(path).unwrap().as_ptr(),
                flags,
                std::ptr::null_mut(),
                std::ptr::null_mut(),
                sound.as_mut(),
            ),
        };
        match result {
            MaResult::Success => Ok(sound),
            result => Err(AudioError::InitSound(result).into()),
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

mod audio;
mod error;
mod ui;

use std::time::Duration;

use fltk::{app, enums::Font};

use crate::{audio::engine::init_audio_engine, ui::shell_window::create_shell_window};

fn main() {
    let app = app::App::default();

    create_shell_window();

    // println!("1");
    // let mut engine = init_audio_engine().expect("init audio engine");
    // println!("2");
    // engine.play_sound("file.mp3").expect("failed to play sound");
    // println!("3");
    // std::thread::sleep(Duration::from_secs(2));
    // println!("4");
    // engine.uninit();

    app::background(255, 255, 255);
    app::set_visible_focus(false);
    app::set_font(Font::Helvetica);

    app.run().unwrap();
}

mod audio;
mod error;
mod ui;
mod event_bus;

use std::time::Duration;

use fltk::{app, enums::Font, prelude::WidgetExt};

use crate::{audio::engine::init_audio_engine, ui::shell_window::create_shell_window};

fn main() {
    let app = app::App::default();

    let mut window = create_shell_window();
    window.show();

    // println!("1");
    // let mut engine = init_audio_engine().expect("init audio engine");
    // println!("2");
    // engine.play_sound("C:\\Users\\duyda\\Downloads\\Hardwell & Blasterjaxx - Beat Of The Drum [lGIYEiACX-I].mp3").expect("failed to play sound");
    // println!("3");
    // std::thread::sleep(Duration::from_secs(45));
    // println!("4");
    // engine.uninit();

    app::background(255, 255, 255);
    app::set_visible_focus(false);
    app::set_font(Font::Helvetica);

    let (s, r) = app::channel();

    // app.run().unwrap();
    while app.wait() {
        println!("app.wait");
        if let Some(msg) = r.recv() {
            match msg {
                true => println!("Clicked"),
                false => (), // Here we basically do nothing
            }
        }
    }
}

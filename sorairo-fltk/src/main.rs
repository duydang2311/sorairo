// #![windows_subsystem = "windows"]

mod common;
mod error;
mod ui;

use std::{env, fs::File, io::Write, path::PathBuf};

use fltk::{app, enums::Font, prelude::FltkError};

use crate::{
    common::{
        AppContext, EventBus, FileOpened, PlaylistService, Track, audio::service::AudioService,
    },
    ui::ShellView,
};

fn main() -> Result<(), FltkError> {
    let mut event_bus = EventBus::new();
    let audio_service = AudioService::new(event_bus.clone());
    let playlist_service = PlaylistService::new(event_bus.clone(), audio_service.clone());
    let ctx = AppContext {
        bus: event_bus.clone(),
        audio: audio_service.clone(),
        playlist: playlist_service,
    };

    let app = app::App::default();
    register_fonts(&app)?;

    let mut shell = ShellView::new(ctx.clone());
    shell.show();

    app::background(255, 255, 255);
    app::set_visible_focus(false);

    event_bus.subscribe::<FileOpened>({
        let ctx = ctx.clone();
        move |opened| {
            let item = Track::new(opened.path.clone());
            ctx.playlist.add_track(item.clone());
            ctx.playlist.set_current_item(item);
            ctx.playlist.play().expect("failed to play track");
        }
    });

    app.run()?;
    Ok(())
}

fn register_fonts(app: &fltk::app::App) -> Result<(), FltkError> {
    let regular_bytes = include_bytes!("../assets/fonts/OpenSans-Regular.ttf");
    let regular_italic_bytes = include_bytes!("../assets/fonts/OpenSans-Italic.ttf");
    let bold_bytes = include_bytes!("../assets/fonts/OpenSans-Bold.ttf");
    let bold_italic_bytes = include_bytes!("../assets/fonts/OpenSans-BoldItalic.ttf");

    let regular = app.load_font(write_temp_asset("OpenSans-Regular.ttf", regular_bytes))?;
    let italic = app.load_font(write_temp_asset(
        "OpenSans-Italic.ttf",
        regular_italic_bytes,
    ))?;
    let bold = app.load_font(write_temp_asset("OpenSans-Bold.ttf", bold_bytes))?;
    let bold_italic = app.load_font(write_temp_asset(
        "OpenSans-BoldItalic.ttf",
        bold_italic_bytes,
    ))?;
    Font::set_font(Font::Helvetica, &regular);
    Font::set_font(Font::HelveticaItalic, &italic);
    Font::set_font(Font::HelveticaBold, &bold);
    Font::set_font(Font::HelveticaBoldItalic, &bold_italic);
    Ok(())
}

fn write_temp_asset(name: &str, data: &[u8]) -> PathBuf {
    let path = env::temp_dir().join(name);
    let mut file = File::create(&path).unwrap();
    file.write_all(data).unwrap();
    path
}

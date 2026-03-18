mod common;
mod error;
mod ui;

use fltk::{app, enums::Font, prelude::FltkError};

use crate::{
    common::{
        AppContext, EventBus, FileOpened, PlaylistService, Track, audio::service::AudioService,
    },
    ui::ShellView,
};

fn main() -> Result<(), FltkError> {
    let mut event_bus = EventBus::new();
    let audio_service = AudioService::new();
    let playlist_service = PlaylistService::new(event_bus.clone(), audio_service.clone());
    let ctx = AppContext {
        bus: event_bus.clone(),
        audio: audio_service.clone(),
        playlist: playlist_service,
    };

    let app = app::App::default();
    let regular = app.load_font("assets/fonts/OpenSans-Regular.ttf")?;
    let italic = app.load_font("assets/fonts/OpenSans-Italic.ttf")?;
    let bold = app.load_font("assets/fonts/OpenSans-Bold.ttf")?;
    let bold_italic = app.load_font("assets/fonts/OpenSans-BoldItalic.ttf")?;
    Font::set_font(Font::Helvetica, &regular);
    Font::set_font(Font::HelveticaItalic, &italic);
    Font::set_font(Font::HelveticaBold, &bold);
    Font::set_font(Font::HelveticaBoldItalic, &bold_italic);

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
            ctx.playlist.play().expect("cant play sound");
        }
    });

    app.run()
}

mod common;
mod error;
mod ui;

use fltk::{app, enums::Font, prelude::FltkError};

use crate::{
    common::{
        app::AppContext,
        audio::service::AudioService,
        event::{EventBus, FileOpened},
    },
    ui::ShellView,
};

fn main() -> Result<(), FltkError> {
    let app = app::App::default();

    let mut event_bus = EventBus::new();
    let audio = AudioService::new();
    let ctx = AppContext::new(event_bus.clone(), audio.clone());
    let mut shell = ShellView::new(ctx);
    shell.show();

    app::background(255, 255, 255);
    app::set_visible_focus(false);
    app::set_font(Font::Helvetica);

    event_bus.subscribe::<FileOpened>(move |opened| {
        audio
            .play_sound(&opened.path.as_os_str().to_string_lossy())
            .expect("failed to play sound");
    });

    app.run()
}

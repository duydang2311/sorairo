use fltk::{
    frame::Frame,
    group,
    prelude::{GroupExt, WidgetBase, WidgetExt},
    window::{self, Window},
};

use crate::{event_bus::EventBus, ui::sys_menu_bar::draw_menu_bar};

pub trait View {
    type Message;
    fn draw(&mut self);
    fn handle(&mut self, msg: Self::Message);
}

pub fn create_shell_window() -> Window {
    let mut wind = window::Window::default()
        .with_size(400, 300)
        .with_label("Sorairo");
    let mut event_bus = EventBus::new();
    event_bus.subscribe(|a: &String| {
        println!("Hello world {}", a);
    });
    let mut flex = group::Flex::default_fill();
    flex.set_type(group::FlexType::Column);
    let menu = draw_menu_bar();
    let expanding = Frame::default().with_label("Hello world");
    flex.fixed(&menu, 30);
    flex.end();

    wind.end();

    wind.make_resizable(true);
    wind.resizable(&flex);
    wind
}

use fltk::{
    button, group,
    prelude::{GroupExt, WidgetBase, WidgetExt},
    window,
};

use crate::ui::sys_menu_bar::draw_menu_bar;

pub fn create_shell_window() {
    let mut wind = window::Window::default()
        .with_size(400, 300)
        .with_label("Sorairo");
    let mut flex = group::Flex::new(0, 0, 400, 300, None);
    flex.set_type(group::FlexType::Column);
    let menu = draw_menu_bar();
    let expanding = button::Button::default().with_label("Expanding");
    flex.fixed(&menu, 30);
    flex.end();

    wind.end();

    wind.make_resizable(true);
    wind.resizable(&flex);
    wind.show();
}

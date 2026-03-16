use fltk::{
    dialog::{FileDialog, FileDialogType},
    enums::{Color, Shortcut},
    menu::{self, SysMenuBar},
    prelude::{MenuExt, WidgetExt},
};

pub fn draw_menu_bar() -> SysMenuBar {
    let mut menu = menu::SysMenuBar::default().with_size(800, 35);
    menu.add(
        "&File/New...\t",
        Shortcut::Ctrl | 'n',
        menu::MenuFlag::Normal,
        |m| {
            pick_file();
        },
    );

    menu.add(
        "&File/Open File...\t",
        Shortcut::Ctrl | 'o',
        menu::MenuFlag::Normal,
        |m| {},
    );

    menu.add(
        "&File/Print...\t",
        Shortcut::Ctrl | 'p',
        menu::MenuFlag::MenuDivider,
        |m| {},
    );

    menu.add(
        "&File/Quit\t",
        Shortcut::Ctrl | 'q',
        menu::MenuFlag::Normal,
        |m| {},
    );

    // if let Some(mut item) = menu.find_item("&File/Quit\t") {
    //     item.set_label_color(Color::Red);
    // }

    menu
}

fn pick_file() {
    let mut dialog = FileDialog::new(FileDialogType::BrowseFile);
    dialog.set_title("audio");
    dialog.set_filter("Audio Files\t*.{mp3,wav}");
    dialog.show();
    println!("filename: {}", dialog.filename().display());
}

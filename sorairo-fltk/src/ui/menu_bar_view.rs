use fltk::{
    dialog::{FileDialog, FileDialogAction, FileDialogType},
    enums::Shortcut,
    menu::{self, SysMenuBar},
    prelude::{MenuExt, WidgetExt},
};

use crate::common::{app::AppContext, event::FileOpened};

pub struct MenuBarView {
    pub menu: SysMenuBar,
    pub ctx: AppContext,
}

impl MenuBarView {
    pub fn new(ctx: &mut AppContext) -> Self {
        let mut menu = menu::SysMenuBar::default().with_size(800, 35);

        let ctx_clone = ctx.clone();
        menu.add(
            "&File/Open File...\t",
            Shortcut::Ctrl | 'o',
            menu::MenuFlag::Normal,
            move |_| {
                pick_file(ctx_clone.clone());
            },
        );

        menu.add(
            "&File/Quit\t",
            Shortcut::Ctrl | 'q',
            menu::MenuFlag::Normal,
            |m| {
                fltk::app::quit();
            },
        );

        Self {
            ctx: ctx.clone(),
            menu,
        }
    }
}

fn pick_file(ctx: AppContext) {
    let mut dialog = FileDialog::new(FileDialogType::BrowseFile);
    dialog.set_title("audio");
    dialog.set_filter("Audio Files\t*.{mp3,wav}");
    match dialog.try_show() {
        Err(_) => {}
        Ok(action) => match action {
            FileDialogAction::Success => {
                ctx.bus.publish(FileOpened {
                    path: dialog.filename(),
                });
            }
            _ => {}
        },
    }
}

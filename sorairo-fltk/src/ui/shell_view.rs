use fltk::{
    group,
    prelude::{GroupExt, WidgetBase, WidgetExt},
    window::{self, Window},
};

use crate::{
    common::{AppContext, SubscriptionTracker},
    ui::{NowPlayingView, menu_bar_view::MenuBarView},
};

pub struct ShellView {
    pub window: Window,
    pub content: group::Group,
    pub ctx: AppContext,
    tracker: SubscriptionTracker,
}

impl ShellView {
    pub fn new(mut ctx: AppContext) -> Self {
        let mut tracker = SubscriptionTracker::default();
        let mut window = window::Window::default()
            .with_size(400, 300)
            .with_label("Sorairo");
        let mut root_flex = group::Flex::default_fill();
        root_flex.set_type(group::FlexType::Column);

        let menu_bar_view = MenuBarView::new(&mut ctx);
        let content = group::Group::default_fill();
        NowPlayingView::new(ctx.clone());
        content.end();

        root_flex.fixed(&menu_bar_view.menu, 30);
        root_flex.end();

        window.end();

        window.make_resizable(true);
        window.resizable(&root_flex);

        tracker.add(ctx.bus.subscribe::<String>(|a| {}));

        Self {
            ctx,
            tracker,
            window,
            content,
        }
    }

    pub fn show(&mut self) {
        self.window.show();
    }

    pub fn destroy(mut self) {
        self.tracker.clear(&mut self.ctx.bus);
        window::Window::delete(self.window);
    }
}

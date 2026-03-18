use std::{cell::RefCell, rc::Rc};

use fltk::{
    frame::Frame,
    group::{self, FlexType},
    prelude::*,
};

use crate::common::{
    app::AppContext,
    event::{FileOpened, SubscriptionTracker},
};

pub struct NowPlayingView {
    ctx: AppContext,
    tracker: SubscriptionTracker,
    container_rc: Rc<RefCell<group::Flex>>,
}

impl NowPlayingView {
    pub fn new(mut ctx: AppContext) -> Self {
        let mut tracker = SubscriptionTracker::default();
        let container_rc = Rc::new(RefCell::new(group::Flex::default_fill()));
        {
            let mut container = container_rc.borrow_mut();
            container.set_type(FlexType::Row);
            container.end();
        }

        tracker.add(ctx.bus.subscribe::<FileOpened>({
            let container_rc = container_rc.clone();
            move |opened| {
                let mut container = container_rc.borrow_mut();
                container.clear();

                container.begin();
                let mut frame = Frame::default_fill();
                frame.set_label(&opened.path.to_string_lossy());
                container.end();
            }
        }));

        Self {
            ctx,
            tracker,
            container_rc,
        }
    }

    pub fn destroy(mut self) {
        self.tracker.clear(&mut self.ctx.bus);
        group::Flex::delete(self.container_rc.borrow_mut().clone());
    }
}

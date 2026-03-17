use fltk::{
    frame::Frame,
    group,
    prelude::{GroupExt, WidgetBase, WidgetExt},
    window::{self, Window},
};

use crate::{event_bus::EventBus, ui::sys_menu_bar::draw_menu_bar};

pub struct View<T> {
    pub inner: T,
    tracker: SubscriptionTracker,
}

impl<T> View<T> {
    /// The universal 'new' that takes a setup closure
    pub fn new<F>(bus: &mut EventBus, setup: F) -> Self 
    where 
        F: FnOnce(&mut EventBus, &mut SubscriptionTracker) -> T 
    {
        let mut tracker = SubscriptionTracker::default();
        
        // Execute the user's setup logic
        let inner = setup(bus, &mut tracker);

        Self { inner, tracker }
    }

    pub fn destroy(mut self, bus: &mut EventBus) {
        self.tracker.clear(bus);
        println!("View resources and subscriptions cleared.");
    }
}

pub fn create_shell_window(bus: &mut EventBus) -> Window {
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

    bus.subscribe::<String>(|a| {});

    wind
}

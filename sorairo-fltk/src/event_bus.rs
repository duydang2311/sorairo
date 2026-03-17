use std::any::{Any, TypeId};
use std::collections::HashMap;

pub type Handler = Box<dyn Fn(&dyn Any)>;

pub struct EventBus {
    handlers: HashMap<TypeId, Vec<Handler>>,
}

impl EventBus {
    pub fn new() -> EventBus {
        EventBus {
            handlers: HashMap::new(),
        }
    }
    pub fn subscribe<T: 'static>(&mut self, f: impl Fn(&T) + 'static) {
        let entry = self.handlers.entry(TypeId::of::<T>()).or_default();

        entry.push(Box::new(move |msg| {
            let msg = msg.downcast_ref::<T>().unwrap();
            f(msg);
        }));
    }
    pub fn publish<T: 'static>(&self, msg: T) {
        if let Some(handlers) = self.handlers.get(&TypeId::of::<T>()) {
            for h in handlers {
                h(&msg);
            }
        }
    }
}

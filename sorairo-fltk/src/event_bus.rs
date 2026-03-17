use std::any::{Any, TypeId};
use std::collections::HashMap;

pub type Handler = Box<dyn Fn(&dyn Any)>;

pub struct EventBus {
    handlers: HashMap<TypeId, Vec<(u64, Handler)>>,
    next_id: u64,
}

impl EventBus {
    pub fn new() -> Self {
        Self {
            handlers: HashMap::new(),
            next_id: 0,
        }
    }

    pub fn subscribe<T: 'static>(&mut self, f: impl Fn(&T) + 'static) -> u64 {
        let id = self.next_id;
        self.next_id += 1;

        let entry = self.handlers.entry(TypeId::of::<T>()).or_default();

        let wrapper = Box::new(move |msg: &dyn Any| {
            if let Some(msg) = msg.downcast_ref::<T>() {
                f(msg);
            }
        });

        entry.push((id, wrapper));
        id
    }

    pub fn unsubscribe(&mut self, id: u64) -> bool {
        for handlers in self.handlers.values_mut() {
            if let Some(pos) = handlers.iter().position(|(h_id, _)| *h_id == id) {
                handlers.remove(pos);
                return true;
            }
        }
        false
    }

    pub fn publish<T: 'static>(&self, msg: T) {
        if let Some(handlers) = self.handlers.get(&TypeId::of::<T>()) {
            for (_, h) in handlers {
                h(&msg);
            }
        }
    }
}

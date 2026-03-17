use std::any::{Any, TypeId};
use std::cell::RefCell;
use std::collections::HashMap;
use std::path::PathBuf;
use std::rc::Rc;

pub type EventHandler = Box<dyn Fn(&dyn Any)>;

#[derive(Clone)]
pub struct EventBus {
    handlers: Rc<RefCell<HashMap<TypeId, Vec<(u64, EventHandler)>>>>,
    next_id: u64,
}

#[derive(Default)]
pub struct SubscriptionTracker {
    ids: Vec<u64>,
}

pub struct FileOpened {
    pub path: PathBuf,
}

impl EventBus {
    pub fn new() -> Self {
        Self {
            handlers: Rc::new(RefCell::new(HashMap::new())),
            next_id: 0,
        }
    }

    pub fn subscribe<T: 'static>(&mut self, f: impl Fn(&T) + 'static) -> u64 {
        let id = self.next_id;
        self.next_id += 1;

        let mut handlers = self.handlers.borrow_mut();
        let entry = handlers.entry(TypeId::of::<T>()).or_default();

        let wrapper = Box::new(move |msg: &dyn Any| {
            if let Some(msg) = msg.downcast_ref::<T>() {
                f(msg);
            }
        });

        entry.push((id, wrapper));
        id
    }

    pub fn unsubscribe(&mut self, id: u64) -> bool {
        let mut handlers = self.handlers.borrow_mut();
        for handlers in handlers.values_mut() {
            if let Some(pos) = handlers.iter().position(|(h_id, _)| *h_id == id) {
                handlers.remove(pos);
                return true;
            }
        }
        false
    }

    pub fn publish<T: 'static>(&self, msg: T) {
        let handlers = self.handlers.borrow();
        if let Some(handlers) = handlers.get(&TypeId::of::<T>()) {
            for (_, h) in handlers {
                h(&msg);
            }
        }
    }
}

impl SubscriptionTracker {
    pub fn add(&mut self, id: u64) {
        self.ids.push(id);
    }

    pub fn clear(&mut self, bus: &mut EventBus) {
        for id in self.ids.drain(..) {
            bus.unsubscribe(id);
        }
    }
}

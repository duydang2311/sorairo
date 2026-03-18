use alien_signals::Effect;

pub type DisposeFn = Box<dyn FnOnce() + 'static>;

#[derive(Default)]
pub struct DisposableBag {
    disposables: Vec<DisposeFn>,
}

impl DisposableBag {
    pub fn new() -> Self {
        Self::default()
    }

    pub fn add(&mut self, dispose: impl FnOnce() + 'static) {
        self.disposables.push(Box::new(dispose));
    }

    pub fn dispose(mut self) {
        for dispose in self.disposables.drain(..) {
            dispose();
        }
    }
}

pub trait EffectDisposableBag {
    fn add_to(self, disposables: &mut DisposableBag);
}

impl EffectDisposableBag for Effect {
    fn add_to(self, disposables: &mut DisposableBag) {
        disposables.add(|| {
            self.dispose();
        });
    }
}

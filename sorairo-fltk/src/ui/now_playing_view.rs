use std::{cell::RefCell, rc::Rc};

use alien_signals::{effect, signal};
use fltk::{
    button::Button,
    enums::Align,
    frame::Frame,
    group::{self, FlexType},
    prelude::*,
};

use crate::common::{
    AppContext, DisposableBag, EffectDisposableBag, PlaylistCurrentTrackChanged,
    SubscriptionTracker,
    audio::service::{PlaybackStatus, PlaybackStatusChanged},
};

pub struct NowPlayingView {
    ctx: AppContext,
    tracker: SubscriptionTracker,
    main: Rc<RefCell<group::Group>>,
    play_button: Rc<RefCell<Button>>,
    disposables: DisposableBag,
}

pub struct TrackView {
    ctx: AppContext,
    tracker: SubscriptionTracker,
    container: Rc<RefCell<group::Flex>>,
}

impl NowPlayingView {
    pub fn new(mut ctx: AppContext) -> Self {
        let playback_status = signal(ctx.audio.get_playback_status());
        let mut disposables = DisposableBag::new();

        let mut tracker = SubscriptionTracker::default();
        let play_button_rc: Rc<RefCell<Button>>;
        let main_rc: Rc<RefCell<group::Group>>;
        {
            let mut container = group::Flex::default_fill();
            container.set_type(FlexType::Column);

            main_rc = Rc::new(RefCell::new(group::Group::default_fill()));
            main_rc.borrow_mut().end();

            let mut bottom = group::Flex::default_fill();
            play_button_rc = Rc::new(RefCell::new(Button::default().with_size(24, 24)));
            {
                let mut play_button = play_button_rc.borrow_mut();
                play_button.set_label("Play");
                play_button.set_align(Align::Center);
                play_button.set_callback({
                    let ctx = ctx.clone();
                    move |_| match ctx.audio.get_playback_status() {
                        PlaybackStatus::Stopped => {
                            ctx.playlist.play().expect("failed to play track");
                        }
                        PlaybackStatus::Playing => {
                            ctx.audio.pause().expect("failed to pause audio");
                        }
                        PlaybackStatus::Paused => {
                            ctx.audio.resume().expect("failed to resume audio");
                        }
                    }
                });
            }
            bottom.end();
            container.fixed(&bottom, 32);
            container.end();
        }

        tracker.add(ctx.bus.subscribe::<PlaylistCurrentTrackChanged>({
            let main_rc = main_rc.clone();
            move |changed| {
                let mut main = main_rc.borrow_mut();
                main.clear();

                if let Some(track) = &changed.track {
                    main.begin();
                    let mut frame = Frame::default_fill();
                    frame.set_label(&track.path.to_string_lossy());
                    main.end();
                }
            }
        }));

        tracker.add(ctx.bus.subscribe::<PlaybackStatusChanged>(move |changed| {
            playback_status.set(changed.status);
        }));

        effect({
            let play_button_rc = play_button_rc.clone();
            move || {
                play_button_rc
                    .borrow_mut()
                    .set_label(match playback_status.get() {
                        PlaybackStatus::Stopped | PlaybackStatus::Paused => "Play",
                        PlaybackStatus::Playing => "Pause",
                    });
            }
        })
        .add_to(&mut disposables);

        Self {
            ctx,
            tracker,
            main: main_rc,
            play_button: play_button_rc,
            disposables,
        }
    }

    pub fn destroy(mut self) {
        self.tracker.clear(&mut self.ctx.bus);
        self.disposables.dispose();
        Button::delete(self.play_button.take());
        group::Group::delete(self.main.take());
    }
}

// impl Drop for NowPlayingView {
//     fn drop(&mut self) {
//         println!("Drop");
//         self.tracker.clear(&mut self.ctx.bus);
//         Button::delete(self.play_button.take());
//         group::Group::delete(self.main.take());
//     }
// }

impl TrackView {
    pub fn new(mut ctx: AppContext) -> Self {
        let mut tracker = SubscriptionTracker::default();
        let container_rc = Rc::new(RefCell::new(group::Flex::default_fill()));
        {
            let mut container = container_rc.borrow_mut();
            container.set_type(FlexType::Row);
            container.end();
        }

        tracker.add(ctx.bus.subscribe::<PlaylistCurrentTrackChanged>({
            let container_rc = container_rc.clone();
            move |changed| {
                let mut container = container_rc.borrow_mut();
                container.clear();

                if let Some(track) = &changed.track {
                    container.begin();
                    let mut frame = Frame::default_fill();
                    frame.set_label(&track.path.to_string_lossy());
                    container.end();
                }
            }
        }));

        Self {
            ctx,
            tracker,
            container: container_rc,
        }
    }

    pub fn destroy(mut self) {
        self.tracker.clear(&mut self.ctx.bus);
        group::Flex::delete(self.container.borrow_mut().clone());
    }
}

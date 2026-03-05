mod player;
mod sorairo;

use std::time::Duration;

use crossbeam_channel::{Receiver, Sender, unbounded};
use eframe::egui::{
    self, Align, Color32, Context, Layout, Margin, Rect, Sense, Shadow, Stroke, Ui,
    containers::{self},
    vec2,
};
use egui_extras::{Column, TableBuilder};
use player::{PlayerState, SorairoPlayer};

use crate::sorairo::{
    agnostic::spawn_audio_thread,
    app::{AppEvent, AppMainView},
};

struct Sorairo {
    player: SorairoPlayer,
    main_view: AppMainView,
    auto_size_this_frame: bool,
}

impl eframe::App for Sorairo {
    fn update(&mut self, ctx: &eframe::egui::Context, frame: &mut eframe::Frame) {
        let playlist_size = self.player.playlist.len();
        if playlist_size > 0
            && let PlayerState::Loaded(loaded) = &self.player.state
        {
            if loaded.player.empty() {
                self.player.set_current(Some(
                    self.player.get_current().map(|a| a + 1).unwrap_or(0) % playlist_size,
                ));
                self.player.play();
            }
        }
        let style = ctx.style();
        let side_top_frame = containers::Frame::side_top_panel(&style).inner_margin(Margin::ZERO);
        egui::TopBottomPanel::top("top_panel")
            .frame(side_top_frame.clone())
            .show(ctx, |ui| {
                egui::MenuBar::new().ui(ui, |ui| {
                    ui.spacing_mut().item_spacing.x = 0.0;
                    ui.spacing_mut().button_padding = vec2(8.0, 0.0);
                    ui.menu_button("file", |ui| {
                        if ui
                            .button("open")
                            .on_hover_text("add a file to playlist and play it")
                            .clicked()
                            && let Some(path) = rfd::FileDialog::new()
                                .add_filter("audio", &["mp3", "wav"])
                                .pick_file()
                        {
                            let path = path.display().to_string();
                            self.player.clear();
                            self.player.add_file(path.clone());
                            self.player.set_current(Some(0));
                            self.player.play();
                        }
                        ui.separator();
                        if ui.button("add files").clicked()
                            && let Some(paths) = rfd::FileDialog::new()
                                .add_filter("audio", &["mp3", "wav"])
                                .pick_files()
                        {
                            for path in &paths {
                                self.player.add_file(path.display().to_string());
                            }
                        }
                        if ui.button("add folder").clicked()
                            && let Some(path) = rfd::FileDialog::new().pick_folder()
                        {
                            // TODO: add all audio files in folder
                        }
                        ui.separator();
                        if ui
                            .button("new playlist")
                            .on_hover_text("create a new playlist")
                            .clicked()
                        {
                            self.player.set_current(None);
                            self.player.clear();
                        }
                        ui.separator();
                        if ui.button("quit").clicked() {
                            ui.ctx().send_viewport_cmd(egui::ViewportCommand::Close);
                        }
                    });
                    match self.main_view {
                        AppMainView::Playlist => {
                            ui.menu_button("tools", |ui| {
                                if ui
                                    .button("auto size")
                                    .on_hover_text(
                                        "automatically size the playlist based on its content",
                                    )
                                    .clicked()
                                {
                                    self.auto_size_this_frame = true;
                                }
                            });
                        }
                        AppMainView::NowPlaying => {}
                    };
                })
            });
        egui::TopBottomPanel::bottom("bottom_panel")
            .frame(side_top_frame)
            .show(ctx, |ui| {
                let progress_height = 8.0;
                draw_playback_progress(ctx, ui, self, progress_height);
                containers::Frame::new()
                    .inner_margin(Margin::symmetric(8, 2))
                    .show(ui, |ui| {
                        ui.horizontal(|ui| {
                            if !self.player.playlist.is_empty() {
                                if let PlayerState::Loaded(loaded) = &self.player.state {
                                    match loaded.player.is_paused() || loaded.player.empty() {
                                        true => draw_play_button(ui, self),
                                        false => draw_pause_button(ui, self),
                                    }
                                } else {
                                    draw_play_button(ui, self);
                                }
                            }

                            ui.horizontal(|ui| {
                                let elapsed = self.player.get_elapsed_secs() as u64;
                                let total = self.player.get_total_secs() as u64;
                                ui.label(format!("{:02}:{:02}", elapsed / 60, elapsed % 60));
                                ui.with_layout(
                                    egui::Layout::right_to_left(egui::Align::Center),
                                    |ui| {
                                        ui.label(format!("{:02}:{:02}", total / 60, total % 60));
                                    },
                                );
                            });
                        });
                    });
            });

        egui::CentralPanel::default()
            .frame(containers::Frame::central_panel(&style).inner_margin(Margin::ZERO))
            .show(ctx, |ui| {
                draw_playlist_table(ui, self);
            });
        self.auto_size_this_frame = false;
    }
}

impl Sorairo {}

fn main() -> eframe::Result {
    env_logger::init();

    let (app_tx, app_rx): (Sender<AppEvent>, Receiver<AppEvent>) = unbounded();
    let audio_handle = spawn_audio_thread(app_rx);
    let options = eframe::NativeOptions {
        viewport: egui::ViewportBuilder::default()
            .with_inner_size([640.0, 480.0])
            .with_min_inner_size([640.0, 480.0]),
        ..Default::default()
    };

    eframe::run_native(
        "Sorairo",
        options,
        Box::new(|cc| {
            // replace_fonts(&cc.egui_ctx);
            set_styles(&cc.egui_ctx);
            // This gives us image support:
            // egui_extras::install_image_loaders(&cc.egui_ctx);
            Ok(Box::new(Sorairo {
                main_view: AppMainView::Playlist,
                player: SorairoPlayer::default(),
                auto_size_this_frame: false,
            }))
        }),
    )?;

    app_tx.send(AppEvent::Exit).unwrap_or_else(|e| {
        log::error!("failed to send exit event: {e}");
    });
    if let Some(handle) = audio_handle {
        handle.thread.join().unwrap_or_else(|_e| {
            log::error!("failed to join audio thread");
        });
    }

    Ok(())
}

fn replace_fonts(ctx: &egui::Context) {
    let mut fonts = egui::FontDefinitions::default();
    fonts.font_data.insert(
        "segoeui".to_owned(),
        std::sync::Arc::new(egui::FontData::from_static(include_bytes!(
            "C:\\Windows\\Fonts\\segoeui.ttf"
        ))),
    );
    fonts
        .families
        .entry(egui::FontFamily::Proportional)
        .or_default()
        .insert(0, "segoeui".to_owned());
    fonts
        .families
        .entry(egui::FontFamily::Monospace)
        .or_default()
        .push("segoeui".to_owned());
    ctx.set_fonts(fonts);
    ctx.all_styles_mut(|style| {
        style.visuals.menu_corner_radius = egui::CornerRadius::ZERO;
        style.visuals.window_corner_radius = egui::CornerRadius::ZERO;
        style.visuals.widgets.noninteractive.corner_radius = egui::CornerRadius::ZERO;
        style.visuals.widgets.inactive.corner_radius = egui::CornerRadius::ZERO;
        style.visuals.widgets.hovered.corner_radius = egui::CornerRadius::ZERO;
        style.visuals.widgets.active.corner_radius = egui::CornerRadius::ZERO;
        style.visuals.widgets.open.corner_radius = egui::CornerRadius::ZERO;
        style.visuals.popup_shadow = Shadow {
            offset: [1, 1],
            blur: 8,
            spread: 0,
            color: Color32::from_hex("#00000011").expect("hex must be valid"),
        }
    });
    ctx.style_mut_of(egui::Theme::Light, |style| {
        style.visuals.window_fill = Color32::from_hex("#f3f3f3").expect("hex must be valid");
        style.visuals.panel_fill = Color32::from_hex("#f3f3f3").expect("hex must be valid");
        style.visuals.window_stroke = Stroke::new(
            1.0,
            Color32::from_hex("#c5c5c5").expect("hex must be valid"),
        );
        style.visuals.widgets.noninteractive.bg_stroke = Stroke::new(
            1.0,
            Color32::from_hex("#d5d5d5").expect("hex must be valid"),
        );
    });
    ctx.style_mut_of(egui::Theme::Dark, |style| {
        style.visuals.window_fill = Color32::from_hex("#202020").expect("hex must be valid");
        style.visuals.panel_fill = Color32::from_hex("#202020").expect("hex must be valid");
        style.visuals.window_stroke = Stroke::new(
            1.0,
            Color32::from_hex("#424242").expect("hex must be valid"),
        );
        style.visuals.widgets.noninteractive.bg_stroke = Stroke::new(
            1.0,
            Color32::from_hex("#323232").expect("hex must be valid"),
        );
    });
}

fn set_styles(ctx: &egui::Context) {
    ctx.all_styles_mut(|style| {
        style.visuals.menu_corner_radius = egui::CornerRadius::ZERO;
        style.visuals.window_corner_radius = egui::CornerRadius::ZERO;
        style.visuals.widgets.noninteractive.corner_radius = egui::CornerRadius::ZERO;
        style.visuals.widgets.inactive.corner_radius = egui::CornerRadius::ZERO;
        style.visuals.widgets.hovered.corner_radius = egui::CornerRadius::ZERO;
        style.visuals.widgets.active.corner_radius = egui::CornerRadius::ZERO;
        style.visuals.widgets.open.corner_radius = egui::CornerRadius::ZERO;
        style.visuals.popup_shadow = Shadow {
            offset: [1, 1],
            blur: 8,
            spread: 0,
            color: Color32::from_hex("#00000011").expect("hex must be valid"),
        }
    });
    ctx.style_mut_of(egui::Theme::Light, |style| {
        style.visuals.window_fill = Color32::from_hex("#f3f3f3").expect("hex must be valid");
        style.visuals.panel_fill = Color32::from_hex("#f3f3f3").expect("hex must be valid");
        style.visuals.window_stroke = Stroke::new(
            1.0,
            Color32::from_hex("#c5c5c5").expect("hex must be valid"),
        );
        style.visuals.widgets.noninteractive.bg_stroke = Stroke::new(
            1.0,
            Color32::from_hex("#d5d5d5").expect("hex must be valid"),
        );
    });
    ctx.style_mut_of(egui::Theme::Dark, |style| {
        style.visuals.window_fill = Color32::from_hex("#202020").expect("hex must be valid");
        style.visuals.panel_fill = Color32::from_hex("#202020").expect("hex must be valid");
        style.visuals.window_stroke = Stroke::new(
            1.0,
            Color32::from_hex("#424242").expect("hex must be valid"),
        );
        style.visuals.widgets.noninteractive.bg_stroke = Stroke::new(
            1.0,
            Color32::from_hex("#323232").expect("hex must be valid"),
        );
    });
}

fn draw_play_button(ui: &mut Ui, app: &mut Sorairo) {
    let desired_size = egui::vec2(20.0, 20.0);

    let (rect, response) = ui.allocate_exact_size(desired_size, egui::Sense::click());

    let visuals = ui.style().interact(&response);
    if response.hovered() {
        ui.painter().rect(
            rect,
            2.0,
            visuals.bg_fill,
            visuals.bg_stroke,
            egui::StrokeKind::Outside,
        );
    } else {
        ui.painter().rect(
            rect,
            2.0,
            visuals.bg_fill,
            visuals.bg_stroke,
            egui::StrokeKind::Outside,
        );
    }

    let center = rect.center();
    let w = rect.width() * 0.4;
    let h = rect.height() * 0.5;

    let points = vec![
        egui::pos2(center.x - w * 0.5, center.y - h * 0.5),
        egui::pos2(center.x - w * 0.5, center.y + h * 0.5),
        egui::pos2(center.x + w * 0.5, center.y),
    ];

    ui.painter().add(egui::Shape::convex_polygon(
        points,
        visuals.fg_stroke.color,
        egui::Stroke::NONE,
    ));

    if response.clicked() {
        if app.player.empty() {
            app.player.play();
        } else {
            app.player.resume();
        }
    }
}

fn draw_pause_button(ui: &mut Ui, app: &Sorairo) {
    let size = egui::vec2(20.0, 20.0);
    let (rect, response) = ui.allocate_exact_size(size, egui::Sense::click());
    let visuals = ui.style().interact(&response);
    ui.painter().rect(
        rect,
        2.0,
        visuals.bg_fill,
        visuals.bg_stroke,
        egui::StrokeKind::Outside,
    );

    let painter = ui.painter();
    let center = rect.center();
    let bar_width = rect.width() * 0.18;
    let bar_height = rect.height() * 0.5;
    let spacing = rect.width() * 0.08;

    let left_rect = egui::Rect::from_center_size(
        egui::pos2(center.x - spacing - bar_width * 0.5, center.y),
        egui::vec2(bar_width, bar_height),
    );

    let right_rect = egui::Rect::from_center_size(
        egui::pos2(center.x + spacing + bar_width * 0.5, center.y),
        egui::vec2(bar_width, bar_height),
    );

    painter.rect_filled(left_rect, 2.0, visuals.fg_stroke.color);
    painter.rect_filled(right_rect, 2.0, visuals.fg_stroke.color);

    if response.clicked() {
        app.player.pause();
    }
}

fn draw_playlist_table(ui: &mut Ui, app: &mut Sorairo) {
    TableBuilder::new(ui)
        .sense(Sense::click())
        .cell_layout(Layout::left_to_right(Align::Center))
        .column(Column::auto().at_least(28.0)) // column 1
        .column(
            Column::remainder()
                .auto_size_this_frame(app.auto_size_this_frame)
                .resizable(true)
                .clip(true),
        ) // column 1
        .column(
            Column::remainder()
                .auto_size_this_frame(app.auto_size_this_frame)
                .resizable(true)
                .clip(true),
        ) // column 1
        .column(
            Column::remainder()
                .auto_size_this_frame(app.auto_size_this_frame)
                .resizable(true)
                .clip(true),
        ) // column 2 (takes remaining space)
        .header(20.0, |mut header| {
            header.col(|ui| {
                ui.add_space(8.0);
                ui.weak("#");
            });
            header.col(|ui| {
                ui.weak("artist");
            });
            header.col(|ui| {
                ui.weak("title");
            });
            header.col(|ui| {
                ui.weak("file");
                ui.add_space(8.0);
            });
        })
        .body(|body| {
            body.rows(18.0, app.player.playlist.len(), |mut row| {
                let idx = row.index();
                let selected = app.player.get_current() == Some(idx);
                let item = &app.player.playlist[idx];
                row.set_selected(selected);
                row.col(|ui| {
                    ui.add_space(8.0);
                    ui.weak((idx + 1).to_string());
                });
                row.col(|ui| {
                    ui.label(item.artist.as_deref().unwrap_or_default());
                });
                row.col(|ui| {
                    ui.label(item.title.as_deref().unwrap_or_default());
                });
                row.col(|ui| {
                    ui.label(&item.file_name);
                    ui.add_space(8.0);
                });
                if row.response().clicked() {
                    app.player.set_current(Some(idx));
                    app.player.stop();
                    app.player.play();
                }
            });
        });
}

fn draw_playback_progress(ctx: &Context, ui: &mut Ui, app: &mut Sorairo, height: f32) {
    if let PlayerState::Loaded(loaded) = &app.player.state {
        if !loaded.player.is_paused() {
            ctx.request_repaint_after(Duration::from_millis(33));
        }
        let (rect, resp) =
            ui.allocate_exact_size(vec2(ui.available_width(), height), Sense::click_and_drag());
        let padding = 0.0;
        let active_rect = rect.shrink(padding);
        let progress = (app.player.get_elapsed_secs() / app.player.get_total_secs()) as f32;
        let progress_width = active_rect.width() * progress;
        let progress_rect = Rect::from_min_max(
            active_rect.min,
            egui::pos2(active_rect.left() + progress_width, active_rect.bottom()),
        );

        ui.painter()
            .rect_filled(rect, 0.0, ui.visuals().widgets.inactive.bg_fill);
        if resp.hovered() {
            ui.painter()
                .rect_filled(progress_rect, 0.0, Color32::from_rgb(106, 195, 254));
        } else {
            ui.painter()
                .rect_filled(progress_rect, 0.0, Color32::from_rgb(106, 195, 254));
        }
        if resp.dragged() || resp.clicked() {
            if let Some(pointer_pos) = resp.interact_pointer_pos() {
                let percent =
                    ((pointer_pos.x - active_rect.left()) / active_rect.width()).clamp(0.0, 1.0);
                let new_time = percent as f64 * app.player.get_total_secs();
                app.player.seek(new_time);
            }
        }
    }
}

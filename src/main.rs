mod player;

use std::time::Duration;

use eframe::egui::{self, Color32, CornerRadius, Rect, RichText, Sense, Shadow, Stroke, Ui, vec2};
use egui_extras::{Column, TableBuilder};
use player::{PlayerState, SorairoPlayer};

#[derive(Default)]
struct Sorairo {
    file_path: Option<String>,
    player: SorairoPlayer,
}

impl eframe::App for Sorairo {
    fn update(&mut self, ctx: &eframe::egui::Context, frame: &mut eframe::Frame) {
        let playlist_size = self.player.playlist.len();
        if playlist_size > 0
            && let PlayerState::Loaded(loaded) = &self.player.state
        {
            if loaded.player.empty() {
                self.player.current_index = (self.player.current_index + 1) % playlist_size;
                self.player.play();
            }
        }
        egui::TopBottomPanel::top("top_panel").show(ctx, |ui| {
            egui::MenuBar::new().ui(ui, |ui| {
                ui.menu_button("File", |ui| {
                    if ui.button("Open").clicked()
                        && let Some(path) = rfd::FileDialog::new()
                            .add_filter("audio", &["mp3", "wav"])
                            .pick_file()
                    {
                        let path = path.display().to_string();
                        self.player.clear();
                        self.player.add_file(path.clone());
                        self.player.play();
                        self.file_path = Some(path);
                    }
                    ui.separator();
                    if ui.button("Add files").clicked()
                        && let Some(paths) = rfd::FileDialog::new()
                            .add_filter("audio", &["mp3", "wav"])
                            .pick_files()
                    {
                        for path in &paths {
                            self.player.add_file(path.display().to_string());
                        }
                    }
                    if ui.button("Quit").clicked() {
                        ui.ctx().send_viewport_cmd(egui::ViewportCommand::Close);
                    }
                });
            })
        });
        egui::TopBottomPanel::bottom("bottom_panel").show(ctx, |ui| {
            if let PlayerState::Loaded(loaded) = &self.player.state {
                if !loaded.player.is_paused() {
                    ctx.request_repaint_after(Duration::from_millis(33));
                }

                ui.horizontal(|ui| {
                    let elapsed = self.player.get_elapsed_secs() as u64;
                    let total = self.player.get_total_secs() as u64;
                    ui.label(format!("{:02}:{:02}", elapsed / 60, elapsed % 60));
                    ui.with_layout(egui::Layout::right_to_left(egui::Align::Center), |ui| {
                        ui.label(format!("{:02}:{:02}", total / 60, total % 60));
                    });
                });

                let (rect, resp) = ui
                    .allocate_exact_size(vec2(ui.available_width(), 8.0), Sense::click_and_drag());
                let padding = 2.0;
                let active_rect = rect.shrink(padding);
                let progress =
                    (self.player.get_elapsed_secs() / self.player.get_total_secs()) as f32;
                let progress_width = active_rect.width() * progress;
                let progress_rect = Rect::from_min_max(
                    active_rect.min,
                    egui::pos2(active_rect.left() + progress_width, active_rect.bottom()),
                );

                ui.painter()
                    .rect_filled(rect, 2.0, ui.visuals().widgets.inactive.bg_fill);
                if resp.hovered() {
                    ui.painter().rect_filled(
                        progress_rect,
                        2.0,
                        ui.visuals().widgets.hovered.fg_stroke.color,
                    );
                } else {
                    ui.painter().rect_filled(
                        progress_rect,
                        2.0,
                        ui.visuals().widgets.inactive.fg_stroke.color,
                    );
                }
                if resp.dragged() || resp.clicked() {
                    if let Some(pointer_pos) = resp.interact_pointer_pos() {
                        let percent = ((pointer_pos.x - active_rect.left()) / active_rect.width())
                            .clamp(0.0, 1.0);
                        let new_time = percent as f64 * self.player.get_total_secs();
                        self.player.seek(new_time);
                    }
                }
            }
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
        });
        egui::CentralPanel::default().show(ctx, |ui| {
            draw_playlist_table(ui, self);
        });
    }
}

impl Sorairo {}

fn main() -> eframe::Result {
    env_logger::init();
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
            replace_fonts(&cc.egui_ctx);
            // This gives us image support:
            // egui_extras::install_image_loaders(&cc.egui_ctx);

            Ok(Box::<Sorairo>::default())
        }),
    )
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
        style.visuals.menu_corner_radius = CornerRadius::ZERO;
        style.visuals.window_corner_radius = CornerRadius::ZERO;
        style.visuals.widgets.hovered.corner_radius = CornerRadius::ZERO;
        style.visuals.widgets.active.corner_radius = CornerRadius::ZERO;
        style.visuals.widgets.open.corner_radius = CornerRadius::ZERO;
        style.visuals.widgets.inactive.corner_radius = CornerRadius::ZERO;
        style.visuals.widgets.noninteractive.corner_radius = CornerRadius::ZERO;
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
        .column(Column::auto().at_least(28.0)) // column 1
        .column(Column::remainder()) // column 2 (takes remaining space)
        .header(20.0, |mut header| {
            header.col(|ui| {
                ui.weak("#");
            });
            header.col(|ui| {
                ui.weak("Title");
            });
        })
        .body(|body| {
            body.rows(18.0, app.player.playlist.len(), |mut row| {
                let idx = row.index();
                let selected = app.player.current_index == idx;
                row.set_selected(selected);
                row.col(|ui| {
                    ui.weak((idx + 1).to_string());
                });
                row.col(|ui| {
                    let mut rich_text = RichText::new(&app.player.playlist[idx]);
                    if selected {
                        rich_text = rich_text.strong();
                    }
                    ui.label(rich_text);
                });
                if row.response().clicked() {
                    app.player.current_index = idx;
                    app.player.stop();
                    app.player.play();
                }
            });
        });
}

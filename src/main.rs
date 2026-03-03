mod player;

use player::{PlayerState, SorairoPlayer};
use eframe::egui::{self};

#[derive(Default)]
struct Sorairo {
    file_path: Option<String>,
    player: SorairoPlayer,
}

impl eframe::App for Sorairo {
    fn update(&mut self, ctx: &eframe::egui::Context, frame: &mut eframe::Frame) {
        egui::CentralPanel::default().show(ctx, |ui| {
            if ui.button("Select file").clicked()
                && let Some(path) = rfd::FileDialog::new().pick_file()
            {
                let path = path.display().to_string();
                self.player.clear();
                self.player.play(&path);
                self.file_path = Some(path);
            }

            if let Some(path) = &self.file_path {
                ui.label(path);
            }

            if let PlayerState::Playing(_) = self.player.get_state() {
                ui.label(format!("time: {}", self.player.get_elapsed_secs()));
                ui.label(format!("volume = {}", self.player.get_volume()));
                ctx.request_repaint_after(std::time::Duration::from_millis(100));
            }
        });
    }
}

impl Sorairo {}

fn main() -> eframe::Result {
    env_logger::init();
    let options = eframe::NativeOptions {
        viewport: egui::ViewportBuilder::default().with_inner_size([320.0, 240.0]),
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
    // Start with the default fonts (we will be adding to them rather than replacing them).
    let mut fonts = egui::FontDefinitions::default();

    // Install my own font (maybe supporting non-latin characters).
    // .ttf and .otf files supported.
    fonts.font_data.insert(
        "segoeui".to_owned(),
        std::sync::Arc::new(egui::FontData::from_static(include_bytes!(
            "C:\\Windows\\Fonts\\segoeui.ttf"
        ))),
    );

    // Put my font first (highest priority) for proportional text:
    fonts
        .families
        .entry(egui::FontFamily::Proportional)
        .or_default()
        .insert(0, "segoeui".to_owned());

    // Put my font as last fallback for monospace:
    fonts
        .families
        .entry(egui::FontFamily::Monospace)
        .or_default()
        .push("segoeui".to_owned());

    // Tell egui to use these fonts:
    ctx.set_fonts(fonts);
}

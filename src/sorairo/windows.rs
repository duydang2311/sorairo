use std::thread::JoinHandle;

use crossbeam_channel::{Receiver, Sender, unbounded};
use windows::Win32::{
    Foundation::PROPERTYKEY,
    Media::Audio::{
        DEVICE_STATE, EDataFlow, ERole, IMMDeviceEnumerator, IMMNotificationClient,
        IMMNotificationClient_Impl, MMDeviceEnumerator,
    },
    System::Com::{
        CLSCTX_ALL, COINIT_MULTITHREADED, CoCreateInstance, CoInitializeEx, CoUninitialize,
    },
};
use windows_core::{PCWSTR, implement};

use crate::sorairo::{app::AppEvent, audio::AudioEvent};

pub fn spawn_audio_thread(
    app_rx: Receiver<AppEvent>,
) -> (JoinHandle<()>, Sender<AudioEvent>, Receiver<AudioEvent>) {
    let (tx, rx): (Sender<AudioEvent>, Receiver<AudioEvent>) = unbounded();
    let tx_clone = tx.clone();
    let handle = std::thread::spawn(move || unsafe {
        CoInitializeEx(None, COINIT_MULTITHREADED).ok().expect("");
        let enumerator: IMMDeviceEnumerator =
            CoCreateInstance(&MMDeviceEnumerator, None, CLSCTX_ALL).unwrap();
        let client: IMMNotificationClient = DeviceWatcher { tx: tx_clone }.into();
        enumerator
            .RegisterEndpointNotificationCallback(&client)
            .unwrap();
        loop {
            if let Ok(AppEvent::Exit) = app_rx.try_recv() {
                break;
            }
            std::thread::park_timeout(std::time::Duration::from_secs(1));
        }

        CoUninitialize();
    });
    return (handle, tx, rx);
}

#[implement(IMMNotificationClient)]
struct DeviceWatcher {
    tx: Sender<AudioEvent>,
}

impl IMMNotificationClient_Impl for DeviceWatcher_Impl {
    fn OnDeviceStateChanged(
        &self,
        _pwstrdeviceid: &PCWSTR,
        _dwnewstate: DEVICE_STATE,
    ) -> windows_core::Result<()> {
        Ok(())
    }

    fn OnDeviceAdded(&self, _pwstrdeviceid: &PCWSTR) -> windows_core::Result<()> {
        Ok(())
    }

    fn OnDeviceRemoved(&self, _pwstrdeviceid: &PCWSTR) -> windows_core::Result<()> {
        Ok(())
    }

    fn OnDefaultDeviceChanged(
        &self,
        _flow: EDataFlow,
        _role: ERole,
        _pwstrdefaultdeviceid: &PCWSTR,
    ) -> windows_core::Result<()> {
        self.tx.send(AudioEvent::DeviceChanged).unwrap_or_else(|e| {
            log::error!("failed to send AudioEvent::DeviceChanged event: {e}");
        });
        Ok(())
    }

    fn OnPropertyValueChanged(
        &self,
        _pwstrdeviceid: &PCWSTR,
        _key: &PROPERTYKEY,
    ) -> windows_core::Result<()> {
        Ok(())
    }
}

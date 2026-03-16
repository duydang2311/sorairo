use std::{
    ffi::{CString, c_char, c_int, c_void},
    fmt::Display,
    mem::MaybeUninit,
    sync::atomic::{AtomicU32, AtomicU64},
};

#[repr(i32)]
#[derive(Debug, Copy, Clone, PartialEq, Eq)]
pub enum MaResult {
    Success = 0,
    Error = -1,
    InvalidArgs = -2,
    InvalidOperation = -3,
    OutOfMemory = -4,
    OutOfRange = -5,
    AccessDenied = -6,
    DoesNotExist = -7,
    AlreadyExists = -8,
    TooManyOpenFiles = -9,
    InvalidFile = -10,
    TooBig = -11,
    PathTooLong = -12,
    NameTooLong = -13,
    NotDirectory = -14,
    IsDirectory = -15,
    DirectoryNotEmpty = -16,
    AtEnd = -17,
    NoSpace = -18,
    Busy = -19,
    IoError = -20,
    Interrupt = -21,
    Unavailable = -22,
    AlreadyInUse = -23,
    BadAddress = -24,
    BadSeek = -25,
    BadPipe = -26,
    Deadlock = -27,
    TooManyLinks = -28,
    NotImplemented = -29,
    NoMessage = -30,
    BadMessage = -31,
    NoDataAvailable = -32,
    InvalidData = -33,
    Timeout = -34,
    NoNetwork = -35,
    NotUnique = -36,
    NotSocket = -37,
    NoAddress = -38,
    BadProtocol = -39,
    ProtocolUnavailable = -40,
    ProtocolNotSupported = -41,
    ProtocolFamilyNotSupported = -42,
    AddressFamilyNotSupported = -43,
    SocketNotSupported = -44,
    ConnectionReset = -45,
    AlreadyConnected = -46,
    NotConnected = -47,
    ConnectionRefused = -48,
    NoHost = -49,
    InProgress = -50,
    Cancelled = -51,
    MemoryAlreadyMapped = -52,

    CrcMismatch = -100,

    FormatNotSupported = -200,
    DeviceTypeNotSupported = -201,
    ShareModeNotSupported = -202,
    NoBackend = -203,
    NoDevice = -204,
    ApiNotFound = -205,
    InvalidDeviceConfig = -206,
    Loop = -207,
    BackendNotEnabled = -208,

    DeviceNotInitialized = -300,
    DeviceAlreadyInitialized = -301,
    DeviceNotStarted = -302,
    DeviceNotStopped = -303,

    FailedToInitBackend = -400,
    FailedToOpenBackendDevice = -401,
    FailedToStartBackendDevice = -402,
    FailedToStopBackendDevice = -403,
}

impl Display for MaResult {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        write!(f, "{:?}", self)
    }
}

pub type ma_uint32 = u32;
pub type ma_uint64 = u64;
pub type ma_bool8 = u8;
pub type ma_bool32 = u32;
pub type ma_sound_end_proc =
    Option<unsafe extern "C" fn(p_sound: *mut ma_sound, user_data: *mut c_void)>;

#[repr(C, align(8))]
#[derive(Debug, Copy, Clone)]
pub struct ma_engine {
    _private: [u8; 1360],
}

#[repr(C)]
pub struct ma_engine_config {
    _private: [u8; 0],
}

#[repr(C)]
pub struct ma_engine_node {
    _private: [u8; 0],
}

#[repr(C)]
pub struct ma_data_source {
    _private: [u8; 0],
}

#[repr(C)]
pub struct ma_resource_manager_data_source {
    _private: [u8; 0],
}

#[repr(C)]
pub struct ma_sound_group {
    _private: [u8; 0],
}

#[repr(C)]
pub struct ma_sound {
    _private: [u8; 0],
}

unsafe extern "C" {
    pub unsafe fn ma_engine_config_init() -> ma_engine_config;
    pub unsafe fn ma_engine_init(
        config: *const ma_engine_config,
        engine: *mut ma_engine,
    ) -> MaResult;
    pub unsafe fn ma_engine_uninit(engine: *mut ma_engine);
    pub unsafe fn ma_engine_play_sound(
        engine: *mut ma_engine,
        path: *const c_char,
        group: *mut ma_sound_group,
    ) -> MaResult;
}

use std::path::PathBuf;

// Windows: %appdata%/terraprisma
// macOS: ~/Library/Application Support/terraprisma
// Linux: $XDG_DATA_HOME/terraprisma or ~/.local/share/terraprisma
pub fn get_terraprisma_data_dir() -> PathBuf {
    let dir = inner_get_terraprisma_data_dir();

    if !dir.exists() {
        std::fs::create_dir_all(&dir).unwrap();
    }

    return dir;
}

#[cfg(target_os = "windows")]
fn inner_get_terraprisma_data_dir() -> PathBuf {
    // %appdata% will always exist, this is a Windows guarantee. If it doesn't,
    // something else is wrong.
    let app_data = std::env::var("APPDATA").unwrap();

    return PathBuf::from(app_data).join("terraprisma");
}

#[cfg(target_os = "macos")]
fn inner_get_terraprisma_data_dir() -> PathBuf {
    let home = std::env::var("HOME").unwrap();

    return PathBuf::from(home).join("Library/Application Support/terraprisma");
}

#[cfg(target_os = "linux")]
fn inner_get_terraprisma_data_dir() -> PathBuf {
    let home = std::env::var("HOME").unwrap();
    let xdg_data_home =
        std::env::var("XDG_DATA_HOME").unwrap_or_else(|_| format!("{}/.local/share", home));

    return PathBuf::from(xdg_data_home).join("terraprisma");
}

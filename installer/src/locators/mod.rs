mod steam;

use std::path::PathBuf;

pub fn locate_game_install_path() -> Option<PathBuf> {
    // TODO: GOG support, return a vector of resolved paths, handle that too.
    steam::check_steam()
}

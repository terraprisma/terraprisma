use colored::Colorize;
use std::path::PathBuf;

mod install;
mod paths;

const NAME: &str = env!("CARGO_PKG_NAME");
const VERSION: &str = env!("CARGO_PKG_VERSION");

fn main() {
    // check if terraprisma.config.json exists

    println!(
        "{}, Terraria-focused Terraprisma installer and launcher; v{}",
        NAME, VERSION
    );
    println!();

    let data_dir = paths::get_terraprisma_data_dir();
    println!(
        "{} {}",
        "Using data directory:".bright_black(),
        data_dir.display().to_string().bright_black()
    );
    println!();

    let args = std::env::args().collect::<Vec<String>>();

    // If no arguments are passed, we'll initiate the installation process.
    if args.len() == 1 {
        println!(
            "{}",
            "No arguments passed, initiating installation process...".bright_black()
        );
        println!();
        let install_path = install::prompt_for_installation_path();
        println!();
        let path: PathBuf;
        match install_path {
            install::InstallationPathType::Steam(path_buf) => {
                println!(
                    "{} {}",
                    "Using Steam installation path:".bright_black(),
                    path_buf.display()
                );
                path = path_buf;
            }
            install::InstallationPathType::Gog(path_buf) => {
                println!(
                    "{} {}",
                    "Using GOG installation path:".bright_black(),
                    path_buf.display()
                );
                path = path_buf;
            }
            install::InstallationPathType::ManualNotSpecified() => {
                panic!(
                    "ManualNotSpecified should not be returned from prompt_for_installation_path"
                );
            }
            install::InstallationPathType::Manual(path_buf) => {
                println!(
                    "{} {}",
                    "Using manually-specified installation path:".bright_black(),
                    path_buf.display()
                );
                path = path_buf;
            }
        }

        println!();
        println!("{}", "Determining installation type...".bright_black());
        println!();

        let install_type = install::prompt_install_type();

        println!();
        println!(
            "Installing game version '{}' using installation @ '{}'...",
            install_type,
            path.display()
        );
        return;
    }

    // If arguments are passed, we'll assume this binary and process is being
    // used to launch the game.
    let terraprisma_config_path = std::path::Path::new("terraprisma.config.json");
    if terraprisma_config_path.exists() {
        // Since a config is already present, we'll just use this.
        let json = std::fs::read_to_string(terraprisma_config_path).unwrap();
    }

    println!();
    println!("terraprisma.config.json not found, performing first-time setup...");

    // let install_type = install::prompt_install_type();
}

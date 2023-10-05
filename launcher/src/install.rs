use colored::Colorize;
use serde::{Deserialize, Serialize};
use std::{
    fmt::Display,
    path::{Path, PathBuf},
};
use steamlocate::SteamDir;

use inquire::{
    required,
    validator::{StringValidator, Validation},
    CustomUserError, Select,
};

const TERARRIA_APPID: &u32 = &105600;

#[derive(Serialize, Deserialize)]
pub struct InstallConfig {
    install_type: InstallType,
    game_path: String,
}

#[derive(Serialize, Deserialize)]
pub enum InstallType {
    TerrariaNetFramework40,
    TerrariaNet7,
}

impl Display for InstallType {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        match self {
            InstallType::TerrariaNetFramework40 => {
                write!(f, "Terraria: Vanilla, uses .NET Framework 4.0")
            }
            InstallType::TerrariaNet7 => write!(f, "Terraria: patched, uses .NET 7.0"),
        }
    }
}

pub enum InstallationPathType {
    Steam(PathBuf),
    Gog(PathBuf),
    ManualNotSpecified(),
    Manual(PathBuf),
}

impl Display for InstallationPathType {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        match self {
            InstallationPathType::Steam(path) => write!(f, "Steam: {}", path.display()),
            InstallationPathType::Gog(path) => write!(f, "GOG: {}", path.display()),
            InstallationPathType::ManualNotSpecified() => write!(f, "Manually input a path..."),
            InstallationPathType::Manual(path) => {
                write!(f, "Manually-input path: {}", path.display())
            }
        }
    }
}

#[derive(Clone)]
struct PathExistsValidator {
    message: String,
}

impl PathExistsValidator {
    pub fn new(message: impl Into<String>) -> Self {
        Self {
            message: message.into(),
        }
    }
}

impl StringValidator for PathExistsValidator {
    fn validate(&self, input: &str) -> Result<Validation, CustomUserError> {
        Ok(if !Path::new(input).exists() {
            Validation::Invalid(self.message.as_str().into())
        } else {
            Validation::Valid
        })
    }
}

#[derive(Clone)]
struct PathIsDirectoryValidator {
    message: String,
}

impl PathIsDirectoryValidator {
    pub fn new(message: impl Into<String>) -> Self {
        Self {
            message: message.into(),
        }
    }
}

impl StringValidator for PathIsDirectoryValidator {
    fn validate(&self, input: &str) -> Result<Validation, CustomUserError> {
        Ok(if !Path::new(input).is_dir() {
            Validation::Invalid(self.message.as_str().into())
        } else {
            Validation::Valid
        })
    }
}

pub fn prompt_for_installation_path() -> InstallationPathType {
    println!(
        "{}",
        "Attempting to auto-detect Terraria installations...".bright_black()
    );

    let options = resolve_installation_paths();

    // Subtract 1 because the last option is ManualNotSpecified.
    println!(
        "{} {} {}",
        "Found".bright_black(),
        options.len() - 1,
        "installation(s).".bright_black()
    );
    println!();

    let ans = Select::new("What Terraria installation would you like to use?", options).prompt();

    let path_type = match ans {
        Ok(ans) => ans,
        Err(_) => {
            println!("Failed to get answer, exiting.");
            std::process::exit(1);
        }
    };

    if matches!(path_type, InstallationPathType::ManualNotSpecified()) {
        let ans = inquire::Text::new("Please input the path to your Terraria installation: ")
            .with_validator(required!("You must specify a path!"))
            .with_validator(PathExistsValidator::new(
                "The specified path does not exist!",
            ))
            .with_validator(PathIsDirectoryValidator::new(
                "The specified path is not a directory!",
            ))
            .prompt();

        match ans {
            Ok(ans) => return InstallationPathType::Manual(Path::new(&ans).to_path_buf()),
            Err(_) => {
                println!("Failed to get answer, exiting.");
                std::process::exit(1);
            }
        }
    }

    return path_type;
}

fn resolve_installation_paths() -> Vec<InstallationPathType> {
    let steam_dir = resolve_steam_path();
    if steam_dir.is_some() {
        println!("{}", "Found Steam installation!".bright_black());
    } else {
        println!("{}", "No Steam installation found!".bright_black());
    }

    println!(
        "{}",
        "GOG installations are currently not auto-detected.".bright_black()
    );

    let mut options: Vec<InstallationPathType> = vec![];
    if let Some(steam_dir) = steam_dir {
        options.push(InstallationPathType::Steam(steam_dir));
    }

    options.push(InstallationPathType::ManualNotSpecified());

    options
}

// TODO: Rewrite this to not use steam-locator, it uses an outtdated crate and
// could check some more registry keys on Windows.
fn resolve_steam_path() -> Option<PathBuf> {
    let steam_dir = SteamDir::locate();
    match steam_dir {
        Some(mut steam_dir) => match steam_dir.app(TERARRIA_APPID) {
            Some(app) => return Some(app.path.clone()),
            None => return None,
        },
        None => return None,
    };
}

pub fn prompt_install_type() -> InstallType {
    println!("Terraprisma is a generalized .NET mod loader, this installation is specifically tailored to Terraria.");
    println!("Terraria uses .NET Framework 4.0 (Mono on non-Windows systems), but modifications such as tModLoader and TShock update to .NET 6.0 or later.");
    println!("Depending on the mods you'd like to install, you'll have to choose what version of Terraria you'd like to use.");
    println!("If you're unsure, consult the READMEs, documentation, websites, installation pages, etc. of the mods you want to use.");
    println!("If you want compatibility with TShock, tModLoader, etc., use .NET 7.0.");
    println!();

    let ans = Select::new(
        "What version of Terraria would you like to use?",
        vec![
            InstallType::TerrariaNetFramework40,
            InstallType::TerrariaNet7,
        ],
    )
    .prompt();

    return match ans {
        Ok(ans) => ans,
        Err(_) => {
            println!("Failed to get answer, exiting.");
            std::process::exit(1);
        }
    };
}

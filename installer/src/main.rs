mod locators;

use std::{
    env, fs,
    io::{stdin, stdout, Write},
    path::PathBuf,
};

use crate::{dotnet::check_dotnet, locators::locate_game_install_path};

mod dotnet;

const NAME: &str = env!("CARGO_PKG_NAME");
const VERSION: &str = env!("CARGO_PKG_VERSION");

// TODO better handling of errors - print them into a log and exit instead of panic!
// TODO app icon
fn main() {
    let dotnet_version = 7;

    println!("{} {}", NAME, format!("v{}", VERSION));

    // TODO GOG support
    let install_path = locate_game_install_path();

    // TODO: This mostly oes for the user-specific path, but we should verify
    // that the path we're using is actually a valid Terraria install. We at
    // least need to care about if the Terraria binary is there, as well as a
    // content folder (even more important, arguably). Maybe this is a job for
    // the Terraprisma launcher instead?
    match install_path {
        None => {
            println!("A Terraria installation could not be automatically resolved.");
            let mut user_path =
                prompt_string("Please enter the path to your Terraria installation: ");
            // TODO: Handle if the user enters their Terraria.exe path instead
            // of Terraria directory path.
            while !PathBuf::from(&user_path).exists() || !PathBuf::from(&user_path).is_dir() {
                println!("The path '{}' does not exist.", user_path);
                user_path = prompt_string("Please enter the path to your Terraria installation: ");
            }
            install_launcher_to_path(PathBuf::from(user_path), dotnet_version);
        }
        Some(terraria_path) => {
            install_launcher_to_path(terraria_path, dotnet_version);
        }
    }
}

fn prompt_bool(message: &str) -> bool {
    print!("{message} (y/n): ");
    stdout().flush().unwrap();
    let mut buf = String::with_capacity(1);
    stdin().read_line(&mut buf).unwrap();

    match buf.to_lowercase()[0..buf.len() - 2].as_ref() {
        "y" | "yes" => true,
        "n" | "no" => false,
        _ => {
            println!("Invalid input. Please type \"y\" for yes or \"n\" for no");
            return prompt_bool(message);
        }
    }
}

fn prompt_string(message: &str) -> String {
    print!("{}", message);
    stdout().flush().unwrap();

    let mut input = String::new();
    stdin().read_line(&mut input).unwrap();

    input.trim().to_owned()
}

fn install_launcher_to_path(mut path: PathBuf, dotnet_version: i32) {
    println!("Determined installation location '{}'.", path.display());

    path.push("Terraprisma");
    println!("Installing to '{}'.", path.display());

    fs::create_dir_all(&path).unwrap();
    env::set_current_dir(path).unwrap();

    let dotnet_path = check_dotnet(&dotnet_version).unwrap();
    println!("{}", dotnet_path.display());
}

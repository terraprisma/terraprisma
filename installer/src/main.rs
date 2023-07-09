use std::{env, fs, io::{stdin, stdout, Write}};

use crate::{dotnet::check_dotnet, steam::check_steam};

mod dotnet;
mod steam;

const NAME: &str = env!("CARGO_PKG_NAME");
const VERSION: &str = env!("CARGO_PKG_VERSION");

// TODO better handling of errors - print them into a log and exit instead of panic!
// TODO app icon
fn main() {
    let dotnet_version = 7;

    println!("{} {}", NAME, format!("v{}", VERSION));

    // TODO GOG support
    let mut terraria_path = check_steam();
    println!("{}", terraria_path.display());

    terraria_path.push("Terraprisma");
    fs::create_dir_all(&terraria_path).unwrap();
    env::set_current_dir(terraria_path).unwrap();

    let dotnet_path = check_dotnet(&dotnet_version).unwrap();
    println!("{}", dotnet_path.display());
}

fn prompt(message: &str) -> bool {
    print!("{message} (y/n): ");
    stdout().flush().unwrap(); 
    let mut buf = String::with_capacity(1);
    stdin().read_line(&mut buf).unwrap();

    match buf.to_lowercase()[0..buf.len() - 2].as_ref() {
        "y" | "yes" => true,
        "n" | "no" => false,
        _ => {
            println!("Invalid input. Please type \"y\" for yes or \"n\" for no");
            return prompt(message);
        },
    }
}
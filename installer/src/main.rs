use std::{env, fs};

use crate::{dotnet::check_dotnet, steam::check_steam};

mod dotnet;
mod steam;

const NAME: &str = env!("CARGO_PKG_NAME");
const VERSION: &str = env!("CARGO_PKG_VERSION");

fn main() {
    let dotnet_version = 7;

    println!("{}{}", NAME, format!("v{}", VERSION));

    let mut terraria_path = check_steam();
    println!("{}", terraria_path.display());

    terraria_path.push("Terraprisma");
    fs::create_dir_all(&terraria_path).unwrap();
    env::set_current_dir(terraria_path).unwrap();

    let dotnet_path = check_dotnet(&dotnet_version);
    println!("{}", dotnet_path.display());
}

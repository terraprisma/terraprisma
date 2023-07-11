use std::env;

use netcorehost::{nethost, pdcstr};

mod dotnet;

const NAME: &str = env!("CARGO_PKG_NAME");
const VERSION: &str = env!("CARGO_PKG_VERSION");

const DOTNET_VERSION: i32 = 7;

// TODO better handling of errors - print them into a log and exit instead of panic!
// TODO app icon
fn main() {
    println!("{} {}", NAME, format!("v{}", VERSION));

    // TODO: Let the user override the install/expected local path with an
    // argument?
    let mut net_install_path = env::current_dir().unwrap();
    net_install_path.push("dotnet");

    let dotnet_global_installed = dotnet::check_global_dotnet(DOTNET_VERSION);
    if dotnet_global_installed {
        print!("Global .NET {} runtime found", DOTNET_VERSION);
    } else {
        // Assume that if the local dir exists, it's been installed.
        // Hopefully this assumption is safe.
        if !net_install_path.exists() {
            print!("Local .NET {} runtime not found", DOTNET_VERSION);
            match dotnet::download_and_extract_dotnet_runtime(
                DOTNET_VERSION,
                net_install_path.to_str().unwrap(),
            ) {
                Ok(_) => println!(" - installed"),
                Err(e) => panic!(" - failed to install: {:?}", e),
            }
        } else {
            println!("Local .NET {} runtime found", DOTNET_VERSION);
        }

        env::set_var("DOTNET_ROOT", net_install_path.to_str().unwrap());
    }

    // If DOTNET_ROOT is set, print it out.
    match env::var("DOTNET_ROOT") {
        Ok(_) => println!("Using .NET runtime at {}", env::var("DOTNET_ROOT").unwrap()),
        Err(_) => {}
    }

    if !std::path::Path::new("./bin/Terraprisma.Launcher.dll").exists() {
        panic!("Assembly not found");
    }

    let hostfxr = nethost::load_hostfxr().unwrap();
    let context = hostfxr
        .initialize_for_dotnet_command_line(pdcstr!("./bin/Terraprisma.Launcher.dll"))
        .unwrap();
    context.run_app().as_hosting_exit_code().unwrap();
}

use std::{str, process::Command, path::PathBuf, env, io::{Read, Cursor}}    ;

use flate2::read::{GzDecoder};
use regex::bytes::Regex;

use which::{which_global, which_in};

const VERSION: Option<&str> = option_env!("CARGO_PKG_VERSION");

fn main() {
    let dotnet_version = 7;

    println!("terraprisma-bootstrapper{}", match VERSION {
        Some(v) => format!(" v{}", v),
        None => format!(", unknown version"),
    });

    let dotnet_path = check_dotnet(&dotnet_version);
    println!("{}", dotnet_path.display());
}

// TODO check existing local .NET runtime?
fn check_dotnet(major: &i32) -> PathBuf {
    match which_global("dotnet") {
        Ok(path) => {
            match get_dotnet_runtimes(major) {
                Some(vers) => {
                    println!("{} compatible runtime(s) installed: ", vers.len());
                    for rt in vers.iter() {
                        println!("  {}", rt);
                    }

                    return path;
                },
                None => {
                    println!("Microsoft.NETCore.App {} runtime not found. Installing self contained runtime...", major);
                    download_and_extract_dotnet_runtime(major)
                }
            }
        },
        Err(_) => {
            println!("dotnet not found on PATH. Installing self contained runtime...");
            download_and_extract_dotnet_runtime(major)
        },
    }
}

fn get_dotnet_runtimes(major: &i32) -> Option<Vec<String>> {
    let dotnet_list_runtimes = Command::new("dotnet")
        .args(["--list-runtimes"])
        .output().unwrap();

    if !dotnet_list_runtimes.status.success() {
        println!(".NET {} not found", major);
        return None;
    };

    let pattern = format!(r"Microsoft.NETCore.App {}\.[0-9]+\.[0-9]+", major);
    let regex = Regex::new(&pattern[..]).unwrap();

    if !regex.is_match(&dotnet_list_runtimes.stdout) {
        return None;
    }

    let mut runtimes = Vec::<String>::new();

    for find in regex.find_iter(&dotnet_list_runtimes.stdout) {
        runtimes.push(str::from_utf8(find.as_bytes()).unwrap().to_string());
    }

    return Some(runtimes);
}

fn download_and_extract_dotnet_runtime(major: &i32) -> PathBuf {
    let version_url: String = version_url(major);
    let client = ureq::agent();

    match client.get(&
        version_url).call() {
        Err(e) => panic!("Error while fetching dotnet version ({}): {}", version_url, e),
        Ok(version) => {
            let version_text = version.into_string().unwrap();
            let runtime_url = runtime_url(&version_text);

            match client.get(&runtime_url).call() {
                Err(e) => panic!("Error while fetching dotnet runtime ({}): {}", runtime_url, e),
                Ok(runtime) => {
                    if runtime_url.ends_with("zip") {
                        // TODO can I do this just with flate2 and without reading into a buffer?
                        let mut buf = Vec::<u8>::new();
                        runtime.into_reader().read_to_end(&mut buf).unwrap();
                        let target_dir = PathBuf::from("dotnet");
                        zip_extract::extract(Cursor::new(buf), &target_dir, true).unwrap();
                    } else if runtime_url.ends_with(".tar.gz") {
                        let gzip = GzDecoder::new(runtime.into_reader());
                        let mut tar = tar::Archive::new(gzip);
                        tar.unpack("dotnet").unwrap();
                    }

                    return find_dotnet_in("dotnet").unwrap();
                }
            }
        },
    }
}

fn find_dotnet_in(path: &str) -> Option<PathBuf> {
    match which_in("dotnet", Some(path), env::current_dir().unwrap()) {
        Ok(buf) => Some(buf),
        Err(_) => None,
    }
}

const AZURE_FEED: &str = "https://dotnetcli.azureedge.net/dotnet";

fn version_url(major: &i32) -> String {
    // TODO is a minor version of not 0 ever used?
    return format!("{}/Runtime/{}.0/latest.version", AZURE_FEED, major);
}

fn runtime_url(specific_version: &String) -> String {
    format!(
        "{}/Runtime/{}/{}",
        AZURE_FEED, specific_version, get_runtime_dist_name(specific_version)
    )
}

fn get_runtime_dist_name(specific_version: &String) -> String {
    format!("dotnet-runtime-{}-{}-{}.{}", specific_version, dotnet_target_os!(), dotnet_target_arch!(),
    match dotnet_target_os!() {
        "win" => "zip",
        "osx" | "linux" => "tar.gz",
        s => panic!("Unknown OS {}", s),
    })
}

#[macro_export]
macro_rules! dotnet_target_os {
    () => {
        match env::consts::OS {
            "linux" => "linux",
            "macos" => "osx",
            "windows" => "win",
            s => panic!("Unknown os: {}", s),
        }
    };
}

#[macro_export]
macro_rules! dotnet_target_arch {
    () => {
        match env::consts::ARCH {
            "x86" => "x86",
            "x86_64" => "x64",
            s => panic!("Unknown arch: {}", s),
        }
    };
}

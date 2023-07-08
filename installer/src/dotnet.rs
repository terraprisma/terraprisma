use std::{str, path::PathBuf, process::Command, io::{self, Cursor}, env};

use flate2::read::GzDecoder;
use regex::bytes::Regex;
use ureq::Response;
use which::{which_global, which_in};
use zip::{ZipArchive, result::ZipError};

use crate::prompt;

#[derive(Debug)]
pub enum Error {
    FetchingVersion(ureq::Error),
    DownloadingRuntime(ureq::Error),
    ExtractingArchive(io::Error),
    InvalidArchive(String),
}

type DotnetResult<T> = Result<T, Error>;

// TODO manage having multiple local dotnet runtimes
pub fn check_dotnet(major: &i32) -> DotnetResult<PathBuf> {
    fn not_found(major: &i32) -> DotnetResult<PathBuf> {
        println!("Microsoft.NETCore.App {} runtime not found.", major);
        match prompt("Would you like to download a self-contained runtime?") {
            true => download_and_extract_dotnet_runtime(major, "./dotnet"),
            // TODO better exit
            false => {println!("Exiting"); panic!("")},
        }
    }

    match which_global("dotnet") {
        Ok(path) => match get_installed_dotnet_runtimes(major) {
            Some(vers) => {
                println!("{} compatible runtime(s) installed: ", vers.len());
                for rt in vers.iter() {
                    println!("  {}", rt);
                }

                return Ok(path);
            },
            None => not_found(major),
        },
        Err(_) => not_found(major),
    }
}

fn get_installed_dotnet_runtimes(major: &i32) -> Option<Vec<String>> {
    let dotnet_list_runtimes = Command::new("dotnet")
        .args(["--list-runtimes"])
        .output().unwrap();

    if !dotnet_list_runtimes.status.success() {
        println!(".NET {} not found", major);
        return None;
    };

    let pattern = format!(r"dMicrosoft.NETCore.App {}\.[0-9]+\.[0-9]+", major);
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

fn download_and_extract_dotnet_runtime(major: &i32, into_path: &str) -> DotnetResult<PathBuf> {
    fn download_runtime(&major: &i32) -> DotnetResult<Response> {
        let client = ureq::agent();

        match client.get(&version_url(&major)).call() {
            Err(e) => Err(Error::FetchingVersion(e)),
            Ok(version) => {
                let version_text = version.into_string().unwrap();
                let runtime_url = runtime_url(&version_text);
    
                match client.get(&runtime_url).call() {
                    Err(e) => Err(Error::DownloadingRuntime(e)),
                    Ok(r) => Ok(r),
                }
            },
        }
    }

    fn extract_runtime(runtime: Response, into_path: &str) -> DotnetResult<PathBuf> {
        fn get_archive_from_url(url: &str) -> &str {
            url.split_at(url.find('.').unwrap()).1
        }

        fn extract_zip(runtime: Response, into_path: &str) -> DotnetResult<PathBuf> {
            let mut buf = Vec::<u8>::new();
            match runtime.into_reader().read_to_end(&mut buf) {
                Err(e) => Err(Error::ExtractingArchive(e)),
                Ok(_) => match ZipArchive::new(Cursor::new(buf)) {
                    Err(e) => match e {
                        ZipError::FileNotFound | ZipError::Io(_) =>
                            unreachable!(),
                        ZipError::InvalidArchive(e) | ZipError::UnsupportedArchive(e) =>
                            Err(Error::InvalidArchive(e.to_string())),
                    },
                    Ok(mut zip) => match zip.extract(into_path) {
                        Err(e) => match e {
                            ZipError::Io(e) => Err(Error::ExtractingArchive(e)),
                            _ => unreachable!(),
                        },
                        Ok(_) => Ok(find_dotnet_in(into_path).unwrap()),
                    }
                }
            }
        }

        fn extract_tar_gz(runtime: Response, into_path: &str) -> DotnetResult<PathBuf> {
            let gzip = GzDecoder::new(runtime.into_reader());
            let mut tar = tar::Archive::new(gzip);
            match tar.unpack(into_path) {
                Ok(_) => Ok(find_dotnet_in(into_path).unwrap()),
                Err(e) => Err(Error::ExtractingArchive(e)),
            }
        }

        if runtime.get_url().ends_with("zip") {
            extract_zip(runtime, into_path)
        } else if runtime.get_url().ends_with(".tar.gz") {
            extract_tar_gz(runtime, into_path)
        } else {
            return Err(Error::InvalidArchive(
                format!("Unknown archive format {}",
                    get_archive_from_url(runtime.get_url())
                ))
            );
        }
    }

    match download_runtime(&major) {
        Err(e) => Err(e),
        Ok(runtime) => extract_runtime(runtime, into_path),
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
    format!("dotnet-runtime-{}-{}-{}.{}", specific_version, dotnet_target_os(), dotnet_target_arch(),
    match dotnet_target_os() {
        "win" => "zip",
        "osx" | "linux" => "tar.gz",
        s => panic!("Unknown OS {}", s),
    })
}



fn dotnet_target_os() -> &'static str {
    match env::consts::OS {
        "linux" => "linux",
        "macos" => "osx",
        "windows" => "win",
        s => panic!("Unknown os: {}", s),
    }
}

fn dotnet_target_arch() -> &'static str {
    match env::consts::ARCH {
        "x86" => "x86",
        "x86_64" => "x64",
        s => panic!("Unknown arch: {}", s),
    }
}

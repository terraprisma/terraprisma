{
  description = "Nix build support for The Terraprisma Project";
  inputs.nixpkgs.url = "nixpkgs/nixos-unstable";
  outputs = { self, nixpkgs }:
    let
      lastModifiedDate = self.lastModifiedDate or self.lastModified or "19700101";
      version = builtins.substring 0 8 lastModifiedDate;
      supportedSystems = [ "x86_64-linux" ]; # Untested on other platforms
      forAllSystems = nixpkgs.lib.genAttrs supportedSystems;
      nixpkgsFor = forAllSystems (system: import nixpkgs { inherit system; overlays = [ self.overlay ]; });
    in

    {
      overlay = final: prev: {
        terraprisma-ssg = with final; buildDotnetModule rec {
          name = "${pname}-${version}";
          pname = "terraprisma-ssg";
          version = "1.0.0";

          src = ./docs/generator;
          projectFile = "Terraprisma.Docs.sln";

          dotnet-sdk = dotnet-sdk_7;
          dotnet-runtime = dotnet-sdk_7; # Yes, this is intentional.
          nugetDeps = ./docs/generator/deps.nix;

          executables = [ "Terraprisma.Docs.SSG" ];
          meta.mainProgram = "Terraprisma.Docs.SSG";
        };
      };

      devShells = forAllSystems (system:
        let
          dotnet_devenv_sdk = (with nixpkgsFor.${system}.dotnetCorePackages; combinePackages [
            sdk_7_0
          ]);
        in
        {
          default = with nixpkgsFor.${system}; mkShell rec {
            name = "default";
            packages = [
              omnisharp-roslyn
              dotnet_devenv_sdk
            ];

            shellHook = ''
              # Microsoft.Build.Locate assumes `dotnet` is never a symlink, so
              # we comply and place the original `dotnet` binary on the PATH
              # before the dotnet_sdk/bin symlink that Nix adds.
              # We also set DOTNET_ROOT, because the PATH changing trick
              # seems to only work when this is properly set.
              export DOTNET_ROOT=${dotnet_devenv_sdk}
              export PATH=${dotnet_devenv_sdk}:$PATH
            '';
          };
        }
      );

      packages = forAllSystems (system:
        {
          inherit (nixpkgsFor.${system}) terraprisma-ssg;
        });
      defaultPackage = forAllSystems (system: self.packages.${system}.terraprisma-ssg);
    };
}

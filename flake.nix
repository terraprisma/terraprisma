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
      };

      devShells = forAllSystems (system:
        let
          dotnet_devenv_sdk = (with nixpkgsFor.${system}.dotnetCorePackages; combinePackages [
            sdk_7_0
            sdk_6_0
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
              export DOTNET_ROOT="${dotnet_devenv_sdk}"
              export DOTNET_ROOT_X64=$DOTNET_ROOT # Ugly hack but .NET seems to break without it
              export PATH="${dotnet_devenv_sdk}:$PATH"
            '';
          };
        }
      );

      packages = forAllSystems (system:
        {
        });
    };
}

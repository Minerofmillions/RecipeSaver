{
  description = "tModLoader development environment";

  inputs = {
    nixpkgs.url = "github:nixos/nixpkgs/nixos-unstable";
    utils.url = "github:numtide/flake-utils";
  };

  outputs = { self, nixpkgs, utils }:
    utils.lib.eachDefaultSystem (system:
      let
        pkgs = import nixpkgs { inherit system; };
        runtimeLibs = with pkgs; [
          vulkan-loader
          libGL
          libX11
          libXcursor
          libXrandr
          libXi
          libpulseaudio
          alsa-lib
          pipewire
        ];
      in
      {
        devShells.default = pkgs.mkShell {
          buildInputs = with pkgs; [
            dotnetCorePackages.sdk_8_0
          ] ++ runtimeLibs;

          shellHook = ''
            export LD_LIBRARY_PATH="${pkgs.lib.makeLibraryPath runtimeLibs}:$LD_LIBRARY_PATH"
            export ALSA_PLUGIN_DIR="${pkgs.pipewire}/lib/alsa-lib"
            echo "tModLoader development shell loaded."
          '';
        };
      });
}

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
        packages = with pkgs; [
          dotnetCorePackages.sdk_8_0
          
          SDL2
          libGL
          vulkan-loader
          libX11
          libXcursor
          libXext
          libXi
          libXrandr
          
          fna3d
        ];
      in
      {
        devShells.default = pkgs.mkShell {
          buildInputs = packages;

          shellHook = ''
            export LD_LIBRARY_PATH="${pkgs.lib.makeLibraryPath packages}:/run/opengl-driver/lib:/run/opengl-driver-32/lib:$LD_LIBRARY_PATH"
            echo "tModLoader development shell loaded."
          '';
        };
      });
}
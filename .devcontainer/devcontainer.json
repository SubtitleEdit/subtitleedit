{
	"name": "MultiversX Smart Contracts",
	"image": "multiversx/devcontainer-smart-contracts-rust:latest",
	// Features to add to the dev container. More info: https://containers.dev/features.
	"features": {
		"ghcr.io/devcontainers/features/docker-in-docker:2": {
			"version": "latest",
			"moby": false,
			"installDockerBuildx": false
		}
	},
	// Use 'forwardPorts' to make a list of ports inside the container available locally.
	// "forwardPorts": [],
	// Run commands after the container is created.
	"postCreateCommand": "python3 ~/multiversx-sdk/devcontainer-resources/post_create_command.py",
	// Configure tool-specific properties.
	"customizations": {
		"vscode": {
			"extensions": [
				"Elrond.vscode-elrond-ide",
				"rust-lang.rust-analyzer",
				"vadimcn.vscode-lldb",
				"dtsvet.vscode-wasm"
			],
			"settings": {
				"launch": {
					"version": "0.2.0",
					"configurations": [],
					"compounds": []
				},
				"terminal.integrated.env.linux": {},
				"terminal.integrated.defaultProfile.linux": "bash"
			}
		}
	},
	"containerEnv": {},
	"remoteUser": "developer"
}

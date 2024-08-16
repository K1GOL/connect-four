# connect-four

A simple game of multiplayer/singleplayer Connect Four written in C#.

## Installation

### Development

Clone the repo. Run with `$ dotnet run <server/client-player/client-bot>`. Build with `$ dotnet build connect-four.csproj -c Release`.

### Release version

Download from the releases tab.

## Usage

### Server

Run the game server with `$ connect-four server`. When prompted, enter the IP and port for the server. Leave empty for defaults.

### Player client

Play the game with the player client. Run `$ connect-four client-player`. When prompted, enter the IP and port for the server, and select player. Leave empty for defaults.

### Bot client

The bot client is a client for bots to play the game. A bot implementation called `C4bot` is included. When prompted, enter the IP and port for the server, and select player. Leave empty for defaults. The bot will then play the game.

# Screenshot tool

Takes the screenshots used by the documentation and saves them in `docs/images/<lang>/`, one folder per interface language (`en`, `pt`, `es`). It drives a headless Edge or Chrome through the DevTools protocol. It needs Node 22 or later and has no dependencies.

## What it captures

| Command | Shots | Needs |
|---|---|---|
| `terminal` | The installer and the Docker commands, rendered as terminal windows from the recorded output in [terminal/](terminal/). The same images go to every language folder. | Nothing else. |
| `wizard` | Every step of the first-run wizard, from the setup code to the finished site. | A site started on an empty data folder, and its setup code. |
| `site` | Every public page, the skin shop, the community ranking, the in-game pages, the phone layout, both themes and every administration page. | A site with data, signed in through the development sign-in. |

The lists of shots are in [site.js](site.js), [wizard.js](wizard.js) and [terminal-shots.json](terminal-shots.json). Each shot has a name, a page, a width (1280 for desktop, 390 for phone, 900x450 for the in-game pages), the actions done before the capture, and what to keep: the full page, the visible part, or one element. Images are WebP at quality 80.

## Running the site for it

The `site` and `wizard` commands sign in through the development sign-in (`/dev/sign-in/<SteamID64>`), which exists only in the Development environment with test sign-in allowed. Publish the site and run it on a data folder of your own:

```bash
dotnet publish src/L4D2PlayStats.Hub.Web -c Release -o ./shots-run
```

```bash
cd shots-run
ASPNETCORE_ENVIRONMENT=Development Development__AllowTestSignIn=true Hub__DataDirectory=/path/to/data ./L4D2PlayStats.Hub.Web --urls http://localhost:5103
```

Never point it at a production data folder: the `site` command buys a skin and the `wizard` command finishes the setup.

## The wizard

Start the site on an empty data folder. It prints the setup code and writes it to `setup-code.txt` in the data folder. Then:

```bash
node tools/screenshots/capture.js wizard --base http://localhost:5103 --lang en --admin 76561198000000000 --setup-code <code>
```

The wizard runs only once per data folder. For each language, stop the site, empty the folder, start it again and use the new code. The tool hides the development sign-in field before each capture, since a production site does not show it, and sends a heartbeat as a game server would, so the connection status turns green.

## The rest of the site

The `site` command expects a copy of real data with:

- every module turned on in **Administration > Modules**;
- two game servers, with their tokens;
- some community votes, a chat ban, a remote command and a player connection, so those pages are not empty.

```bash
node tools/screenshots/capture.js site --base http://localhost:5103 --lang en --admin <SteamID64 of an administrator> --player <SteamID64 of a player with matches and stars> --token <token of server 1> --token <token of server 2>
```

Before the live server shots it sends the live match of each server, the way the plugins do, and waits for the delay of the live page. The shop shots buy the first skin the player can afford, so each run needs a skin that player does not own yet.

## The terminal windows

The files in [terminal/](terminal/) are the real output of the commands, recorded with colours in a disposable `jrei/systemd-ubuntu:24.04` container (`docker exec -t`). The tool replays them through a small terminal emulator and screenshots the result. To record them again, run the command in a container and save its output:

```bash
docker exec -t <container> bash -c "stty cols 96 rows 30; playstats-hub status" > tools/screenshots/terminal/linux-status.log
```

Then:

```bash
node tools/screenshots/capture.js terminal
```

## Options

| Option | Meaning |
|---|---|
| `--base` | Address of the site. |
| `--lang` | `en`, `pt` or `es`. It sets the interface language before the first shot. |
| `--admin` | SteamID64 of an administrator of that site. |
| `--player` | SteamID64 used for the player page, the shop and the in-game pages. |
| `--token` | Token of a game server, `identifier:key`. Repeat it for a second server. |
| `--setup-code` | The setup code, for the wizard. |
| `--only` | Comma-separated names of the shots to take again. |
| `--out` | Folder for the images. Default: `docs/images`. |
| `--langs` | For `terminal`: the language folders to write. Default: `en,pt,es`. |
| `--browser` | Path to Edge or Chrome, if it is not found by itself. |

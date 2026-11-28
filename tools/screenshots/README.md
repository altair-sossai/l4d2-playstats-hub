# Screenshot tool

Takes the screenshots used by the documentation and saves them in `docs/images/<lang>/`, one folder per interface language (`en`, `pt`, `es`). It drives a headless Edge or Chrome through the DevTools protocol. It needs Node 22 or later and has no dependencies.

## What it captures

| Command | Shots | Needs |
|---|---|---|
| `terminal` | The installer and the Docker commands, rendered as terminal windows from the recorded output in [terminal/](terminal/). The same images go to every language folder. | Nothing else. |
| `wizard` | Every step of the first-run wizard, from the setup code to the finished site. | A site started on an empty data folder, and its setup code. |
| `switch` | The installation of l4d2.com.br for [Switching from the old site](../../docs/switch-from-old-site.md): the wizard and the administration pages with the values of that site, and the overview after the import. | A site started on an empty data folder, its setup code and the logo of the site. For the overview, the data folder after the import. |
| `site` | Every public page, the skin shop, the community ranking, the championships, the bans, the notifications, the in-game pages, the phone layout, both themes and every administration page. | A site with data, signed in through the development sign-in. |

The lists of shots are in [site.js](site.js), [wizard.js](wizard.js) and [terminal-shots.json](terminal-shots.json). Each shot has a name, a page, a width (1280 for desktop, 390 for phone, 900x450 for the in-game pages), the actions done before the capture, and what to keep: the full page, the visible part, or one element. Images are WebP at quality 80. After opening a page the tool moves the pointer once, as a visitor would, since the site asks for the players online, shown in the menu bar, only after the first interaction.

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

The wizard runs only once per data folder. For each language, stop the site, empty the folder, start it again and use the new code. The tool hides the development sign-in field before each capture, since a production site does not show it. After the last step it opens the page of the new server in the administration area and sends a heartbeat as a game server would, so the connection status turns green.

## The switch guide

The `switch` command fills the wizard and the administration pages with the values of l4d2.com.br, listed at the top of [switch.js](switch.js), and captures each step as `switch-*.webp`. Start the site on an empty data folder, as for the wizard, then:

```bash
node tools/screenshots/capture.js switch --base http://localhost:5103 --lang en --admin 76561198000000000 --setup-code <code> --logo logo.png --site-address http://124.198.128.43:5090
```

`--site-address` replaces the local address in the lines shown for the game server. The same command configures a real site: `--admins` adds more administrators, `--steam-key-file` reads the Steam Web API key from a file and `--token-file` saves the token of the game server to a file, so neither is printed. With `--step overview` it only captures the overview of the administration area, after the statistics are imported.

## Fictional data

The screenshots of the documentation show fictional players. They were taken on a copy of the data of a real community in which every person was replaced by a made-up one, so the matches, the rankings and the numbers look real but no nickname, avatar, SteamID or address belongs to anyone. [fictional-data.js](fictional-data.js) makes that copy from a data folder of your own:

```bash
node tools/screenshots/fictional-data.js --from /path/to/real/data --to /path/to/fictional/data
```

- It only reads the folder in `--from`, and stops if another program, such as the site, has its database open (detected on Windows and Linux). Stop the site first, or give a copy of the folder.
- The folder in `--to` must be new or empty. It gets the database, the logo and new avatars; the real avatars and the keys are not copied.
- Every SteamID becomes one whose account number starts at 4,000,000,000, a range Steam does not use yet, so no profile link leads to a real person. The administrators come first, so the first one is always `76561201960265728`. The SteamIDs change in every table, in the settings and in the raw statistics files, in every format they appear in (SteamID64, `STEAM_x:y:z` and `[U:1:n]`).
- Every name becomes a made-up gamer tag, the same one for the same person everywhere, the raw statistics files and the previous names included. A player who used several names gets several fictional names.
- Every player who had an avatar gets a new one, a coloured square with initials or a simple shape, drawn in Edge or Chrome.
- The IP addresses of the player connections become addresses of the documentation ranges (`198.51.100.0/24` and `192.0.2.0/24`). The game servers become `L4D2 Competitive #1`, `#2`... at `203.0.113.10`, with the identifiers `l4d2-competitive-1`, `l4d2-competitive-2`...: their tokens keep the same key with the new identifier.
- The skin catalog is kept as it is, since it credits the authors of the skins on the Steam Workshop.

The names, SteamIDs and avatars come from a random seed, which the tool prints; pass it again with `--seed` to get the same result from the same data. It needs Node 22.16 or later, with no flags. When it finishes, start the site on the new folder, sign in with the SteamID64 it printed for the administrator and run **Administration > Overview > Reprocess all matches**, so every match is built again from the fictional files.

## The rest of the site

The `site` command expects a data folder, such as the fictional copy above, with:

- every module turned on in **Administration > Modules**;
- two game servers, with their tokens;
- some community votes, a chat ban, a remote command and a player connection, so those pages are not empty;
- a few backups in **Administration > Backup**, one of them failed, so the list of recent backups is not empty. Azurite, the Azure Storage emulator, can receive them on the machine: start it with `npx azurite-blob --skipApiVersionCheck`, save its connection string (`UseDevelopmentStorage=true` or the full one with `devstoreaccount1`) and a container name, and click **Back up now**. The `admin-backup` shot then fills the form with an example container address, which is never saved.

```bash
node tools/screenshots/capture.js site --base http://localhost:5103 --lang en --admin <SteamID64 of an administrator> --player <SteamID64 of a player with matches and stars> --token <token of server 1> --token <token of server 2>
```

Before the live server shots it sends the live match of each server, the way the plugins do, with the players at the top of the ranking of the site, and waits for the delay of the live page. Restart the site before each language, so the live chat starts empty. Before the shop shots it refunds the purchases of the player, then buys the first skin the player can afford. The chat ban shots add a ban for `--banned`. Before the Anti-Cheat settings shot it puts the first two players of the ranking on the whitelist and the next six on the required list, and saves the mode. It also changes the data, so use a copy that can be thrown away: it blocks two voters of the community ranking for the blocked voting page, adds the sixth player of the ranking as an administrator, and imports forced names for the 11th to 16th players and adds one for the 17th. Before the bans shots it sends bans and connections as the plugins do, for players of the ranking, and before the championships shots it creates three championships ([championships.js](championships.js)): a finished league, a cup in progress with a draft, standings and a bracket, and one that takes requests to join, with images drawn in the browser. A button that asks for confirmation is confirmed by itself. Use a new copy of the data for each language.

The development sign-in has no Steam name, so it uses the name the site knows for that player. The menu, the purchases and the audit logs show `Player <SteamID64>` only for a user the site does not know.

## The terminal windows

The files in [terminal/](terminal/) are the real output of the commands, recorded with colours in a disposable `jrei/systemd-ubuntu:24.04` container (`docker exec -t`). The tool replays them through a small terminal emulator and screenshots the result. To record them again, run the command in a container and save its output:

```bash
docker exec -t <container> bash -c "stty cols 96 rows 30; playstats-hub status" > tools/screenshots/terminal/linux-status.log
```

For the Docker commands, install Docker inside that container (`curl -fsSL https://get.docker.com | sh`) and give its `/var/lib/docker` and `/var/lib/containerd` volumes of their own (`-v <volume>:/var/lib/docker`), since images do not unpack on the file system of the container itself.

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
| `--player` | SteamID64 used for the player page, the shop and the in-game pages. Its purchases are refunded before the shop shots, so it can buy again. Default: the first player of the ranking. |
| `--voter` | SteamID64 of a player who can still vote today, for the voting page. Default: `--player`. |
| `--banned` | SteamID64 used in the chat ban shots. Default: `--player`. |
| `--token` | Token of a game server, `identifier:key`. Repeat it for a second server. |
| `--setup-code` | The setup code, for the wizard. |
| `--only` | Comma-separated names of the shots to take again. |
| `--out` | Folder for the images. Default: `docs/images`. |
| `--langs` | For `terminal`: the language folders to write. Default: `en,pt,es`. |
| `--browser` | Path to Edge or Chrome, if it is not found by itself. |

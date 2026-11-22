# Browser tests

Checks the main pages of the site in a headless Chrome or Edge, so a layout or JavaScript regression fails the build. They run on every push and pull request in [Browser tests](../../.github/workflows/browser-tests.yml) and need Node 22 or later, with no dependencies. They drive the browser with [cdp.js](../screenshots/cdp.js) from the screenshot tool.

## What it does

[run.js](run.js) starts the published site on a new, empty data folder, in the Development environment with the development sign-in (`/dev/sign-in/<SteamID64>`), and fills it the way a real community would:

- completes the setup wizard with the code from `setup-code.txt`, turns on every module and creates a game server;
- uploads 14 statistics files, five matches of 16 fictional players, made from the sample in [tests/L4D2PlayStats.Hub.Tests/Samples](../../tests/L4D2PlayStats.Hub.Tests/Samples) with the token of the server ([statistics.js](statistics.js), players in [fixtures.js](fixtures.js));
- sets the community ranking to 4 votes a day, posts a live match and casts community votes with six players.

Then it runs the checks in [checks.js](checks.js): the home ranking (also at 390 px, with no sideways scroll), the player page and its tabs, the page of an unknown SteamID, the matches, the players list (the overview, ordering it by a column, one statistic and the old history address), every public page at 390 px, the commands page (search, section links and the folded administrator commands), the community ranking (invitation, "how it works", player details and columns windows), voting with the arrow keys and a click on a photo up to the daily limit, the latest votes filters and player search window, every administration page and its help icons, the player search window, the remote command builder, the chat limits and live settings forms, the related accounts window when banning from the chat, the live page receiving new data, Portuguese and Spanish, and the light theme. A JavaScript error or a server error on any page also fails the check.

Everything runs against that throwaway site; the data folder is deleted at the end. Never point it at a real data folder.

## Running it

```bash
dotnet publish src/L4D2PlayStats.Hub.Web -c Release -o ./e2e-site
node tools/e2e/run.js --site ./e2e-site
```

It takes about two minutes. A failed check prints why and saves a screenshot, the page and the site log in `e2e-output/`, which the workflow uploads as the `browser-test-failures` artifact.

| Option | Meaning |
|---|---|
| `--site` | Folder of the published site. |
| `--port` | Port of the site. Default: `5199`. |
| `--only` | Comma-separated names of the checks to run, such as `community-vote,languages`. The data is always created. |
| `--browser` | Path to Chrome or Edge, if it is not found by itself. |
| `--out` | Folder for the failures. Default: `e2e-output`. |
| `--data` | An empty folder to use as the data folder, kept at the end. |
| `--keep` | Keep the temporary data folder. |
| `--serve` | Create the data, then keep the site running for a look in the browser, until Ctrl+C. |

# Anti-Cheat page

🌐 **English** | [Português](anti-cheat.pt.md) | [Español](anti-cheat.es.md)

Some communities ask players to open the game through an Anti-Cheat program before they join the competitive server. The **Anti-Cheat** page of the site explains how to use that program and offers its download.

The Anti-Cheat itself is a separate Windows program with its own service. It is not part of this repository, and this site does not check whether a player is running it. The site only shows the page, and the live server shows an **Anti-Cheat on** badge when the game server reports it.

The page is a module, off by default.

[Back to the manual](README.md)

## Contents

- [What players see](#what-players-see)
- [For administrators](#for-administrators)

## What players see

![The Anti-Cheat page](../images/en/anti-cheat.webp)

- **Download Anti-Cheat**, with the version, the size and the release date.
- **Verify the file**: the PowerShell command to compute the SHA-256 hash of the download, and the value it must match.
- **How to use it**: download and run, answer and wait, play with the window open.
- **What the window shows**: the meaning of each state of the program (starting, updating, active, failed).
- **Common problems**: answers to the usual questions.
- **About -insecure (no VAC)**: the program opens the game without VAC, so it carries no VAC ban risk, and only servers without VAC can be joined while it runs.
- **Privacy**: what the program sends and what it does not do.

The texts of the page are part of the site and are translated with it.

## For administrators

**Administration > Anti-Cheat page** has two addresses.

![The Anti-Cheat settings](../images/en/admin-anti-cheat.webp)

| Setting | What it does |
|---|---|
| Anti-Cheat service address | Optional. The page asks this service for the latest version, its size, its date and its checksum. Must start with `https://`. |
| Download address | Used when the service does not answer or is not set. Must start with `https://`. |

**Open the Anti-Cheat page** shows the result. Turn the page on or off in [Modules](administration.md#modules); when it is off, the menu item disappears and the page answers as not found.

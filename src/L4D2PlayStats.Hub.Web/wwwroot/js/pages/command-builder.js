var commandBuilderConfig = JSON.parse(document.getElementById("command-builder-config").textContent);

function livePlayerNames(selector) {
    var source = $(selector);

    if (!source.length)
        return [];

    return $.map(JSON.parse(source.text()), function (player) {
        return player.name;
    });
}

function livePlayersMissing() {
    return !$('#survivors').length;
}

function typedPlayerName() {
    return livePlayersMissing() ? commandBuilderConfig.texts["Type the player name"] : null;
}

var terminal = new CommandBuilder({
    modal: '#terminal',
    list: '#commands',
    preview: '#command-preview',
    previewContainer: '#command-container',
    texts: {
        selectCommand: commandBuilderConfig.texts["Select a command"],
        back: commandBuilderConfig.texts["Back"],
        next: commandBuilderConfig.texts["Next"]
    },
    commands: {
        '!forcematch': {
            description: commandBuilderConfig.texts["Forces the selection of a match mode"],
            args: ['matchMode']
        },
        '!aac': {
            description: commandBuilderConfig.texts["Turns anti-cheat on or off"],
            args: ['antiCheatState']
        },
        '!resetmatch': {
            description: commandBuilderConfig.texts["Unloads the current match mode"],
            args: []
        },
        '!forcestart': {
            description: commandBuilderConfig.texts["Forces the round to start without waiting for everyone to ready up"],
            args: []
        },
        '!forcepause': {
            description: commandBuilderConfig.texts["Pauses the match"],
            args: []
        },
        '!forceunpause': {
            description: commandBuilderConfig.texts["Unpauses the match"],
            args: []
        },
        '!changelevel': {
            description: commandBuilderConfig.texts["Changes the current campaign"],
            args: ['campaign']
        },
        '!restartmap': {
            description: commandBuilderConfig.texts["Restarts the current map"],
            args: []
        },
        '!swapto': {
            description: commandBuilderConfig.texts["Swaps players between teams"],
            args: ['team', 'players']
        },
        '!swapteams': {
            description: commandBuilderConfig.texts["Swaps both teams to the opposite side"],
            args: []
        },
        '!givetank': {
            description: commandBuilderConfig.texts["Sets which player will be the tank"],
            args: ['infecteds']
        },
        '!tankshuffle': {
            description: commandBuilderConfig.texts["Re-picks who will be the tank"],
            args: []
        },
        '!kick': {
            description: commandBuilderConfig.texts["Kicks a player from the server"],
            args: ['players', 'reason']
        },
        '!votekick': {
            description: commandBuilderConfig.texts["Initiates a vote to kick a player"],
            args: ['players', 'reason']
        },
        '!ban': {
            description: commandBuilderConfig.texts["Bans players for a specified amount of time"],
            args: ['players', 'minutes', 'reason']
        },
        '!voteban': {
            description: commandBuilderConfig.texts["Initiates a vote to ban a player"],
            args: ['players', 'reason']
        },
        '!silence': {
            description: commandBuilderConfig.texts["Silences a player"],
            args: ['players']
        },
        '!unsilence': {
            description: commandBuilderConfig.texts["Removes silence from a player"],
            args: ['players']
        },
        '!gag': {
            description: commandBuilderConfig.texts["Gags a player"],
            args: ['players']
        },
        '!ungag': {
            description: commandBuilderConfig.texts["Removes gag from a player"],
            args: ['players']
        },
        '!mute': {
            description: commandBuilderConfig.texts["Mutes a player"],
            args: ['players']
        },
        '!unmute': {
            description: commandBuilderConfig.texts["Unmutes a player"],
            args: ['players']
        },
        '!slay': {
            description: commandBuilderConfig.texts["Slays a player"],
            args: ['players']
        },
        '!crash': {
            description: commandBuilderConfig.texts["Causes the server to crash and restart to its initial state"],
            args: []
        }
    },
    dataSources: {
        wildcards: {
            title: commandBuilderConfig.texts["Select a wildcard"],
            items: function () {
                return ['@all', '@survivors', '@infected'];
            }
        },
        survivors: {
            title: commandBuilderConfig.texts["Select a survivor"],
            input: typedPlayerName,
            items: function () {
                return livePlayerNames('#survivors');
            }
        },
        infecteds: {
            title: commandBuilderConfig.texts["Select an infected"],
            input: typedPlayerName,
            items: function () {
                return livePlayerNames('#infecteds');
            }
        },
        spectators: {
            title: commandBuilderConfig.texts["Select a spectator"],
            input: typedPlayerName,
            items: function () {
                return livePlayerNames('#spectators');
            }
        },
        players: {
            title: commandBuilderConfig.texts["Select a player"],
            input: typedPlayerName,
            items: function () {
                var sources = terminal.dataSources;

                return []
                    .concat(sources.survivors.items())
                    .concat(sources.infecteds.items())
                    .concat(sources.spectators.items())
                    .concat(sources.wildcards.items());
            }
        },
        team: {
            title: commandBuilderConfig.texts["Select a team"],
            items: function () {
                return [
                    { displayText: commandBuilderConfig.texts["Spectators"], value: 1 },
                    { displayText: commandBuilderConfig.texts["Survivors"], value: 2 },
                    { displayText: commandBuilderConfig.texts["Infected"], value: 3 }
                ];
            }
        },
        minutes: {
            title: commandBuilderConfig.texts["Select duration in minutes"],
            items: function () {
                return [
                    { displayText: commandBuilderConfig.texts["5 minutes"], value: 5 },
                    { displayText: commandBuilderConfig.texts["10 minutes"], value: 10 },
                    { displayText: commandBuilderConfig.texts["30 minutes"], value: 30 },
                    { displayText: commandBuilderConfig.texts["1 hour"], value: 60 },
                    { displayText: commandBuilderConfig.texts["1 day"], value: 1440 },
                    { displayText: commandBuilderConfig.texts["Permanent"], value: 0 }
                ];
            }
        },
        reason: {
            title: commandBuilderConfig.texts["Select a reason"],
            items: function () {
                return [
                    { displayText: commandBuilderConfig.texts["None"] },
                    commandBuilderConfig.texts["Toxic behavior"],
                    commandBuilderConfig.texts["Griefing"],
                    commandBuilderConfig.texts["Cheating"],
                    commandBuilderConfig.texts["Mic spam"],
                    commandBuilderConfig.texts["AFK"],
                    commandBuilderConfig.texts["Harassment"],
                    commandBuilderConfig.texts["Team kill"],
                    commandBuilderConfig.texts["Rule violation"],
                    commandBuilderConfig.texts["Other"]
                ];
            }
        },
        matchMode: {
            title: commandBuilderConfig.texts["Select the match mode"],
            items: function () {
                return [
                    { displayText: 'ZoneMod', value: 'zonemod' },
                    { displayText: 'ZoneMod Retro', value: 'zoneretro' },
                    { displayText: '3v3 ZoneMod', value: 'zm3v3' },
                    { displayText: '2v2 ZoneMod', value: 'zm2v2' },
                    { displayText: '1v1 ZoneMod', value: 'zm1v1' },

                    { displayText: 'NeoMod Tourney', value: 'neomod' },

                    { displayText: 'NextMod', value: 'nextmod' },
                    { displayText: '3v3 NextMod', value: 'nextmod3v3' },
                    { displayText: '2v2 NextMod', value: 'nextmod2v2' },
                    { displayText: '1v1 NextMod', value: 'nextmod1v1' },

                    { displayText: 'Promod Elite', value: 'pmelite' },
                    { displayText: 'Deadman', value: 'deadman' },

                    { displayText: 'Acemod RV', value: 'acemodrv' },
                    { displayText: '3v3 Acemod RV', value: 'amrv3v3' },
                    { displayText: '2v2 Acemod RV', value: 'amrv2v2' },
                    { displayText: '1v1 Acemod RV', value: 'amrv1v1' },

                    { displayText: 'Equilibrium ', value: 'eq' },
                    { displayText: '3v3 EQ', value: 'eq3v3' },
                    { displayText: '2v2 EQ', value: 'eq2v2' },
                    { displayText: '1v1 EQ', value: 'eq1v1' },

                    { displayText: 'Apex', value: 'apex' },

                    { displayText: 'ZoneHunters', value: 'zonehunters' },
                    { displayText: '3v3 ZoneHunters', value: 'zh3v3' },
                    { displayText: '2v2 ZoneHunters', value: 'zh2v2' },
                    { displayText: '1v1 ZoneHunters', value: 'zh1v1' }
                ];
            }
        },
        antiCheatState: {
            title: commandBuilderConfig.texts["Turn the anti-cheat on or off"],
            items: function () {
                return [
                    { displayText: commandBuilderConfig.texts["Turn on"], value: 'on' },
                    { displayText: commandBuilderConfig.texts["Turn off"], value: 'off' }
                ];
            }
        },
        campaign: {
            title: commandBuilderConfig.texts["Select a campaign"],
            items: function () {
                return [
                    { displayText: 'Dead Center', value: 'c1m1_hotel' },
                    { displayText: 'Dark Carnival', value: 'c2m1_highway' },
                    { displayText: 'Swamp Fever', value: 'c3m1_plankcountry' },
                    { displayText: 'Hard Rain', value: 'c4m1_milltown_a' },
                    { displayText: 'The Parish', value: 'c5m1_waterfront' },
                    { displayText: 'The Passing', value: 'c6m1_riverbank' },
                    { displayText: 'The Sacrifice', value: 'c7m3_port' },
                    { displayText: 'No Mercy', value: 'c8m1_apartment' },
                    { displayText: 'Crash Course', value: 'c9m1_alleys' },
                    { displayText: 'Death Toll', value: 'c10m1_caves' },
                    { displayText: 'Dead Air', value: 'c11m1_greenhouse' },
                    { displayText: 'Blood Harvest', value: 'c12m1_hilltop' },
                    { displayText: 'Cold Stream', value: 'c13m1_alpinecreek' }
                ];
            }
        }
    }
});

$(function () {
    $.ajax({
        url: '/api/campaigns',
        method: 'GET',
        dataType: 'json',
        success: function (campaigns) {
            terminal.dataSources.campaign.items = function () {
                return $.map(campaigns, function (campaign) {
                    return { displayText: campaign.name, value: campaign.maps[0] };
                });
            };
        }
    });
});

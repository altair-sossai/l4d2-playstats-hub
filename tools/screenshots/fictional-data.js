process.removeAllListeners('warning');
process.on('warning', warning => {
    if (warning.name !== 'ExperimentalWarning')
        console.warn(warning);
});

const crypto = require('crypto');
const fs = require('fs');
const path = require('path');
const { DatabaseSync, backup } = require('node:sqlite');

const steamBase = 76561197960265728n;
const firstAccount = 4000000000n;
const steamRange = 1n << 32n;
const keptTables = new Set(['Skin', 'SkinText', 'Campaigns', 'CampaignMaps', '__EFMigrationsHistory', '__EFMigrationsLock', 'sqlite_sequence']);
const nameColumns = {
    MatchPlayers: ['CommunityId', 'Name'],
    PlayerProfiles: ['CommunityId', 'Name'],
    PlayerConnections: ['CommunityId', 'Name'],
    ChatBans: ['CommunityId', 'Name'],
    Administrators: ['CommunityId', 'Name'],
    SkinPurchase: ['CommunityId', 'PlayerName'],
    RemoteCommandLogs: ['AdministratorCommunityId', 'AdministratorName']
};
const extraNameColumns = {
    ChatBans: [['CreatedByCommunityId', 'CreatedByName']],
    SkinPurchase: [['GiftedByCommunityId', 'GiftedByName']]
};
const freeTextColumns = { ChatBans: ['Reason'], Punishments: ['Reason'], RemoteCommandLogs: ['Command'] };

const words = {
    adjectives: ['Silent', 'Rusty', 'Lazy', 'Swift', 'Crimson', 'Frozen', 'Lucky', 'Grumpy', 'Sneaky', 'Brave', 'Salty', 'Cosmic', 'Wild', 'Mighty', 'Sleepy', 'Hungry', 'Electric', 'Golden', 'Fuzzy', 'Iron', 'Turbo', 'Neon', 'Rapid', 'Quiet', 'Feral', 'Noble', 'Rogue', 'Jolly', 'Dusty', 'Misty', 'Stormy', 'Lunar', 'Solar', 'Atomic', 'Pixel', 'Spicy', 'Crispy', 'Cheesy', 'Little', 'Big', 'Old', 'Wicked', 'Clumsy', 'Furious', 'Hidden', 'Lonely', 'Savage', 'Final', 'Lost', 'Dark', 'Bright', 'Royal', 'Arctic', 'Tropical', 'Burnt', 'Steel', 'Copper', 'Velvet', 'Hollow', 'Ghostly', 'Midnight', 'Polar', 'Sunny', 'Rainy', 'Windy', 'Loud', 'Tiny', 'Giant', 'Fancy', 'Crazy', 'Happy', 'Lucid', 'Rapid', 'Smooth', 'Sharp', 'Blunt', 'Shiny', 'Rotten', 'Sticky', 'Frosty', 'Toasted', 'Wired', 'Sonic', 'Mystic', 'Primal', 'Infinite', 'Broken', 'Patient', 'Restless', 'Careful', 'Reckless', 'Humble', 'Proud', 'Calm', 'Nervous'],
    nouns: ['Fox', 'Hammer', 'Panda', 'Falcon', 'Badger', 'Otter', 'Wolf', 'Raven', 'Viper', 'Cobra', 'Moose', 'Walrus', 'Penguin', 'Llama', 'Yeti', 'Goblin', 'Knight', 'Pirate', 'Ninja', 'Samurai', 'Wizard', 'Cowboy', 'Sheriff', 'Medic', 'Sniper', 'Biscuit', 'Pickle', 'Waffle', 'Taco', 'Noodle', 'Muffin', 'Pancake', 'Potato', 'Coconut', 'Pretzel', 'Nugget', 'Meatball', 'Cactus', 'Comet', 'Meteor', 'Rocket', 'Thunder', 'Blizzard', 'Tornado', 'Volcano', 'Glacier', 'Canyon', 'Crow', 'Hornet', 'Mantis', 'Scorpion', 'Shark', 'Squid', 'Kraken', 'Gecko', 'Capybara', 'Armadillo', 'Toucan', 'Jaguar', 'Puma', 'Ocelot', 'Sloth', 'Koala', 'Wombat', 'Beaver', 'Bison', 'Rhino', 'Hippo', 'Turtle', 'Lobster', 'Crab', 'Sardine', 'Mosquito', 'Beetle', 'Spider', 'Bandit', 'Outlaw', 'Ranger', 'Scout', 'Pilot', 'Captain', 'Sergeant', 'Corporal', 'Baron', 'Duke', 'Chef', 'Doctor', 'Janitor', 'Mailman', 'Plumber', 'Trucker', 'Biker', 'Skater', 'Surfer', 'Drifter', 'Nomad', 'Wanderer', 'Hermit', 'Monk', 'Druid', 'Bard', 'Golem', 'Troll', 'Ogre', 'Gnome', 'Dwarf', 'Phoenix', 'Dragon', 'Griffin', 'Hydra', 'Mammoth', 'Raptor', 'Cyclops', 'Vandal', 'Reaper', 'Specter', 'Wraith', 'Zombie', 'Survivor', 'Molotov', 'Shotgun', 'Magnum', 'Crowbar', 'Machete', 'Chainsaw', 'Grenade', 'Pipebomb', 'Medkit', 'Defib', 'Tatu', 'Jabuti', 'Caju', 'Pastel', 'Coxinha', 'Pipoca', 'Tucano', 'Saci', 'Boto', 'Mico', 'Quati', 'Lobo', 'Gato', 'Pato', 'Urubu', 'Sabiá', 'Jacaré', 'Sucuri', 'Bigode', 'Paçoca', 'Churros', 'Burrito', 'Pudim', 'Tapioca', 'Cuscuz', 'Chimichurri', 'Empanada', 'Mate', 'Condor', 'Lhama', 'Gaúcho', 'Vaquero'],
    infected: ['Tank', 'Witch', 'Boomer', 'Smoker', 'Hunter', 'Jockey', 'Charger', 'Spitter', 'Horde', 'Common'],
    actions: ['Buster', 'Slayer', 'Bait', 'Tamer', 'Whisperer', 'Hugger', 'Chaser', 'Magnet', 'Food', 'Fan', 'Hater', 'Killer', 'Skeeter', 'Dodger', 'Blocker', 'Crusher', 'Collector', 'Tickler'],
    phrases: ['PillsHere', 'SaferoomCamper', 'CrownMaster', 'SkeetMachine', 'LedgeHanger', 'RockEater', 'NoRescue', 'BileJar', 'FullBonus', 'TeamWipe', 'LastSurvivor', 'ClutchOrKick', 'InstaCap', 'DeadCenter', 'DarkCarnival', 'ParishRunner', 'SwampFever', 'NoMercyPlease', 'DeathToll', 'BloodHarvest', 'ColdStream', 'CrashCourse', 'TheSacrifice', 'PassingBy', 'SpitOnMe', 'ChargeMeBro', 'WitchPlease', 'TankYouVeryMuch', 'BoomerSmile', 'PounceKing', 'DeadstopPro', 'LevelUpPls', 'MvpOrNothing', 'FriendlyFireOops', 'RageQuitter', 'OneMoreMap', 'GgWp', 'AfkAgain', 'LagSwitchless'],
    syllables: ['ka', 'zu', 'ri', 'mo', 'te', 'lo', 'vi', 'ny', 'xo', 'ke', 'ru', 'sa', 'mi', 'to', 'zi', 'fa', 'le', 'ro', 'ju', 'vo', 'ti', 'ma', 'nu', 'ze', 'go', 'li'],
    tags: ['ZK', 'BR', 'GG', 'TDZ', 'L4D', 'SVR', 'NMR', 'DC', 'HR', 'PX', 'RX', 'VLT', 'ORC', 'WLF', 'FX', 'KRT', 'ZN', 'LV', 'MX', 'TNK']
};

function usage() {
    console.log(`Usage:
  node tools/screenshots/fictional-data.js --from <data folder with real data> --to <new folder> [--seed <text>] [--browser <path>]

Copies the data folder given in --from into the new folder given in --to, replacing every person with a
fictional one: SteamIDs, names, avatars, IP addresses, the raw statistics files and the settings. The folder in
--from is only read, and the tool stops if another program, such as the site, has its database open. The folder
in --to must not exist or must be empty. Then start the site on the new folder and run
Administration > Overview > "Reprocess all matches".`);
}

function parse(argv) {
    const options = {};

    for (let index = 0; index < argv.length; index += 2) {
        if (!argv[index].startsWith('--') || argv[index + 1] === undefined)
            return null;

        options[argv[index].slice(2)] = argv[index + 1];
    }

    return options.from && options.to ? options : null;
}

function random(seed) {
    let state = crypto.createHash('sha256').update(seed).digest().readUInt32LE(0);

    return () => {
        state = (state + 0x6d2b79f5) >>> 0;
        let value = state;
        value = Math.imul(value ^ (value >>> 15), value | 1);
        value ^= value + Math.imul(value ^ (value >>> 7), value | 61);

        return ((value ^ (value >>> 14)) >>> 0) / 4294967296;
    };
}

function isSteamId(value) {
    return typeof value === 'bigint' && value > steamBase && value < steamBase + steamRange;
}

function accountOf(id) {
    return BigInt(id) - steamBase;
}

class Names {
    constructor(next, reserved) {
        this.next = next;
        this.reserved = reserved;
        this.used = new Set();
    }

    pick(list) {
        return list[Math.floor(this.next() * list.length)];
    }

    digits() {
        const choices = [String(Math.floor(this.next() * 90) + 10), String(Math.floor(this.next() * 900) + 100), String(1980 + Math.floor(this.next() * 30)), '7', '13', '22', '99', '777'];

        return this.pick(choices);
    }

    leet(text) {
        const table = { o: '0', e: '3', a: '4', i: '1', s: '5' };

        return text.replace(/[oeais]/g, letter => this.next() < 0.6 ? table[letter] : letter);
    }

    candidate() {
        const roll = this.next();
        const adjective = this.pick(words.adjectives);
        const noun = this.pick(words.nouns);

        if (roll < 0.22)
            return adjective + noun;

        if (roll < 0.34)
            return noun.toLowerCase() + this.digits();

        if (roll < 0.42)
            return `${adjective.toLowerCase()}_${noun.toLowerCase()}`;

        if (roll < 0.5)
            return `${adjective} ${noun}`;

        if (roll < 0.57) {
            const tag = this.pick(words.tags);

            return `[${tag}] ${noun}`;
        }

        if (roll < 0.63)
            return this.leet(noun) + (this.next() < 0.5 ? this.digits() : '');

        if (roll < 0.68)
            return this.pick(['x', 'o', 'its', 'the', 'mr', 'lil']) + noun;

        if (roll < 0.73)
            return noun + this.pick(['_x', 'zinho', 'BR', '_br', 'TV', 'GG', '_', 'ito']);

        if (roll < 0.79)
            return this.pick(words.infected) + this.pick(words.actions) + (this.next() < 0.25 ? this.digits() : '');

        if (roll < 0.84)
            return this.pick(words.phrases) + (this.next() < 0.3 ? this.digits() : '');

        if (roll < 0.9) {
            const length = 2 + Math.floor(this.next() * 2);
            const word = Array.from({ length }, () => this.pick(words.syllables)).join('');

            return this.next() < 0.5 ? word : word[0].toUpperCase() + word.slice(1);
        }

        if (roll < 0.95)
            return (adjective + noun).toLowerCase();

        return noun;
    }

    create() {
        for (let attempt = 0; attempt < 100000; attempt++) {
            const name = this.candidate();
            const key = name.toLowerCase();

            if (this.used.has(key) || this.reserved.has(key))
                continue;

            this.used.add(key);

            return name;
        }

        throw new Error('Could not create a new fictional name.');
    }
}

class Mapping {
    constructor(next, reservedNames) {
        this.next = next;
        this.ids = new Map();
        this.names = new Names(next, reservedNames);
        this.counts = new Map();
        this.fictional = new Map();
        this.addresses = new Map();
        this.wordNames = new Map();
    }

    countName(key, name) {
        if (name === null || name === undefined || name === '')
            return;

        if (!this.counts.has(key))
            this.counts.set(key, new Map());

        const counts = this.counts.get(key);
        const lower = name.toLowerCase();

        counts.set(lower, (counts.get(lower) || 0) + 1);
    }

    assignIds(administrators, others) {
        let account = firstAccount;

        for (const id of administrators) {
            if (this.ids.has(id))
                continue;

            this.ids.set(id, steamBase + account);
            account += 1n;
        }

        const shuffled = [...others].filter(id => !this.ids.has(id)).sort();

        for (let index = shuffled.length - 1; index > 0; index--) {
            const swap = Math.floor(this.next() * (index + 1));

            [shuffled[index], shuffled[swap]] = [shuffled[swap], shuffled[index]];
        }

        for (const id of shuffled) {
            account += 1n + BigInt(Math.floor(this.next() * 90000));
            this.ids.set(id, steamBase + account);
        }

        if (account >= steamRange)
            throw new Error('Too many players for the fictional account range.');
    }

    assignNames() {
        for (const [key, counts] of this.counts) {
            const ordered = [...counts.entries()].sort((a, b) => b[1] - a[1] || (a[0] < b[0] ? -1 : 1));
            const names = new Map();

            for (const [lower] of ordered)
                names.set(lower, this.names.create());

            this.fictional.set(key, names);
        }

        for (const [key, names] of this.fictional)
            for (const [lower, name] of names)
                if (lower.length >= 4 && !this.wordNames.has(lower))
                    this.wordNames.set(lower, name);
    }

    id(value) {
        const key = typeof value === 'bigint' ? value : BigInt(value);

        return this.ids.has(key) ? this.ids.get(key) : null;
    }

    name(key, name) {
        if (name === null || name === undefined || name === '')
            return name;

        const signIn = /^Player (\d{17})$/.exec(name);

        if (signIn) {
            const mapped = this.id(signIn[1]);

            return mapped ? `Player ${mapped}` : name;
        }

        if (!this.fictional.has(key))
            this.fictional.set(key, new Map());

        const names = this.fictional.get(key);
        const lower = name.toLowerCase();

        if (!names.has(lower))
            names.set(lower, this.names.create());

        return names.get(lower);
    }

    primary(key) {
        const names = this.fictional.get(key);

        if (names && names.size > 0)
            return names.values().next().value;

        return this.name(key, `#${key}`);
    }

    text(value) {
        if (typeof value !== 'string')
            return value;

        return value
            .replace(/\b7656\d{13}\b/g, match => {
                const mapped = this.id(match);

                return mapped ? String(mapped) : match;
            })
            .replace(/STEAM_([0-5]):([01]):(\d+)/g, (match, universe, low, high) => {
                const mapped = this.id(steamBase + BigInt(high) * 2n + BigInt(low));

                return mapped ? `STEAM_${universe}:${accountOf(mapped) % 2n}:${accountOf(mapped) / 2n}` : match;
            })
            .replace(/\[U:1:(\d+)\]/g, (match, account) => {
                const mapped = this.id(steamBase + BigInt(account));

                return mapped ? `[U:1:${accountOf(mapped)}]` : match;
            });
    }

    words(value) {
        if (typeof value !== 'string')
            return value;

        return this.text(value).split(/(\s+)/).map(token => this.wordNames.get(token.toLowerCase()) || token).join('');
    }

    address(value) {
        if (!value)
            return value;

        if (!this.addresses.has(value)) {
            const index = this.addresses.size;
            const networks = ['198.51.100', '192.0.2'];
            const network = networks[Math.floor(index / 250) % networks.length];

            this.addresses.set(value, value.includes(':') && !value.includes('.') ? `2001:db8::${(index + 1).toString(16)}` : `${network}.${(index % 250) + 2}`);
        }

        return this.addresses.get(value);
    }
}

function idKey(value) {
    return `id:${value}`;
}

function fileNameLines(content) {
    const lines = content.split('\n');
    const start = lines.findIndex(line => line.trim().startsWith('[PlayerNames'));
    const result = [];

    if (start < 0)
        return { lines: null, entries: result };

    for (let index = start + 1; index < lines.length; index++) {
        const match = /^(\d+);([^;]*);(.*?)(\r?)$/.exec(lines[index]);

        if (match)
            result.push({ index, number: match[1], steamId: match[2], name: match[3], end: match[4] });
    }

    return { lines, entries: result };
}

function steamKey(steamId) {
    const match = /^STEAM_\d:([01]):(\d+)$/.exec(steamId);

    return match ? idKey(steamBase + BigInt(match[2]) * 2n + BigInt(match[1])) : `name:${steamId}`;
}

function nameKeyFor(steamId, name) {
    const key = steamKey(steamId);

    return key.startsWith('name:') ? `name:${name.toLowerCase()}` : key;
}

function tables(db) {
    return db.prepare("select name from sqlite_master where type = 'table'").all().map(row => row.name).filter(name => !keptTables.has(name));
}

function columns(db, table) {
    return db.prepare(`pragma table_info("${table}")`).all();
}

function rows(db, table) {
    const statement = db.prepare(`select rowid as __rowid, * from "${table}"`);

    statement.setReadBigInts(true);

    return statement.all();
}

function collect(db, mapping) {
    const ids = new Set();
    const administrators = [];

    for (const table of tables(db)) {
        if (table === 'StatisticsFiles')
            continue;

        for (const row of rows(db, table)) {
            for (const [column, value] of Object.entries(row)) {
                if (column === '__rowid')
                    continue;

                if (isSteamId(value))
                    ids.add(value);

                if (typeof value === 'string')
                    for (const match of value.matchAll(/\b7656\d{13}\b/g))
                        if (isSteamId(BigInt(match[0])))
                            ids.add(BigInt(match[0]));
            }

            if (table === 'Administrators')
                administrators.push(row.CommunityId);

            const pairs = [...(nameColumns[table] ? [nameColumns[table]] : []), ...(extraNameColumns[table] || [])];

            for (const [idColumn, nameColumn] of pairs)
                if (row[idColumn] !== null && row[idColumn] !== undefined && !/^Player \d{17}$/.test(row[nameColumn] || ''))
                    mapping.countName(idKey(row[idColumn]), row[nameColumn]);
        }
    }

    const files = db.prepare('select Content from StatisticsFiles');

    for (const { Content } of files.iterate()) {
        for (const match of Content.matchAll(/STEAM_[0-5]:([01]):(\d+)/g))
            ids.add(steamBase + BigInt(match[2]) * 2n + BigInt(match[1]));

        const parsed = fileNameLines(Content);

        for (const entry of parsed.entries)
            mapping.countName(nameKeyFor(entry.steamId, entry.name), entry.name);
    }

    return { ids, administrators };
}

function rewriteFile(content, mapping) {
    const parsed = fileNameLines(content);

    if (parsed.lines) {
        for (const entry of parsed.entries)
            parsed.lines[entry.index] = `${entry.number};${entry.steamId};${mapping.name(nameKeyFor(entry.steamId, entry.name), entry.name)}${entry.end}`;

        content = parsed.lines.join('\n');
    }

    return mapping.text(content);
}

function convertTable(db, table, mapping, statements) {
    const info = columns(db, table);
    const integerColumns = info.filter(column => /INT/i.test(column.type)).map(column => column.name);
    const textColumns = info.filter(column => /TEXT|CHAR|CLOB/i.test(column.type) || column.type === '').map(column => column.name);
    const pairs = [...(nameColumns[table] ? [nameColumns[table]] : []), ...(extraNameColumns[table] || [])];
    const nameFields = new Set(pairs.map(pair => pair[1]));
    const freeText = new Set(freeTextColumns[table] || []);
    let changed = 0;

    for (const row of rows(db, table)) {
        const update = {};

        for (const [idColumn, nameColumn] of pairs)
            if (row[nameColumn] !== null && row[nameColumn] !== undefined)
                update[nameColumn] = row[idColumn] === null || row[idColumn] === undefined ? mapping.words(row[nameColumn]) : mapping.name(idKey(row[idColumn]), row[nameColumn]);

        for (const column of integerColumns)
            if (isSteamId(row[column]) && mapping.id(row[column]))
                update[column] = mapping.id(row[column]);

        for (const column of textColumns) {
            if (nameFields.has(column) || row[column] === null || row[column] === undefined)
                continue;

            if (table === 'StatisticsFiles' && column === 'Content')
                update[column] = rewriteFile(row[column], mapping);
            else if (table === 'PlayerConnections' && column === 'IpAddress')
                update[column] = mapping.address(row[column]);
            else if (freeText.has(column))
                update[column] = mapping.words(row[column]);
            else if (typeof row[column] === 'string')
                update[column] = mapping.text(row[column]);
        }

        if (table === 'PlayerProfiles')
            update.AvatarUpdatedAtUtc = new Date().toISOString().replace('T', ' ').replace('Z', '');

        const keys = Object.keys(update).filter(column => update[column] !== row[column]);

        if (keys.length === 0)
            continue;

        const sql = `update "${table}" set ${keys.map(column => `"${column}" = ?`).join(', ')} where rowid = ?`;

        if (!statements.has(sql))
            statements.set(sql, db.prepare(sql));

        statements.get(sql).run(...keys.map(column => update[column]), row.__rowid);
        changed++;
    }

    return changed;
}

function convertServers(db, mapping) {
    const servers = rows(db, 'GameServers').sort((a, b) => (a.Id < b.Id ? -1 : 1));
    const renamed = [];

    servers.forEach((server, index) => {
        const number = index + 1;
        const name = `L4D2 Competitive #${number}`;
        const publicId = `l4d2-competitive-${number}`;
        const port = 27014 + number;

        db.prepare('update GameServers set Name = ?, PublicId = ?, Address = ?, AlternativeAddress = ? where Id = ?')
            .run(name, publicId, `203.0.113.10:${port}`, server.AlternativeAddress ? `203.0.113.11:${port}` : null, server.Id);
        db.prepare('update RemoteCommandLogs set GameServerName = ? where GameServerId = ?').run(name, server.Id);
        renamed.push({ from: server.PublicId, to: publicId });
    });

    return renamed;
}

function convertSettings(db, mapping, administrator) {
    const statement = db.prepare('select Key, Value from Settings');
    let changed = 0;

    for (const row of statement.all()) {
        const value = mapping.text(row.Value).replace(/steamcommunity\.com\/id\/[A-Za-z0-9_-]+/g, `steamcommunity.com/profiles/${administrator}`);

        if (value !== row.Value) {
            db.prepare('update Settings set Value = ? where Key = ?').run(value, row.Key);
            changed++;
        }
    }

    return changed;
}

function initials(name) {
    const parts = name.replace(/[\[\]_]/g, ' ').replace(/([a-z])([A-Z])/g, '$1 $2').split(/\s+/).filter(part => /[A-Za-z0-9]/.test(part));
    const letters = parts.map(part => part.replace(/[^A-Za-z0-9]/g, '')[0]).filter(Boolean);

    return (letters.length > 1 ? letters[0] + letters[1] : (parts[0] || '?').replace(/[^A-Za-z0-9]/g, '').slice(0, 2)).toUpperCase() || '?';
}

const drawAvatar = `(items => items.map(item => {
    const size = 184;
    const center = size / 2;
    const canvas = document.createElement('canvas');
    canvas.width = size;
    canvas.height = size;
    const context = canvas.getContext('2d');
    const gradient = context.createLinearGradient(0, 0, size, size);
    gradient.addColorStop(0, 'hsl(' + item.hue + ', 62%, ' + item.light + '%)');
    gradient.addColorStop(1, 'hsl(' + ((item.hue + item.shift) % 360) + ', 68%, ' + (item.light - 16) + '%)');
    context.fillStyle = gradient;
    context.fillRect(0, 0, size, size);
    context.fillStyle = 'rgba(255, 255, 255, 0.88)';
    context.strokeStyle = 'rgba(255, 255, 255, 0.88)';
    context.lineWidth = 12;
    let bits = item.bits;
    if (item.style === 'initials') {
        context.font = 'bold ' + (item.text.length > 1 ? 76 : 96) + 'px Segoe UI, Arial, sans-serif';
        context.textAlign = 'center';
        context.textBaseline = 'middle';
        context.fillText(item.text, center, center + 6);
    } else if (item.style === 'pixels') {
        const cell = 28;
        const start = (size - cell * 5) / 2;
        for (let row = 0; row < 5; row++)
            for (let column = 0; column < 3; column++) {
                const on = bits % 2 === 1;
                bits = Math.floor(bits / 2);
                if (!on)
                    continue;
                context.fillRect(start + column * cell, start + row * cell, cell, cell);
                context.fillRect(start + (4 - column) * cell, start + row * cell, cell, cell);
            }
    } else if (item.style === 'circle') {
        context.beginPath();
        context.arc(center, center, 50, 0, Math.PI * 2);
        context.fill();
    } else if (item.style === 'ring') {
        context.beginPath();
        context.arc(center, center, 48, 0, Math.PI * 2);
        context.stroke();
    } else if (item.style === 'diamond') {
        context.beginPath();
        context.moveTo(center, 36);
        context.lineTo(size - 36, center);
        context.lineTo(center, size - 36);
        context.lineTo(36, center);
        context.closePath();
        context.fill();
    } else if (item.style === 'triangle') {
        context.beginPath();
        context.moveTo(center, 40);
        context.lineTo(size - 40, size - 50);
        context.lineTo(40, size - 50);
        context.closePath();
        context.fill();
    } else {
        context.beginPath();
        for (let point = 0; point < 10; point++) {
            const radius = point % 2 === 0 ? 60 : 25;
            const angle = -Math.PI / 2 + point * Math.PI / 5;
            context.lineTo(center + radius * Math.cos(angle), center + radius * Math.sin(angle));
        }
        context.closePath();
        context.fill();
    }
    return canvas.toDataURL('image/jpeg', 0.9).split(',')[1];
}))`;

async function drawAvatars(folder, players, next, browser) {
    if (players.length === 0)
        return 0;

    const { launch } = require('./cdp');
    const page = await launch({ browser });
    const styles = ['initials', 'initials', 'initials', 'initials', 'pixels', 'pixels', 'pixels', 'circle', 'ring', 'diamond', 'triangle', 'star'];

    try {
        fs.mkdirSync(folder, { recursive: true });

        for (let start = 0; start < players.length; start += 40) {
            const batch = players.slice(start, start + 40).map(player => ({
                id: String(player.id),
                text: initials(player.name),
                hue: Math.floor(next() * 360),
                shift: 20 + Math.floor(next() * 80),
                light: 42 + Math.floor(next() * 18),
                bits: 1 + Math.floor(next() * 32766),
                style: styles[Math.floor(next() * styles.length)]
            }));
            const images = await page.evaluate(`${drawAvatar}(${JSON.stringify(batch)})`);

            batch.forEach((item, index) => fs.writeFileSync(path.join(folder, `${item.id}.jpg`), Buffer.from(images[index], 'base64')));
        }
    } finally {
        await page.close();
    }

    return players.length;
}

function inUse(file) {
    if (process.platform === 'win32') {
        try {
            fs.renameSync(file, file);
        } catch (error) {
            return ['EBUSY', 'EPERM', 'EACCES'].includes(error.code);
        }

        return false;
    }

    if (process.platform !== 'linux' || !fs.existsSync('/proc'))
        return false;

    const real = fs.realpathSync(file);

    for (const pid of fs.readdirSync('/proc').filter(name => /^\d+$/.test(name))) {
        try {
            for (const descriptor of fs.readdirSync(`/proc/${pid}/fd`))
                if (fs.readlinkSync(`/proc/${pid}/fd/${descriptor}`) === real)
                    return true;
        } catch {
        }
    }

    return false;
}

function isEmptyFolder(folder) {
    return !fs.existsSync(folder) || (fs.statSync(folder).isDirectory() && fs.readdirSync(folder).length === 0);
}

async function main() {
    const options = parse(process.argv.slice(2));

    if (!options) {
        usage();
        process.exit(1);
    }

    const from = path.resolve(options.from);
    const to = path.resolve(options.to);
    const source = path.join(from, 'hub.db');
    const target = path.join(to, 'hub.db');

    if (from === to || to.startsWith(from + path.sep) || from.startsWith(to + path.sep))
        throw new Error('--from and --to must be two separate folders.');

    if (!fs.existsSync(source))
        throw new Error(`There is no hub.db in ${from}.`);

    if (!isEmptyFolder(to))
        throw new Error(`${to} is not empty. Give a new folder in --to, so no data folder in use is ever changed.`);

    if (inUse(source))
        throw new Error(`${source} is open in another program, probably the site. Stop the site, or copy the folder first and give the copy in --from.`);

    const seed = options.seed || crypto.randomBytes(16).toString('hex');
    const next = random(seed);

    fs.mkdirSync(to, { recursive: true });

    const reader = new DatabaseSync(source, { readOnly: true });

    await backup(reader, target);
    reader.close();

    const db = new DatabaseSync(target, { enableForeignKeyConstraints: false });

    db.exec('PRAGMA busy_timeout = 0');
    db.exec('PRAGMA locking_mode = EXCLUSIVE');
    db.exec('BEGIN EXCLUSIVE');

    const reserved = new Set();
    const mapping = new Mapping(next, reserved);
    const { ids, administrators } = collect(db, mapping);

    for (const counts of mapping.counts.values())
        for (const lower of counts.keys())
            reserved.add(lower);

    mapping.assignIds(administrators, ids);
    mapping.assignNames();

    const report = {};
    const statements = new Map();

    for (const table of tables(db))
        if (table !== 'Settings' && table !== 'GameServers')
            report[table] = convertTable(db, table, mapping, statements);

    const administrator = mapping.id(administrators[0] || [...ids][0]);
    const servers = convertServers(db, mapping);

    report.Settings = convertSettings(db, mapping, administrator);
    db.exec('COMMIT');
    db.exec('VACUUM');
    db.exec('PRAGMA wal_checkpoint(TRUNCATE)');

    const withAvatars = fs.existsSync(path.join(from, 'avatars'))
        ? fs.readdirSync(path.join(from, 'avatars')).map(file => /^(\d{17})\.jpg$/.exec(file)).filter(Boolean).map(match => BigInt(match[1])).filter(id => mapping.id(id))
        : [];
    const profiles = db.prepare('select CommunityId, Name from PlayerProfiles');

    profiles.setReadBigInts(true);

    const profileNames = new Map(profiles.all().map(row => [row.CommunityId, row.Name]));
    const players = withAvatars.map(id => ({ id: mapping.id(id), name: profileNames.get(mapping.id(id)) || mapping.primary(idKey(id)) }));
    const playerProfiles = db.prepare('select count(*) as count from PlayerProfiles').get().count;

    db.close();

    if (fs.existsSync(path.join(from, 'branding')))
        fs.cpSync(path.join(from, 'branding'), path.join(to, 'branding'), { recursive: true });

    const avatars = await drawAvatars(path.join(to, 'avatars'), players, next, options.browser);

    console.log(`Fictional data written to ${to}`);
    console.log(`Seed: ${seed}`);
    console.log(`Players: ${ids.size} SteamIDs, ${playerProfiles} profiles, ${avatars} avatars drawn`);
    console.log(`Rows changed: ${Object.entries(report).filter(([, count]) => count > 0).map(([table, count]) => `${table} ${count}`).join(', ')}`);
    console.log(`Administrators: ${administrators.map(id => String(mapping.id(id))).join(', ')}`);

    for (const server of servers)
        console.log(`Game server ${server.from} is now ${server.to}: its token is now ${server.to}:<the same key>`);

    console.log('Next: start the site on the new folder and run Administration > Overview > "Reprocess all matches".');
}

main().catch(error => {
    console.error(error.message);
    process.exit(1);
});

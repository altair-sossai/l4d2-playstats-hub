# Notice

The plugins in this folder are released under the GNU General Public License, version 3. The full text is in [LICENSE](LICENSE).

## Written for this project

| Files | Author |
|---|---|
| `scripting/l4d2_playstats_sync.sp`, `scripting/l4d2_gameinfo_sync.sp`, `scripting/l4d2_ranking.sp`, `scripting/l4d2_show_patent_icon.sp`, `scripting/l4d2_weapon_skins.sp` | Altair Sossai. Moved here from [l4d2-zone-server](https://github.com/altair-sossai/l4d2-zone-server). |
| `scripting/include/l4d2_playstats_hub.inc` | Altair Sossai. |
| `translations/**`, `configs/weapon_skins.cfg`, `cfg/playstats.cfg.example` | Altair Sossai. |

## Third-party include files

These files are needed only to compile the plugins. They are copied without changes, and each one keeps the license and the copyright notices of its project.

| Files in `scripting/include/` | Project | Source |
|---|---|---|
| `ripext.inc`, `ripext/http.inc`, `ripext/json.inc` | REST in Pawn, by Erik Minekus | <https://github.com/ErikMinekus/sm-ripext> |
| `left4dhooks.inc`, `left4dhooks_anim.inc`, `left4dhooks_lux_library.inc`, `left4dhooks_silver.inc`, `left4dhooks_stocks.inc` | Left 4 DHooks Direct, by SilverShot, based on Left 4 Downtown 2 | <https://github.com/SilvDev/Left4DHooks> |
| `colors.inc` | Colored Chat Functions, by exvel, edited by Popoklopsi, Powerlord and Bara | <https://github.com/SirPlease/L4D2-Competitive-Rework> |
| `readyup.inc`, `pause.inc` | L4D2 Ready-Up and Pause, from L4D2 Competitive Rework | <https://github.com/SirPlease/L4D2-Competitive-Rework> |
| `l4d2util.inc`, `l4d2util_constants.inc`, `l4d2util_infected.inc`, `l4d2util_rounds.inc`, `l4d2util_stocks.inc`, `l4d2util_survivors.inc`, `l4d2util_tanks.inc`, `l4d2util_weapons.inc` | L4D2Util, from L4D2 Competitive Rework | <https://github.com/SirPlease/L4D2-Competitive-Rework> |
| `l4d2_hybrid_scoremod.inc` | L4D2 Scoremod+, by Visor, from L4D2 Competitive Rework | <https://github.com/SirPlease/L4D2-Competitive-Rework> |
| `l4d2_skill_detect.inc` | L4D2 Skill Detect, by Tabun, from L4D2 Competitive Rework | <https://github.com/SirPlease/L4D2-Competitive-Rework> |
| `l4d2_boss_percents.inc` | L4D2 Boss Flow Announce, by spoon | <https://github.com/spoon-l4d2/Plugins> |
| `l4d2_anti_cheat.inc` | L4D2 Anti-Cheat plugin, by Altair Sossai | <https://github.com/altair-sossai/l4d2-zone-server> |

The copies were taken from [l4d2-zone-server](https://github.com/altair-sossai/l4d2-zone-server), folder `addons/sourcemod/scripting/include`.

The standard SourceMod include files are not copied here. The build scripts take them from the SourceMod package, <https://www.sourcemod.net>.

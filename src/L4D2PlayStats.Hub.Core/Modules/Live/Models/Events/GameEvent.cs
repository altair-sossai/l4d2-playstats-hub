using System.Text.Json.Serialization;
using L4D2PlayStats.Hub.Core.Modules.Live.Enums;
using L4D2PlayStats.Hub.Core.Modules.Live.Extensions;

namespace L4D2PlayStats.Hub.Core.Modules.Live.Models.Events;

[JsonPolymorphic(
    TypeDiscriminatorPropertyName = "type",
    IgnoreUnrecognizedTypeDiscriminators = true,
    UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToBaseType)]
[JsonDerivedType(typeof(BoomerVomitLandedEvent), "boomerVomitLanded")]
[JsonDerivedType(typeof(CarAlarmTriggeredEvent), "carAlarmTriggered")]
[JsonDerivedType(typeof(ChargerLevelEvent), "chargerLevel")]
[JsonDerivedType(typeof(ChargerLevelHurtEvent), "chargerLevelHurt")]
[JsonDerivedType(typeof(DeathChargeEvent), "deathCharge")]
[JsonDerivedType(typeof(HunterHighPounceEvent), "hunterHighPounce")]
[JsonDerivedType(typeof(MixStartedEvent), "mixStarted")]
[JsonDerivedType(typeof(PauseEvent), "pause")]
[JsonDerivedType(typeof(PlayerDeathEvent), "playerDeath")]
[JsonDerivedType(typeof(RoundEndedEvent), "roundEnded")]
[JsonDerivedType(typeof(RoundLiveEvent), "roundLive")]
[JsonDerivedType(typeof(SkeetEvent), "skeet")]
[JsonDerivedType(typeof(SkeetHurtEvent), "skeetHurt")]
[JsonDerivedType(typeof(TankBecameBotEvent), "tankBecameBot")]
[JsonDerivedType(typeof(TankDiedEvent), "tankDied")]
[JsonDerivedType(typeof(TankRockEatenEvent), "tankRockEaten")]
[JsonDerivedType(typeof(TankRockSkeetedEvent), "tankRockSkeeted")]
[JsonDerivedType(typeof(TankSpawnedEvent), "tankSpawned")]
[JsonDerivedType(typeof(TongueCutEvent), "tongueCut")]
[JsonDerivedType(typeof(WitchCrownEvent), "witchCrown")]
[JsonDerivedType(typeof(WitchCrownHurtEvent), "witchCrownHurt")]
public class GameEvent : FeedItem
{
    [JsonIgnore]
    public virtual GameEventType EventType => GameEventType.Unknown;

    public virtual int Stars => EventType.Stars();

    public LivePlayer? Actor { get; set; }
}

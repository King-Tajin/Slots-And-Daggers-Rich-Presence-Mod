using System.Collections.Generic;

namespace DiscordPresence
{
    internal static class EnemyAssets
    {
        internal static readonly HashSet<string> Known = new HashSet<string>
        {
            "archie", "bandit", "bogner", "brollegg", "burster", "crabtaur", "deathtrout", "deviled-egg", "duelist", "egg-of-kul",
            "egg-queen", "eggforcer", "egglancer", "eggo-arena", "eggogryph", "eggomancer", "eggron", "eggscalibur", "egguana", "eggzerker",
            "fangris", "frygg", "golden-egg", "guardian-of-the-seven", "half-yolk", "hammer-goblin", "hechtia", "helga", "high-priest", "horn-egg",
            "horn-goblin", "ike", "king-benedict-iv", "king-komodo", "king-lino-i", "knight-of-baghar", "knight-of-kul", "leggo", "lesser-priest", "lich-egg",
            "lich-hound", "lobbus", "lord-slotor", "lower-clerk", "mothgul", "mud-fairy", "nail-head-gang", "obsidius", "pine-lurcher", "rotter",
            "shell-guard", "shellcaster", "shellwarden", "shredder-3000", "sir-otto", "skinless-walker", "slasher-goblin", "snake-egg", "snatcher", "soil-egg",
            "solandra", "spike-egg", "terrorwing", "the-arm", "the-blosh", "the-butcher-of-kul", "the-cleaver", "the-deceiver", "the-enforcer", "the-gunslinger",
            "the-minister", "the-queen-of-snakes", "the-rider-of-kul", "the-shard-thing", "the-triserpent", "tunnel-egg", "vicar-longlegs", "yolk-elemental", "yolk-trooper", "yolkbot",
            "yolker"
        };

        internal static readonly Dictionary<string, string> Aliases = new Dictionary<string, string>
        {
            { "eggo", "eggo-arena" }
        };
    }
}

using System.ComponentModel;
using Exiled.API.Interfaces;

namespace ZombieSuicideHotline
{
    public class SuicideConfig : IConfig {
        [Description("Is the plugin enabled?")]
        public bool IsEnabled { get; set; } = true;

        [Description("How much do Teslas and Drops deal to zombies?")]
        public float ZombieDmg { get; set; } = 0;
    }
}

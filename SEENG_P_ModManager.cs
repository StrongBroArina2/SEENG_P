namespace SEENG_Core
{
    public class ModManager
    {
        public static WorkshopMod LastSelectedMod { get; set; }

        public static void UpdateMod(WorkshopMod mod)
        {/// i love skybox loader
            LastSelectedMod = mod;
        }
    }
}
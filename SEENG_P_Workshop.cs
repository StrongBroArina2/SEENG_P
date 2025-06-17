namespace SEENG_Core
{
    public class WorkshopMod
    {
        public string Name { get; }
        public string ModPath { get; }
        public string ArcShipSysPath { get; }
        public string AudioSbcPath { get; }

        public WorkshopMod(string name, string modPath, string arcShipSysPath, string audioSbcPath)
        {
            Name = name;
            ModPath = modPath;
            ArcShipSysPath = arcShipSysPath;
            AudioSbcPath = audioSbcPath;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
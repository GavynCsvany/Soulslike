namespace Soulslike.Player.Stats
{
    public class PlayerStats
    {
        
        // Base stats
        public int Vigor = 10;
        public int Endurance = 10;
        
        // Derived base stats
        public float Health => Vigor * 10;
        public float MaxHealth => Endurance * 10;
        public float Stamina => Endurance * 7.5f;
        public float MaxStamina => Endurance * 7.5f;
        
    }
}
using Godot;

namespace TideyUp.Utils
{
    public static class Random
    {
        private static RandomNumberGenerator rng = null;
        private static RandomNumberGenerator RNG
        { 
            get {
                if(rng == null)
                {
                    rng = new RandomNumberGenerator();
                    rng.Randomize();
                }
                return rng;
            }
        }
        public static int Range(int min, int max)
        {
            return (int)((RNG.Randi() % (max - min)) + min);
        }

        public static float Range(float min, float max)
        {
            return (RNG.Randf() * (max - min)) + min;
        }

        public static float Value()
        {
            return RNG.Randf();
        }
    }
}
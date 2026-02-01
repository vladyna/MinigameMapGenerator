using UnityEngine;
namespace Generator.Core
{
    public class SeedController
    {
        private int _seed;

        public int Seed
        {
            get { return _seed; }
        }

        public SeedController()
        {
            SetRandomSeed();
        }

        public void SetSeed(int seed)
        {
            _seed = seed;
        }

        public void SetRandomSeed()
        {
            _seed = Random.Range(int.MinValue, int.MaxValue);
        }

    }
}

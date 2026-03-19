// CheckpointData.cs
// A clean container for everything the checkpoint needs to remember.
// this can be serialized to disk for a full save system in the future

namespace Demo.Systems
{
    public class CheckpointData
    {
        public bool HasData { get; private set; } = false;
        public float Health { get; private set; }
        public UnityEngine.Vector2 Position { get; private set; }

        public void Save(float currentHealth, UnityEngine.Vector2 position)
        {
            Health = currentHealth;
            Position = position;
            HasData = true;
        }

        public void Clear()
        {
            HasData = false;
        }
    }
}
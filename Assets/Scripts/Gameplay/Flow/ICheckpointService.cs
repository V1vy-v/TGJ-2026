namespace Gameplay.Flow
{
    public static class CheckpointIds
    {
        public const string P0_1 = "P0_1";
        public const string P0_2 = "P0_2";
        public const string P1_1 = "P1_1";
        public const string P1_1_AfterNpc1 = "P1_1 (1)";
        public const string P1_1_After1_3 = "P1_1 (2)";
        public const string P1_1_After1_4 = "P1_1 (3)";
        public const string P2_1 = "P2_1";
        public const string P2_1_After2_2 = "P2_1 (1)";
        public const string P2_1_After2_3 = "P2_1 (2)";
        public const string P2_1_AfterTeleport4 = "P2_1 (4)";
        public const string P2_1_AfterTeleport5 = "P2_1 (5)";
    }

    public interface ICheckpointService
    {
        string CurrentCheckpoint { get; }
        void Unlock(string checkpointId);
        void Respawn();
    }
}

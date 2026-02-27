using AES.Tools;


namespace Domain
{
    public sealed class SceneStartArgs : ISceneArgs
    {
        public int StageNumber { get; }

        public static readonly SceneStartArgs Default = new(1);

        public SceneStartArgs(int stageNumber)
        {
            StageNumber = stageNumber;
        }
    }
}
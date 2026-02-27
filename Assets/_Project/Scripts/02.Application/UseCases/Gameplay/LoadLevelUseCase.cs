using App.Results;
using Cysharp.Threading.Tasks;
using Domain;

namespace App.Gameplay
{
    public sealed class LoadLevelUseCase
    {
        private readonly ILevelConfigRepository _levelConfigRepository;
        private readonly IBoardRepository _boardRepository;
        private readonly IMoveHistoryRepository _moveHistoryRepository;

        public LoadLevelUseCase(
            ILevelConfigRepository levelConfigRepository,
            IBoardRepository boardRepository,
            IMoveHistoryRepository moveHistoryRepository)
        {
            _levelConfigRepository = levelConfigRepository;
            _boardRepository       = boardRepository;
            _moveHistoryRepository = moveHistoryRepository;
        }

        public async UniTask<LoadLevelResult> ExecuteAsync(int levelId, int variant = 0)
        {
            // 1) LevelConfig를 Addressables에서 로드
            var config = await _levelConfigRepository.LoadAsync(levelId, variant);

            // 2) 도메인 보드 생성
            var board = BoardFactory.CreateFromConfig(config);
            _boardRepository.Save(board);
            _moveHistoryRepository.Save(new MoveHistory());

            // 3) DTO 변환
            var layoutDto = LevelConfigMapper.ToLayoutDto(config);
            var boardDto  = BoardMapper.ToDto(board);

            return new LoadLevelResult(layoutDto, boardDto, config.Difficulty);
        }
    }
}
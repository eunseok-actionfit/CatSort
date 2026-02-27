using System;
using System.IO;
using AES.Tools;
using App.State;
using Domain;
using SimpleFileBrowser;
using UnityEngine;


namespace App.Mvvm
{
    public sealed class LevelEditorViewModel
    {
        public const string LevelIdFormat = "level_{0:000}";
        public const string LevelIdPrefix = "level_";
        
        private const string LastLevelPathKey = "LEVEL_EDITOR_LAST_PATH";


        private readonly BoardViewModel _gameBoardVm;
        private readonly IBoardRepository _boardRepository;
        private readonly IMoveHistoryRepository _historyRepository;

        public ObservableProperty<int> Level { get; } = new();

        public ObservableProperty<LevelConfigViewModel> CurrentLevel { get; } = new();
        public ObservableProperty<GameStatus> Status { get; } = new();

        public event Action RefreshPreviewRequest = delegate { };

        // 커맨드들
        public ICommand NewLevelCommand { get; }
        public ICommand PlayCommand { get; }
        public ICommand StopCommand { get; }

        public ICommand SaveAsJsonCommand { get; }
        public ICommand LoadFromJsonCommand { get; }

        public LevelEditorViewModel(
            BoardViewModel gameBoardVm,
            IBoardRepository boardRepository,
            IMoveHistoryRepository historyRepository)
        {
            _gameBoardVm = gameBoardVm;
            _boardRepository = boardRepository;
            _historyRepository = historyRepository;

            //_gameBoardVm.ResetBoard();

            OnNewLevel();
            // // 처음엔 빈 레벨 or 디폴트 레벨 하나 생성
            // var levelVm = CreateDefaultLevel();
            // CurrentLevel.Value = levelVm;
            //
            // WireLevel(levelVm); // 변경 이벤트 연결
            // ApplyCurrentConfigToGameAsPreview();


            NewLevelCommand = new Command(
                execute: OnNewLevel,
                canExecute: () => Status.Value != GameStatus.Playing
            );

            // 심플파일브라우저를 이용한 JSON Save/Load 커맨드
            SaveAsJsonCommand = new Command(
                execute: OnSaveAsJson,
                canExecute: () => CurrentLevel.Value != null
                                  && Status.Value != GameStatus.Playing
            );

            LoadFromJsonCommand = new Command(
                execute: OnLoadFromJson,
                canExecute: () => Status.Value != GameStatus.Playing
            );

            PlayCommand = new Command(
                execute: OnPlay,
                canExecute: () => CurrentLevel.Value != null
                                  && Status.Value != GameStatus.Playing);

            StopCommand = new Command(
                execute: OnStop,
                canExecute: () => Status.Value == GameStatus.Playing
            );
        }

        private void WireLevel(LevelConfigViewModel levelVm)
        {
            // 레이아웃 필드 변경 시
            levelVm.LayoutType.Subscribe(_ => TryRefreshPreview());
            levelVm.SpacingX.Subscribe(_ => TryRefreshPreview());
            levelVm.SpacingY.Subscribe(_ => TryRefreshPreview());
            levelVm.GridRowCount.Subscribe(_ => TryRefreshPreview());
            levelVm.ArcRadius.Subscribe(_ => TryRefreshPreview());
            levelVm.ArcStartAngle.Subscribe(_ => TryRefreshPreview());
            levelVm.ArcEndAngle.Subscribe(_ => TryRefreshPreview());

            // 컬럼 리스트 변경 시
            levelVm.Columns.ItemAdded += OnColumnAdded;
            levelVm.Columns.ItemRemoved += OnColumnRemoved;

            // 이미 있는 컬럼들도 묶기
            foreach (var col in levelVm.Columns)
                WireColumn(col);
        }

        private void OnColumnAdded(int index, ColumnConfigViewModel colVm)
        {
            Debug.Log("OnColumnAdded: " + colVm.Id.Value + "");
            WireColumn(colVm);
            TryRefreshPreview();
        }

        private void OnColumnRemoved(int index, ColumnConfigViewModel colVm)
        {
            TryRefreshPreview();
        }

        private void WireColumn(ColumnConfigViewModel colVm)
        {
            colVm.Capacity.Subscribe(_ => TryRefreshPreview());
            colVm.BehaviorType.Subscribe(_ => TryRefreshPreview());
            colVm.LockColor.Subscribe(_ => TryRefreshPreview());

            colVm.Cats.ItemAdded += OnCatAdded;
            colVm.Cats.ItemRemoved += OnCatRemoved;

            foreach (var catVm in colVm.Cats)
                WireCat(catVm);
        }

        private void OnCatAdded(int index, CatConfigViewModel catVm)
        {
            WireCat(catVm);
            TryRefreshPreview();
        }

        private void OnCatRemoved(int index, CatConfigViewModel catVm)
        {
            TryRefreshPreview();
        }

        private void WireCat(CatConfigViewModel catVm)
        {
            catVm.Color.Subscribe(_ => TryRefreshPreview());
            catVm.IsHidden.Subscribe(_ => TryRefreshPreview());
        }

        private void OnNewLevel()
        {
            var vm = CreateDefaultLevel();
            CurrentLevel.Value = vm;
            WireLevel(vm);
            
            _gameBoardVm.ResetBoard();  

            // 바로 게임 보드 쪽도 미리보기로 갱신
            ApplyCurrentConfigToGameAsPreview();
        }

        private LevelConfigViewModel CreateDefaultLevel()
        {
            var vm = new LevelConfigViewModel();

            var config = LevelConfigFactory.CreateDefaultConfig();
            vm.FromConfig(config);
            return vm;
        }

        private void TryRefreshPreview()
        {
            // 플레이 중이면 프리뷰 갱신 금지
            if (Status.Value == GameStatus.Playing)
                return;

            ApplyCurrentConfigToGameAsPreview();
            RefreshPreviewRequest.Invoke();
        }

        private int ParseLevelId(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                Debug.LogError("[LevelConfigViewModel] LevelId 텍스트가 비어 있습니다.");
                return 0;
            }

            text = text.Trim();

            if (text.StartsWith(LevelIdPrefix, StringComparison.OrdinalIgnoreCase))
            {
                text = text.Substring(LevelIdPrefix.Length); // "001"
            }

            if (!int.TryParse(text, out var id)) { return 0; }

            return id;
        }


        private void ApplyCurrentConfigToGameAsPreview()
        {
            var levelVm = CurrentLevel.Value;
            if (levelVm == null)
                return;

            var config = levelVm.ToConfig();

            // 도메인 보드 생성
            var board = BoardFactory.CreateFromConfig(config);
            _boardRepository.Save(board);
            _historyRepository.Save(new MoveHistory());

            // 상태 + 레이아웃 VM에 반영
            var stateDto = BoardMapper.ToDto(board);
            var layoutDto = LevelConfigMapper.ToLayoutDto(config);

            _gameBoardVm.ApplyState(stateDto);
            _gameBoardVm.ApplyLayout(layoutDto);

            // 프리뷰 모드이므로 입력은 막아둠
            ChangeState(GameStatus.Editing);
        }

        private void OnPlay()
        {
            // 플레이 시작 직전에 한 번 프리뷰를 강제 동기화
            ApplyCurrentConfigToGameAsPreview();
            ChangeState(GameStatus.Playing);
        }

        private void OnStop()
        {
            ChangeState(GameStatus.Editing);
            
            //플레이 도중 변경된 보드 상태/캐시 싹 비우기
            _gameBoardVm.ResetBoard();
            // 플레이에서 만든 상태는 버리고, 설계 상태로 되돌림
            ApplyCurrentConfigToGameAsPreview();
        }

        public void ChangeState(GameStatus status)
        {
            Status.Value = status;
            _gameBoardVm.Status.Value = status;
        }

        // JSON 저장 다이얼로그
        private void OnSaveAsJson()
        {
            var levelVm = CurrentLevel.Value;
            if (levelVm == null)
                return;

            var config = levelVm.ToConfig();

            // 1) Level 값 기준으로 기본 LevelId를 한 번 세팅해준다
            //    - JSON에 이미 LevelId가 있으면 그대로 사용
            if (Level.Value >= 0 && string.IsNullOrEmpty(config.LevelId))
            {
                config.LevelId = string.Format(LevelIdFormat, Level.Value);
            }

            // 2) 기본 폴더: 마지막 사용 경로나, 없으면 persistentDataPath/Levels
            var initialPath = GetInitialDirectory();

            // 3) 초기 파일명은 현재 LevelId 또는 기본값
            var initialName = string.IsNullOrEmpty(config.LevelId)
                ? "NewLevel"
                : config.LevelId;

            FileBrowser.SetFilters(
                showAllFilesFilter: false,
                new FileBrowser.Filter("JSON", ".json")
            );
            FileBrowser.SetDefaultFilter(".json");

            FileBrowser.ShowSaveDialog(
                onSuccess: paths =>
                {
                    if (paths == null || paths.Length == 0)
                        return;

                    var path = paths[0];

                    // 확장자 없으면 .json 붙여주기
                    if (string.IsNullOrEmpty(Path.GetExtension(path)))
                        path += ".json";

                    // 실제 저장될 파일 이름(확장자 제거)
                    var fileNameWithoutExt = Path.GetFileNameWithoutExtension(path);

                    // 파일 이름에서 level_번호 패턴을 파싱해서 Level/LevelId 갱신
                    var parsedId = ParseLevelId(fileNameWithoutExt);
                    if (parsedId > 0)
                    {
                        Level.Value = parsedId;
                        config.LevelId = string.Format(LevelIdFormat, parsedId);
                    }

                    var json = JsonUtility.ToJson(config, prettyPrint: true);
                    File.WriteAllText(path, json);

                    // 마지막 경로 저장
                    SaveLastPath(path);
                },
                onCancel: () => { },
                pickMode: FileBrowser.PickMode.Files,
                initialPath: initialPath,
                initialFilename: initialName + ".json",
                title: "Save Level JSON",
                saveButtonText: "Save"
            );
        }


        // JSON 로드 다이얼로그
        private void OnLoadFromJson()
        {
            // 기본 폴더: 마지막 사용 경로나, 없으면 persistentDataPath/Levels
            var initialPath = GetInitialDirectory();

            FileBrowser.SetFilters(
                showAllFilesFilter: false,
                new FileBrowser.Filter("JSON", ".json")
            );

            FileBrowser.SetDefaultFilter(".json");

            FileBrowser.ShowLoadDialog(
                onSuccess: paths => {
                    if (paths == null || paths.Length == 0)
                        return;

                    var path = paths[0];
                    if (!File.Exists(path))
                        return;

                    // 마지막 경로 저장
                    SaveLastPath(path);

                    var json = File.ReadAllText(path);
                    var config = JsonUtility.FromJson<LevelConfig>(json);
                    if (config == null)
                        return;

                    // 새 VM으로 갈아끼우기
                    var vm = new LevelConfigViewModel();
                    vm.FromConfig(config);
                    CurrentLevel.Value = vm;

                    var id = ParseLevelId(config.LevelId);
                    if (id == 0)
                    {
                        // config.LevelId에서 못 뽑으면 파일 이름에서 한 번 더 시도
                        var fileName = Path.GetFileNameWithoutExtension(path);
                        id = ParseLevelId(fileName);
                    }
                    Level.Value = id;

                    // 레벨 내부 와이어링/프리뷰
                    _gameBoardVm.ResetBoard();  
                    WireLevelIfNeeded(vm);
                    ApplyCurrentConfigToGameAsPreview();
                },
                onCancel: () => { },
                pickMode: FileBrowser.PickMode.Files,
                initialPath: initialPath,
                title: "Load Level JSON",
                loadButtonText: "Load"
            );
        }

        private void WireLevelIfNeeded(LevelConfigViewModel vm)
        {
            try { WireLevel(vm); }
            catch (Exception ex)
            {
                Debug.LogError($"WireLevel 실패: {ex}");
                throw;
            }
        }
        
        // 마지막 경로 가져오기 (없으면 기본 Levels 폴더)
        private string GetInitialDirectory()
        {
            var saved = PlayerPrefs.GetString(LastLevelPathKey, string.Empty);
            if (!string.IsNullOrEmpty(saved))
            {
                try
                {
                    string dir = null;

                    if (Directory.Exists(saved))
                    {
                        dir = saved;
                    }
                    else if (File.Exists(saved))
                    {
                        dir = Path.GetDirectoryName(saved);
                    }

                    if (!string.IsNullOrEmpty(dir))
                    {
                        Directory.CreateDirectory(dir);
                        return dir;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"GetInitialDirectory 실패, 기본 경로로 fallback: {e}");
                }
            }

            // fallback: persistentDataPath/Levels
            var fallback = Path.Combine(Application.persistentDataPath, "Levels");
            Directory.CreateDirectory(fallback);
            return fallback;
        }

        // 마지막 경로 저장 (파일을 넘겨도 폴더만 저장)
        private void SaveLastPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;

            string dirToSave = null;

            if (Directory.Exists(path))
            {
                dirToSave = path;
            }
            else
            {
                var dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir))
                    dirToSave = dir;
            }

            if (string.IsNullOrEmpty(dirToSave))
                return;

            PlayerPrefs.SetString(LastLevelPathKey, dirToSave);
            PlayerPrefs.Save();
        }


    }
}
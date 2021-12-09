using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using Febucci.UI;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameManager : MonoBehaviour
{
    [SerializeField, Range(0f, 0.3f)] private float _displayDelay;
    [SerializeField] private GenerationLevel _difficulty;
    [SerializeField] private GenerationLevel _starterCells;
    [Space]
    [SerializeField] private Transform _minefieldObject;
    [SerializeField] private bool _showDebugValues;
    [SerializeField] private GameObject _generationBlocker;
    [SerializeField] private Animator _helpPanel;
    [Space]
    [SerializeField] private TMP_Dropdown _difficultyDropdown;
    [SerializeField] private TMP_Dropdown _starterCellsDropdown;
    [Space]
    [SerializeField] private TextMeshProUGUI _scoreCount;
    [SerializeField] private TextMeshProUGUI _hiscoreCount;
    [SerializeField] private TextMeshProUGUI _winCount;
    [SerializeField] private TextMeshProUGUI _lossCount;
    [Space]
    [SerializeField] private UnityEvent _gameWon;
    [SerializeField] private UnityEvent _gameLost;

    private Cell[,] _cells;
    private IEnumerator _ongoingGeneration;
    private enum GenerationLevel
    {
        SuperEasy,
        VeryEasy,
        Easy,
        SortOfEasy,
        Medium,
        MediumHard,
        Hard,
        SuperHard,
        Extreme
    }
    private int GenerationValue(GenerationLevel value)
    {
        switch (value)
        {
            case GenerationLevel.SuperEasy:
                return 9;
            case GenerationLevel.VeryEasy:
                return 8;
            default:
            case GenerationLevel.Easy:
                return 7;
            case GenerationLevel.SortOfEasy:
                return 6;
            case GenerationLevel.Medium:
                return 5;
            case GenerationLevel.MediumHard:
                return 4;
            case GenerationLevel.Hard:
                return 3;
            case GenerationLevel.SuperHard:
                return 2;
            case GenerationLevel.Extreme:
                return 1;
        }
    }

    private int _hiscore = 0;
    private int _score = 0;
    private int _wins = 0;
    private int _loses = 0;

    private bool _helpPanelOpen = false;


    private void Awake()
    {
        GetCellReferences();
        SetButtonClickFunctions();
        LoadAnySavedData();

        GenerateNewMinefield();
    }

    private void GetCellReferences()
    {
        List<Cell> cells = new List<Cell>();
        foreach (Transform cell in _minefieldObject.Find("Minefield Cells").transform)
        {
            cells.Add(new Cell(
                cell.Find("Cell Text").GetComponent<TextAnimator>(),
                cell.Find("Debug Text").GetComponent<TextMeshProUGUI>()
            ));
        }

        _cells = new Cell[,]
        {
            { cells[0], cells[1], cells[2], cells[3], cells[4], cells[5], cells[6], cells[7], cells[8] },
            { cells[9], cells[10], cells[11], cells[12], cells[13], cells[14], cells[15], cells[16], cells[17] },
            { cells[18], cells[19], cells[20], cells[21], cells[22], cells[23], cells[24], cells[25], cells[26] },
            { cells[27], cells[28], cells[29], cells[30], cells[31], cells[32], cells[33], cells[34], cells[35] },
            { cells[36], cells[37], cells[38], cells[39], cells[40], cells[41], cells[42], cells[43], cells[44] },
            { cells[45], cells[46], cells[47], cells[48], cells[49], cells[50], cells[51], cells[52], cells[53] },
            { cells[54], cells[55], cells[56], cells[57], cells[58], cells[59], cells[60], cells[61], cells[62] },
            { cells[63], cells[64], cells[65], cells[66], cells[67], cells[68], cells[69], cells[70], cells[71] },
            { cells[72], cells[73], cells[74], cells[75], cells[76], cells[77], cells[78], cells[79], cells[80] }
        };
    }

    private void SetButtonClickFunctions()
    {
        for (int x = 0; x < 9; x++)
        {
            for (int y = 0; y < 9; y++)
            {
                Vector2Int xy = new Vector2Int(x, y);
                _cells[xy.x, xy.y].Button.onClick.AddListener(() => CellClicked(new Vector2Int(xy.x, xy.y)));
            }
        }

        _difficultyDropdown.ClearOptions();
        string[] difficultyNamesArray = Enum.GetNames(typeof(GenerationLevel));
        List<string> difficultyNamesList = new List<string>(difficultyNamesArray);
        _difficultyDropdown.AddOptions(difficultyNamesList);
        _difficultyDropdown.value = (int)_difficulty;

        _starterCellsDropdown.ClearOptions();
        string[] _starterCellsNamesArray = Enum.GetNames(typeof(GenerationLevel));
        List<string> _starterCellsNamesList = new List<string>(_starterCellsNamesArray);
        _starterCellsDropdown.AddOptions(_starterCellsNamesList);
        _starterCellsDropdown.value = (int)_starterCells;
    }

    private void LoadAnySavedData()
    {
        _score = 0;
        _hiscore = PlayerPrefs.GetInt("MinefieldHiscore", 0);
        _wins = PlayerPrefs.GetInt("MinefieldWins", 0);
        _loses = PlayerPrefs.GetInt("MinefieldLoses", 0);
        UpdateUI();
    }

    public void ChangeDifficulty(int index) => _difficulty = (GenerationLevel)index;

    public void ChangeStarterCells(int index) => _starterCells = (GenerationLevel)index;

    public void GenerateNewMinefield()
    {
        _score = 0;
        UpdateUI();

        if (_ongoingGeneration != null)
            StopCoroutine(_ongoingGeneration);
        _ongoingGeneration = PopulateCellsWithValues();
        StartCoroutine(_ongoingGeneration);
    }

    private IEnumerator PopulateCellsWithValues()
    {
        _generationBlocker.SetActive(true);

        for (int x = 0; x < 9; x++)
            for (int y = 0; y < 9; y++)
                _cells[x, y].Cover();

        bool _hasAtLeastOneMine = false;
        Cell cell;
        for (int x = 0; x < 9; x++)
        {
            for (int y = 0; y < 9; y++)
            {
                cell = _cells[x, y];
                cell.IsMine = Random.Range(1, 11) > GenerationValue(_difficulty);
                if (cell.IsMine)
                    _hasAtLeastOneMine = true;

                if (!cell.IsMine)
                    if (Random.Range(1, 11) < GenerationValue(_starterCells))
                        cell.Uncover();

                if (_showDebugValues)
                    cell.SetDebugText(cell.IsMine ? "X" : "O");

                yield return new WaitForSecondsRealtime(_displayDelay);
            }
        }

        if (!_hasAtLeastOneMine)
        {
            cell = _cells[Random.Range(0, 9), Random.Range(0, 9)];
            cell.IsMine = true;
            cell.Cover();
        }

        for (int x = 0; x < 9; x++)
        {
            for (int y = 0; y < 9; y++)
            {
                cell = _cells[x, y];
                if (cell.Visible && !cell.IsMine)
                    cell.CheckSurroundingMines(new Vector2Int(x, y), _cells);
            }
        }

        _generationBlocker.SetActive(false);
        _ongoingGeneration = null;
    }

    public void CellClicked(Vector2Int cellIndex)
    {
        Cell cell = _cells[cellIndex.x, cellIndex.y];
        cell.Uncover();
        if (!cell.IsMine)
            _score += cell.CheckSurroundingMines(cellIndex, _cells);
        CheckForGameConclusion(cell);
    }

    private void CheckForGameConclusion(Cell cell)
    {
        if (_score > _hiscore)
            _hiscore = _score;

        if (cell.IsMine)
        {
            _loses++;
            _gameLost.Invoke();
        }
        else
        {
            bool _completed = true;
            for (int x = 0; x < 9; x++)
                for (int y = 0; y < 9; y++)
                    if (!_cells[x, y].IsMine)
                        if (!_cells[x, y].Visible)
                            _completed = false;

            if (_completed)
            {
                _wins++;
                _gameWon.Invoke();
            }
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        _scoreCount.SetText(_score.ToString());
        _hiscoreCount.SetText(_hiscore.ToString());
        _winCount.SetText(_wins.ToString());
        _lossCount.SetText(_loses.ToString());
    }

    public void ToggleHelpPanel()
    {
        _helpPanel.SetTrigger(_helpPanelOpen ? "Close" : "Open");
        _helpPanelOpen = !_helpPanelOpen;
    }

    public void QuitGame()
    {
        Application.Quit();

        #if UNITY_EDITOR
        EditorApplication.isPlaying = false;
        #endif
    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.SetInt("MinefieldWins", _wins);
        PlayerPrefs.SetInt("MinefieldLoses", _loses);
        if (_hiscore > PlayerPrefs.GetInt("MinefieldScore"))
            PlayerPrefs.SetInt("MinefieldHiscore", _hiscore);
    }

#if UNITY_EDITOR
    [ContextMenu("Clear PlayerPrefs Data")]
    private void ClearPrefsData() => PlayerPrefs.DeleteAll();
#endif
}

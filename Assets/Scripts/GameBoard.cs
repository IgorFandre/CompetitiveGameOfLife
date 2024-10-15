using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using Unity.Mathematics;

public class GameBoard : MonoBehaviour
{
    // Панель с описанием механики игры
    [SerializeField] private GameObject infoPanel;

    // Текстовые панели с обновляемыми данными по игре
    [SerializeField] private Text infoText;
    [SerializeField] private Text greenScoreText;
    [SerializeField] private Text redScoreText;
    [SerializeField] private Text generationText;

    // Карты, в которых будут отображаться клетки, одна текущая, вторая для обновления состояния
    [SerializeField] private Tilemap currentState;
    [SerializeField] private Tilemap nextState;

    // Тайлы для клеток первого и второго игрока
    [SerializeField] private Tile aliveGreenTile;
    [SerializeField] private Tile aliveRedTile;

    // Паттерны для выставления на доску
    [SerializeField] private Pattern EMPTY;
    [SerializeField] private Pattern POINT;
    [SerializeField] private Pattern PENTAMINO_R;
    [SerializeField] private Pattern SPACESHIP;
    [SerializeField] private Pattern GLIDER;
    [SerializeField] private Pattern LOAF;
    [SerializeField] private Pattern GENERATOR;

    // Интервал между обновлениями доски
    [SerializeField] private float updateInterval;

    // Ограничение на размер доски
    private static readonly int boarderLimit = 1000;

    // Для сохранения текущего паттерна для создания его при клике мышки на карту
    private Pattern curPattern = null;
    private Tile curTile;

    // Показатели, которые выводим в текстовых окошках
    private int scoreGreen = 0;
    private int scoreRed = 0;
    private int generation = 1;

    // Сет для хранения живых клеток
    private HashSet<Vector3Int> aliveCells;

    // Сет для хранения клеток, которые могут быть живыми в следующем кадре
    private HashSet<Vector3Int> cellsToCheck;

    private void Start() {
        aliveCells = new HashSet<Vector3Int>();
        cellsToCheck = new HashSet<Vector3Int>();

        // По умолчанию ставим цвет зеленого игрока.
        curTile = aliveGreenTile;

        Clear();
    }

    // Функция отдает экземпляр клетки по номеру игрока
    public Tile GetTile(int player_num) {
        if (player_num == 1) return aliveGreenTile;
        return aliveRedTile;
    }

    // Функция отдает счет игрока по его номеру
    public int GetScore(int player_num) {
        if (player_num == 1) return scoreGreen;
        return scoreRed;
    }

    public void Update() {
        // Сначала обновляем текстовые поля
        generationText.text = "Поколение: " + generation.ToString();
        greenScoreText.text = "Счет: " + scoreGreen.ToString();
        redScoreText.text = "Счет: " + scoreRed.ToString();

        // Теперь проверяем события нажатий на мышку
        if (Input.GetMouseButtonDown(0)) {
            var mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (!IsPause() || curPattern == null) {
                return;
            }
            SetPatternOnBoard(
                mouseWorldPos.x > 0 ? (int)mouseWorldPos.x : (int)mouseWorldPos.x - 1, 
                mouseWorldPos.y > 0 ? (int)mouseWorldPos.y : (int)mouseWorldPos.y - 1
            );
        }

        // Теперь проверяем события нажатий на клавиши
        // Окно help
        if (Input.GetKeyUp(KeyCode.H)) {
            Debug.Log("open|close Information Board");
            if (infoPanel.activeSelf) {
                infoPanel.SetActive(false);
            } else {
                infoPanel.SetActive(true);
            }
        }
        // Установить зеленую клетку
        if (Input.GetKeyUp(KeyCode.G)) {
            Debug.Log("set Green");
            curTile = aliveGreenTile;
        }
        // Установить красную клетку
        if (Input.GetKeyUp(KeyCode.R)) {
            Debug.Log("set Red");
            curTile = aliveRedTile;
        }
        // Нажатием на цифру устанавливаем шаблон
        if (Input.GetKeyUp(KeyCode.Alpha0)) {
            Debug.Log("set NOTHING");
            curPattern = EMPTY;
        }
        if (Input.GetKeyUp(KeyCode.Alpha1)) {
            Debug.Log("set POINT");
            curPattern = POINT;
        }
        if (Input.GetKeyUp(KeyCode.Alpha2)) {
            Debug.Log("set PENTAMINO_R");
            curPattern = PENTAMINO_R;
        }
        if (Input.GetKeyUp(KeyCode.Alpha3)) {
            Debug.Log("set SPACESHIP");
            curPattern = SPACESHIP;
        }
        if (Input.GetKeyUp(KeyCode.Alpha4)) {
            Debug.Log("set GLIDER");
            curPattern = GLIDER;
        }
        if (Input.GetKeyUp(KeyCode.Alpha5)) {
            Debug.Log("set LOAF");
            curPattern = LOAF;
        }
        if (Input.GetKeyUp(KeyCode.Alpha6)) {
            Debug.Log("set GENERATOR");
            curPattern = GENERATOR;
        }
    }

    // Функция, чтобы устанавливать паттерны извне в том числе извне
    public void SetCurPattern(Pattern pattern, int player_num) {
        curPattern = pattern;
        curTile = GetTile(player_num);
    }

    // Функция, чтобы устанавливать текущий паттерн на доску с нужными координатами
    private void SetPatternOnBoard(int x_offset, int y_offset) {
        Vector2Int center = curPattern.GetCenter();
        
        center.x -= x_offset;
        center.y -= y_offset;

        Debug.Log("set on map " + curPattern.cells.Length.ToString() + " points");

        for (int i = 0; i < curPattern.cells.Length; i++) {
            Vector3Int cell = (Vector3Int)(curPattern.cells[i] - center);
            currentState.SetTile(cell, curTile);
            aliveCells.Add(cell);
        }
    }

    // Функция проверки, что игра на паузе
    private bool IsPause() {
        return Mathf.Approximately(Time.timeScale, 0.0f);
    }

    // Функция перезапуска игры
    public void Clear() {
        scoreGreen = scoreRed = 0;
        generation = 1;

        SetCurPattern(null, 1);

        currentState.ClearAllTiles();
        nextState.ClearAllTiles();
        aliveCells.Clear();
        cellsToCheck.Clear();
    }

    private void OnEnable() {
        StartCoroutine(Simulate());
    }

    // Функция симуляции игры, запускается во втором потоке
    private IEnumerator Simulate() {
        var interval = new WaitForSeconds(updateInterval);
        var prevInterval = updateInterval;

        yield return interval;

        while (enabled) {
            if (prevInterval != updateInterval) {
                interval = new WaitForSeconds(updateInterval);
                prevInterval = updateInterval;
            }

            UpdateState();

            generation++;

            yield return interval;
        }
    }

    // Функция обновления состояния поля
    private void UpdateState() {
        cellsToCheck.Clear();

        foreach (Vector3Int cell in aliveCells) {
            for (int x = -1; x <= 1; x++) {
                if (Math.Abs(cell.x + x) >= boarderLimit) {
                    continue;
                }
                for (int y = -1; y <= 1; y++) {
                    if (Math.Abs(cell.y + y) >= boarderLimit) {
                        continue;
                    }
                    cellsToCheck.Add(cell + new Vector3Int(x, y, 0));
                }
            }
        }

        foreach (Vector3Int cell in cellsToCheck) {
            Vector2Int neighbors = CountNeighbors(cell); // х - соседи зеленого цвета, у - красного
            int all_neighbors = neighbors.x + neighbors.y;
            TileBase alive = IsAlive(cell);

            if (!alive && all_neighbors == 3) {
                // если создается новая клетка
                // ее цвет определяется по цветам соседей
                if (neighbors.x > neighbors.y) {
                    nextState.SetTile(cell, aliveGreenTile);
                    scoreGreen++;
                } else {
                    nextState.SetTile(cell, aliveRedTile);
                    scoreRed++;
                }
                aliveCells.Add(cell);
            } else if (alive && (all_neighbors < 2 || all_neighbors > 3)) {
                // если клетка умирает
                aliveCells.Remove(cell);
            } else {
                // если клетка остается такой же
                nextState.SetTile(cell, alive);
                aliveCells.Add(cell);
            }
        }

        // Меняем местами поля (обновление происходящего на экране)
        Tilemap tmp = currentState;
        currentState = nextState;
        nextState = tmp;
        nextState.ClearAllTiles();
    }

    private Vector2Int CountNeighbors(Vector3Int cell) { 
        // х - соседи зеленого цвета, у - красного
        Vector2Int answer = Vector2Int.zero;
        for (int x = -1; x <= 1; x++) {
            for (int y = -1; y <= 1; y++) {
                Vector3Int neighbor = cell + new Vector3Int(x, y, 0);
                TileBase tile = IsAlive(neighbor);
                if ((x | y) == 0 || !tile) {
                    continue;
                }
                if (tile == aliveGreenTile) {
                    answer.x++;
                } else {
                    answer.y++;
                }
            }
        }
        return answer;
    }

    // Функция, возвращающая экземпляр клетки по координатам, если клетка не жива, возвращает null
    private TileBase IsAlive(Vector3Int cell) {
        return currentState.GetTile(cell);
    }
}

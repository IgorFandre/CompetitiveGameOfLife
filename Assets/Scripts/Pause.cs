using UnityEngine;
using UnityEngine.UI; 

public class PauseButton : MonoBehaviour {
    [SerializeField] private GameBoard gameBoard;
    public GameObject canvasToShow;
    void Start() {
        if (canvasToShow != null) {
            canvasToShow.SetActive(true);
        } else {
            return;
        }
        // По умолчанию выставляем игру на паузу, так как поле изначально пустое
        TogglePause();
        GetComponent<Button>().onClick.AddListener(TogglePause);
    }

    public void TogglePause() {
        gameBoard.SetCurPattern(null, 1);
        Time.timeScale = Mathf.Approximately(Time.timeScale, 0.0f) ? 1.0f : 0.0f;
        if (canvasToShow != null) {
            canvasToShow.SetActive(true);
        }
    }
}
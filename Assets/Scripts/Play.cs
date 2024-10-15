using UnityEngine;
using UnityEngine.UI; 

public class PlayButton : MonoBehaviour {
    [SerializeField] private GameBoard gameBoard;
    public GameObject canvasToShow;
    void Start() {
        GetComponent<Button>().onClick.AddListener(TogglePause);
    }

    public void TogglePause() {
        gameBoard.SetCurPattern(null, 1);
        Time.timeScale = Mathf.Approximately(Time.timeScale, 0.0f) ? 1.0f : 0.0f;
        if (canvasToShow != null) {
            canvasToShow.SetActive(false);
        }
    }
}
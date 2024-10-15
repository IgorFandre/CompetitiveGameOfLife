using UnityEngine;
using UnityEngine.UI;

public class ReloadButton : MonoBehaviour {
    [SerializeField] private GameBoard gameBoard;

    void Start() {
        GetComponent<Button>().onClick.AddListener(Reload);
    }

    // Вызываем функцию поля для его очистки
    public void Reload() {
        gameBoard.Clear();
    }
}
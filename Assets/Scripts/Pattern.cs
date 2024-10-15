using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.Mathematics;

[CreateAssetMenu(menuName = "Game of Life/Pattern")]
public class Pattern : ScriptableObject
{
    // Шаблон фигуры представляет из себя двумерный массив координат точек
    public Vector2Int[] cells;

    // Функция, вычисляющая координаты центра фигуры
    public Vector2Int GetCenter() {
        if (cells == null | cells.Length == 0) {
            return Vector2Int.zero;
        }
        Vector2Int min = Vector2Int.zero;
        Vector2Int max = Vector2Int.zero;

        for (int i = 0; i < cells.Length; i++) {
            Vector2Int cell = cells[i];
            min.x = Math.Min(cell.x, min.x);
            min.y = Math.Min(cell.y, min.y);
            max.x = Math.Min(cell.x, max.x);
            max.y = Math.Min(cell.y, max.y);
        }

        return (min + max) / 2;
    }
}


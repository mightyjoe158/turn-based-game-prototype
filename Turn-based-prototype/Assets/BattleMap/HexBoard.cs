﻿using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;
using System.Linq;

public class HexBoard : MonoBehaviour, IEnumerable<Hex>{

    private Hex[,] cells;
    [SerializeField]
    private int columns = 5;
    [SerializeField]
    private int cellsPerColumn = 4;
    [SerializeField]
    private GameObject hexPrefab;
    [SerializeField]
    private float hexWidth;

    private float offsetX;
    private float offsetY;
    private float scale;

    private void Awake()
    {
        cells = new Hex[columns,cellsPerColumn];
        var gridw = GetComponent<RectTransform>().rect.width;
        var hexw = hexPrefab.GetComponent<RectTransform>().rect.width;
        var gridh = GetComponent<RectTransform>().rect.height;
        var hexh = hexPrefab.GetComponent<RectTransform>().rect.height;
        var widthScale = (gridw / (0.75f * (columns+1))) / hexw;
        var heightScale = (gridh / (cellsPerColumn)) / hexh;

        scale = Mathf.Min(widthScale, heightScale);
        offsetX = +hexw * scale / 2;
        offsetY = -hexh * scale / 2;

        //print("h" + heightScale + " w" + widthScale);

        Vector3 startingPosition = gameObject.transform.position;
        for(int i=0; i < columns; i++)
            for(int j = 0; j < cellsPerColumn; j++)
            {
                var cell = (GameObject)Instantiate(hexPrefab, calculateHexTransform(i, j, hexWidth*scale/2 ,startingPosition), Quaternion.Euler(0,0,0));
                cells[i, j] = cell.GetComponent<Hex>();
                cells[i, j].Position = OffsetToAxial(i, j);
                cell.transform.SetParent(this.gameObject.transform);
                cell.GetComponent<RectTransform>().localScale = new Vector3(scale,scale,scale);
            }
        
    }    

    //Indexers
    public Hex this[int q, int r]
    {
        get { return cells[q, r + q / 2]; }
        set { cells[q, r + q / 2] = value; }
    }
    public Hex this[Vector2 position]
    {
        get { return this[(int)position.x, (int)position.y]; }
        set { this[(int)position.x, (int)position.y] = value; }
    }

    //Calculating coordinates
    private static Vector2 AxialToOffset(int q, int r) { return new Vector2(q, r + (q / 2)); }
    private static Vector2 OffsetToAxial(int col, int row) { return new Vector2(col, row - (col / 2)); }
    private static Vector2 CubeToAxial(Vector3 position) { return CubeToAxial((int)position.x, (int)position.y, (int)position.z); }
    private static Vector2 CubeToAxial(int x, int y, int z)
    {
        return new Vector2(x, z);
    }

    //Calculating position of hex on screen
    private Vector2 calculateHexTransform(int col, int row, float hexSize, Vector3 startingPosition)
    {
        Vector2 offset = new Vector2(offsetX, offsetY);
        Vector2 result = new Vector3(col * 1.5f * hexSize, - row * Mathf.Sqrt(3f) * hexSize);
        result += offset*1.5f;
        if (col % 2 == 1)
            result.y -= (Mathf.Sqrt(3.0f) / 2) * hexSize;
        return result;
    }

    //Calculations of area on map
    public List<Vector2> ProperHexesInRange(Vector2 center, int range)
    {
        return HexesInRange(center, range).Where(vector => isOnBoard(vector)).ToList();
    }
    public static List<Vector2> HexesInRange(Vector2 center,  int range)
    {
        List<Vector2> result = new List<Vector2>();
        for (int x = -range; x <= range; x++)
        {
            for (int y = Math.Max(-range, -x - range); y <= Math.Min(range, -x + range); y++)
            {
                //FIX possible optimalization ?
                int z = -x - y;
                if (z <= range)
                    result.Add(center + CubeToAxial(new Vector3(x, y, z)));
            }
        }
        return result;
    }
    public bool isOnBoard(Vector2 vector)
    {
        if (vector.x >= 0 && vector.x < columns)
            if (vector.y >= OffsetToAxial((int)vector.x, 0).y && vector.y <= OffsetToAxial((int)vector.x, cellsPerColumn - 1).y)
                return true;
        return false;
    }
    
    //IEnumerable Interface
    public IEnumerator<Hex> GetEnumerator()
    {
        foreach (Hex h in cells)
            yield return h;
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
        return cells.GetEnumerator();
    }
}

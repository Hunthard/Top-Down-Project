using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HallPlacer : MonoBehaviour
{
    // Размеры лабиринта
    public const int MAZE_HEIGHT = 100;
    public const int MAZE_WIDTH = 100;
    // Максимальное количество коридоров
    public const int hallCount = 250;
    
    // Массив коридоров
    [Header("Префабы коридоров")]
    public Hall[] HallPrefabs;
    // Стартовый коридор
    public Hall StartingHall;

    // Координаты стартового коридора в массиве
    [Header("Начальное положение")]
    //public Vector2Int StartPosition;

    [Range(0, MAZE_WIDTH - 1)]
    public int _x;
    [Range(0, MAZE_HEIGHT - 1)]
    public int _y;

    // Массив заспавненых коридоров
    private Hall[,] spawnedHalls;

    // Список вероятнестей спавна всех коридоров
    private List<float> hallSpawnChances = new List<float>();

    // Start is called before the first frame update
    void Start() 
    {
        spawnedHalls = new Hall[MAZE_HEIGHT, MAZE_WIDTH];
        spawnedHalls[_x, _y] = StartingHall;

        for (int i = 0; i < HallPrefabs.Length; i++)
        {
            hallSpawnChances.Add(HallPrefabs[i].SpawnChance);
        }

        for (int i = 0; i < hallCount; i++)
        {
            PlaceOneHall();
            Debug.Log("PlaceOneHall was exec");
            //yield return new WaitForSecondsRealtime(0.5f);
        }
    }

    private void PlaceOneHall()
    {
        HashSet<Vector2Int> vacantPlaces = new HashSet<Vector2Int>();

        for (int x = 0; x < MAZE_WIDTH; x++)
        {
            for (int y = 0; y < MAZE_HEIGHT; y++)
            {
                // Если коридор не заспавнен, то перейти к следующей итерации
                if (spawnedHalls[x, y] == null) continue;

                // Если не на левой границе и слева есть свободное место, то добавляем в список вакантное место
                if (x > 0 && !spawnedHalls[x - 1, y]) vacantPlaces.Add(new Vector2Int(x - 1, y));
                // Если не на нижней границе и снизу есть свободное место, то добавляем в список вакантное место
                if (y > 0 && !spawnedHalls[x, y - 1]) vacantPlaces.Add(new Vector2Int(x, y - 1));
                // Если не на правой границе и справа есть свободное место, то добавляем в список вакантное место
                if (x < (MAZE_WIDTH - 1) && !spawnedHalls[x + 1, y]) vacantPlaces.Add(new Vector2Int(x + 1, y));
                // Если не на верхней границе и сверху есть свободное место, то добавляем в список вакантное место
                if (y < (MAZE_HEIGHT - 1) && !spawnedHalls[x, y + 1]) vacantPlaces.Add(new Vector2Int(x, y + 1));
            }
        }

        Hall newHall = Instantiate(GetRandomHall());

        int limit = 500;
        while (limit-- > 0)
        {
            Vector2Int position = vacantPlaces.ElementAt<Vector2Int>(Random.Range(0, vacantPlaces.Count));

            if (ConnectToHall(newHall, position))
            {
                newHall.transform.position = new Vector3(position.x - _x, position.y - _y, 0);
                spawnedHalls[position.x, position.y] = newHall;
                Debug.Log("Halls was placed");
                return;
            }
        }

        Destroy(newHall.gameObject);
        Debug.Log("Halls was destroyed");
    }

    private bool ConnectToHall(Hall hall, Vector2Int p)
    {
        // Список соседей коридора при наличии смежных проходов
        List<Vector2Int> neighbours = new List<Vector2Int>();

        // Если не на верхней границе, у коридора есть проход наверх и у соседа сверху есть проход вниз (если этот коридор заспавнен), то добавить его с список соседей
        if (hall.DoorTop != null && (p.y < (MAZE_HEIGHT - 1)) && spawnedHalls[p.x, p.y + 1]?.DoorBottom) neighbours.Add(Vector2Int.up);
        // Если не на нижней границе, у коридора есть проход вниз и у соседа снизу есть проход наверх (если этот коридор заспавнен), то добавить его с список соседей
        if (hall.DoorBottom != null && (p.y > 0) && spawnedHalls[p.x, p.y - 1]?.DoorTop) neighbours.Add(Vector2Int.down);
        // Если не на правой границе, у коридора есть проход направо и у соседа справа есть проход налево (если этот коридор заспавнен), то добавить его с список соседей
        if (hall.DoorRight != null && (p.x < (MAZE_WIDTH - 1)) && spawnedHalls[p.x + 1, p.y]?.DoorLeft) neighbours.Add(Vector2Int.right);
        // Если не на левой границе, у коридора есть проход налево и у соседа слева есть проход направо (если этот коридор заспавнен), то добавить его с список соседей
        if (hall.DoorLeft != null && (p.x > 0) && spawnedHalls[p.x - 1, p.y]?.DoorRight) neighbours.Add(Vector2Int.left);


        if (neighbours.Count == 0) return false;

        return true;
    }

    private Hall GetRandomHall()
    {
        float value = Random.Range(0, hallSpawnChances.Sum());
        float sum = 0;

        for (int i = 0; i < hallSpawnChances.Count; i++)
        {
            sum += hallSpawnChances[i];
            if (value < sum)
            {
                return HallPrefabs[i];
            }
        }

        return HallPrefabs[HallPrefabs.Length - 1];
    }
}

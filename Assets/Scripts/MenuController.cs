using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [Header("Menus")]
    [SerializeField] List<Menu> menus = new List<Menu>();

    [Header("Loading components")]
    public Slider progressSlider;
    public Text progressText;


    // Start is called before the first frame update
    void Start()
    {
        // Показать Main Menu при запуске
        ShowMenu(menus[0]);
    }

    public void ShowMenu(Menu menuToShow)
    {
        // Убедимся, что мы отслеживаем это меню
        if (!menus.Contains(menuToShow))
        {
            Debug.LogErrorFormat(
                "{0} is not in the list of menus",
                menuToShow.name
            );
            return;
        }

        // Активируем это меню и отключаем остальные
        foreach (var otherMenu in menus)
        {
            // Это то меню, которое мы хотим отобразить?
            if (otherMenu == menuToShow)
            {
                // Отметить как активное
                otherMenu.gameObject.SetActive(true);

                // Передать в Menu object вызов эвента "did appear"
                otherMenu.menuDidAppear.Invoke();
            }
            else
            {
                // Это меню уже активно?
                if (otherMenu.gameObject.activeInHierarchy)
                {
                    // Если да, передать в Menu object вызов эвента "disappear"
                    otherMenu.menuWillDisappear.Invoke();
                }

                // Отметить как неактивное
                otherMenu.gameObject.SetActive(false);
            }
        }
    }

    // Выход из игры
    public void ExitGame()
    {
        Application.Quit();
    }

    public void RunGame(int sceneIndex)
    {
        // Начинаем загрузку сцены.
        // Получаем объект операции загрузки сцены
        var operation = SceneManager.LoadSceneAsync(sceneIndex);

        Debug.Log("Starting load...");

        // Не переходить к сцене пока загрузка не завершится
        operation.allowSceneActivation = false;

        // Запустить корутину, которая запустится во время загрузки сцены
        // и запустит некоторый код после загрузки сцены
        StartCoroutine(WaitForLoading(operation));
    }

    IEnumerator WaitForLoading(AsyncOperation operation)
    {
        // Пока загружка сцены не достигнет 90%
        while (operation.progress < 0.9f)
        {
            // Обновить статус загрузки
            Debug.Log("Loading progress..." + operation.progress);

            progressSlider.value = operation.progress;
            progressText.text = "Loading..." + operation.progress;

            yield return null;
        }
        //yield return new WaitUntil(() => operation.isDone);

        // Загрузка завершена
        Debug.Log("Loading complete!");

        // Обновляем статус загрузки на "Done"
        progressText.text = "Loading done";

        yield return new WaitForSeconds(2);

        operation.allowSceneActivation = true;
    }
}

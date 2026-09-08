using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VRDoor : MonoBehaviour
{
    public RoomData roomData;           // przypisujesz w Inspectorze
 
    public float waitBeforeLoad = 1f;   // opcjonalny delay dopóki drzwi siê otwieraj¹

    private bool isLoading = false;

    public void OnHandleGrab()
    {
        if (isLoading || roomData == null) return;

        isLoading = true;

   
        // start ³adowania pokoju
        StartCoroutine(LoadRoom(roomData.sceneName));
    }

    private IEnumerator LoadRoom(string sceneName)
    {
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        loadOp.allowSceneActivation = false;

        // czekamy a¿ scena siê wczyta do 90%
        while (loadOp.progress < 0.9f)
            yield return null;

        // czekamy chwilê, a¿ drzwi siê otworz¹
        yield return new WaitForSeconds(waitBeforeLoad);

        // aktywujemy scenê
        loadOp.allowSceneActivation = true;

        // odinstalowujemy inne pokoje (z wyj¹tkiem Hub)
        UnloadOtherRooms(sceneName);
    }

    private void UnloadOtherRooms(string keepScene)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene s = SceneManager.GetSceneAt(i);
            if (s.name != "HubRoom" && s.name != keepScene)
                SceneManager.UnloadSceneAsync(s);
        }
    }
}

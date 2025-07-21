
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MadApper
{
    public class ScenesLoaderHelper : MonoBehaviour
    {
        public void z_ReloadScene()
        {
            ScenesLoader.Reload();
        }
        public void z_ReloadSceneOld()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}

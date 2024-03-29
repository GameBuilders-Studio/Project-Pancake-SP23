using UnityEngine;

namespace GameBuilders.Singleton
{
    public class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        public static T Instance
        {
            get
            {
                if (s_instance == null)
                {
                    T[] managers = Object.FindObjectsOfType(typeof(T)) as T[];
                    if (managers.Length != 0)
                    {
                        if (managers.Length == 1)
                        {
                            s_instance = managers[0];
                            s_instance.gameObject.name = typeof(T).Name;
                            return s_instance;
                        }
                        else
                        {
                            Debug.LogError("Class " + typeof(T).Name + " exists multiple times in violation of singleton pattern. Destroying all copies");
                            foreach (T manager in managers)
                            {
                                Destroy(manager.gameObject);
                            }
                        }
                    }
                    var go = new GameObject(typeof(T).Name, typeof(T));
                    s_instance = go.GetComponent<T>();
                    DontDestroyOnLoad(go);
                }
                return s_instance;
            }
            set
            {
                s_instance = value;
            }
        }

        private static T s_instance;
    }
}

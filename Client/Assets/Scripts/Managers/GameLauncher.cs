using UnityEngine;

public class GameLauncher : MonoBehaviour
{
    async void Start()
    {
        // 按顺序创建 Manager，每个都是独立物体
        // 这样在 Hierarchy 面板中，你可以清晰地看到每个系统的状态
        new GameObject("UIManager", typeof(UIManager));
        new GameObject("ResourceManager", typeof(ResourceManager));
        //new GameObject("AudioManager", typeof(AudioManager));
        new GameObject("SimpleClient", typeof(SimpleClient), typeof(TTTGameClient));
        //new GameObject("TTTGameClient", typeof(TTTGameClient));

        // 全部启动后，加载真正的游戏主场景或弹出登录界面
        await UIManager.Instance.PushPanel<UI_Login>();

        // 销毁启动器自己，或者禁用它
        Destroy(this.gameObject);
    }
}

using UnityEngine;

public abstract class BasePanel : MonoBehaviour
{
    protected virtual void Awake()
    {
        // 挂载脚本后，立即自动扫描并绑定按钮、文本等
        UIBinder.Bind(this);
    }

    public virtual void OnEnter() => gameObject.SetActive(true);
    public virtual void OnPause() { }
    public virtual void OnResume() { }
    public virtual void OnExit() => gameObject.SetActive(false);
}
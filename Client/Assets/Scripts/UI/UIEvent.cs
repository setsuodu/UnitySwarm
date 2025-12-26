using System;

// 全局事件，框架的解耦中心
public static class UIEvent
{
    // 登录相关事件
    public static Action<string> OnLoginSuccess; // 传递用户名
    public static Action<string> OnLoginFailed;  // 传递错误信息

    // 游戏逻辑相关
    public static Action<string> OnChess;  // 落子

    // 全局通用事件（如金币更新、飘字提示）
    public static Action<string> OnShowToast;
}
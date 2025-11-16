namespace ET.Server
{
    [Invoke((long)(SceneType.LoginCenter))]
    public class FiberInit_LoginCenter:AInvokeHandler<FiberInit,ETTask>
    {
        public override async ETTask Handle(FiberInit fiberInit)
        {
            //登陆中心服是跨区服的，重复登录时会通知断开session连接
            Scene root = fiberInit.Fiber.Root;
            root.AddComponent<MailBoxComponent, MailBoxType>(MailBoxType.UnOrderedMessage);
            root.AddComponent<TimerComponent>();
            root.AddComponent<CoroutineLockComponent>();
            root.AddComponent<ProcessInnerSender>();
            root.AddComponent<MessageSender>();

            root.AddComponent<LoginInfoRecordComponent>();
            await ETTask.CompletedTask;
        }
    }
}


namespace ET.Server
{
    public static class DisconnectHelper
    {
        public static async ETTask Disconnect(this Session self)
        {
            if (self == null || self.IsDisposed)
            {
                return;
            }
            long instanceId = self.InstanceId;
            TimerComponent timerComponent = self.Root().GetComponent<TimerComponent>();
            await timerComponent.WaitAsync(1000);
            
            //要判断InstanceId为同一个才会Dispose
            if (self.InstanceId != instanceId)
            {
                return;
            }
            self.Dispose();
        }
    }
}


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

        public static async ETTask KickPlayerNoLock(Player player)
        {
            if (player == null || player.IsDisposed)
            {
                return;
            }

            switch (player.PlayerState)
            {
                case PlayerState.DisConnect:
                    break;
                case PlayerState.Gate:
                    break;
                case PlayerState.Game:
                    //通知逻辑服下线unit角色逻辑，并将数据存入数据库
                    var m2G_RequestExitGame = await player.Root().GetComponent<MessageLocationSenderComponent>()
                            .Get(LocationType.Unit).Call(player.UnitId, G2M_RequestExitGame.Create()) as M2G_RequestExitGame;
                    //通知移除账号角色登录信息
                    G2L_RemoveLoginRecord g2LRemoveLoginRecord = G2L_RemoveLoginRecord.Create();
                    g2LRemoveLoginRecord.AccountName = player.Account;
                    g2LRemoveLoginRecord.ServerId = player.Zone();
                    var l2G_RemoveLoginRecord = await player.Root().GetComponent<MessageSender>()
                            .Call(StartSceneConfigCategory.Instance.LoginCenterConfig.ActorId, g2LRemoveLoginRecord) as L2G_RemoveLoginRecord;
                    break;
            }
            TimerComponent timerComponent = player.Root().GetComponent<TimerComponent>();
            player.PlayerState = PlayerState.DisConnect;
            await player.GetComponent<PlayerSessionComponent>().RemoveLocation(LocationType.GateSession);
            await player.RemoveLocation(LocationType.Player);
            player.Root().GetComponent<PlayerComponent>()?.Remove(player);
            player.Dispose();
            await timerComponent.WaitAsync(300);
        }
        
        
        //踢玩家下线
        public static async ETTask KickPlayer(Player player)
        {
            if (player == null || player.IsDisposed)
            {
                return;
            }
            long instanceId = player.InstanceId;
            CoroutineLockComponent coroutineLockComponent = player.Root().GetComponent<CoroutineLockComponent>();
            using (await coroutineLockComponent.Wait(CoroutineLockType.LoginGate,player.Account.GetLongHashCode()))
            {
                if (player.IsDisposed || instanceId != player.InstanceId)
                {
                    return;
                }

                await KickPlayerNoLock(player);
            }
        }
    }
}


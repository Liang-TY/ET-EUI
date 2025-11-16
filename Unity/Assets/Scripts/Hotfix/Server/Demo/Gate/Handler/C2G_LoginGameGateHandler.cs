namespace ET.Server
{
    [MessageSessionHandler(SceneType.LoginCenter)]
    public class C2G_LoginGameGateHandler:MessageSessionHandler<C2G_LoginGameGate, G2C_LoginGameGate> 
    {
        protected override async ETTask Run(Session session, C2G_LoginGameGate request, G2C_LoginGameGate response)
        {
            Scene root = session.Root();
            if (session.GetComponent<SessionLockingComponent>() != null)
            {
                response.Error = ErrorCode.ERR_RequestRepeatedly;
                return;
            }
            string account = root.GetComponent<GateSessionKeyComponent>().Get(request.Key);
            if (account == null)
            {
                response.Error = ErrorCore.ERR_ConnectGateKeyError;
                response.Message = "Gate key验证失败!";
                session?.Disconnect().Coroutine();
                return;
            }
            root.GetComponent<GateSessionKeyComponent>().Remove(request.Key);
            session.RemoveComponent<SessionAcceptTimeoutComponent>();
            
            CoroutineLockComponent coroutineLockComponent = root.GetComponent<CoroutineLockComponent>();
            long instanceId = session.InstanceId;
            using (session.AddComponent<SessionLockingComponent>())
            {
                using (await coroutineLockComponent.Wait(CoroutineLockType.LoginGate,request.AccountName.GetLongHashCode()))
                {
                    if (instanceId != session.InstanceId)
                    {
                        response.Error = ErrorCode.ERR_LoginGateError;
                        return;
                    }
                    //通知登录中心服，记录本次登录的服务器zone
                    G2L_AddLoginRecord g2LAddLoginRecord = G2L_AddLoginRecord.Create();
                    g2LAddLoginRecord.AccountName = request.AccountName;
                    g2LAddLoginRecord.ServerId = root.Zone();
                    L2G_AddLoginRecord l2GAddLoginRecord = 
                            await root.GetComponent<MessageSender>().Call(
                                StartSceneConfigCategory.Instance.LoginCenterConfig.ActorId,g2LAddLoginRecord
                                ) as L2G_AddLoginRecord;
                    if (l2GAddLoginRecord.Error != ErrorCode.ERR_Success)
                    {
                        response.Error =l2GAddLoginRecord.Error;
                        session?.Disconnect().Coroutine();
                        return;
                    }
                    PlayerComponent playerComponent = root.GetComponent<PlayerComponent>();
                    Player player = playerComponent.GetByAccount(account);
                    if (player != null)
                    {
                        player = playerComponent.AddChildWithId<Player, string>(request.RoleId, account);
                        player.UnitId = request.RoleId;
                        playerComponent.Add(player);
                        PlayerSessionComponent playerSessionComponent = player.GetComponent<PlayerSessionComponent>();
                        playerSessionComponent.AddComponent<MailBoxComponent, MailBoxType>(MailBoxType.GateSession);
                        await playerSessionComponent.AddLocation(LocationType.Player);
                        session.AddComponent<SessionPlayerComponent>().Player = player;
                        player.PlayerState = PlayerState.Gate;
                    }
                    else
                    {
                        //后面会讲什么时候加上这个组件
                        player.RemoveComponent<PlayerOfflineOutTimeComponent>();
                        session.AddComponent<SessionPlayerComponent>().Player = player;
                        player.GetComponent<PlayerSessionComponent>().Session = session;
                    }
                    response.PlayerId = player.Id;
                }
            }
        }
    }
}

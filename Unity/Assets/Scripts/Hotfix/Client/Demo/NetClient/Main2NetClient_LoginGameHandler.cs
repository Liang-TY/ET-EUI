namespace ET.Client
{
        //使用NetClient这个scene来处理网络消息
    [MessageHandler(SceneType.NetClient)]
    public class Main2NetClient_LoginGameHandler: MessageHandler<Scene, Main2NetClient_LoginGame, NetClient2Main_LoginGame>
    {
        protected override async ETTask Run(Scene root, Main2NetClient_LoginGame request, NetClient2Main_LoginGame response)
        {
            string account = request.Account;
            NetComponent netComponent = root.GetComponent<NetComponent>();
            // 创建一个gate Session,并且保存到SessionComponent中
            //为什么这里用两个account参数？
            Session gateSession = await netComponent.CreateRouterSession(NetworkHelper.ToIPEndPoint(request.GateAddress), account, account);
            gateSession.AddComponent<ClientSessionErrorComponent>();
            root.AddComponent<SessionComponent>().Session = gateSession;
            C2G_LoginGameGate c2GLoginGameGate = C2G_LoginGameGate.Create();
            c2GLoginGameGate.Key = request.RealmKey;
            c2GLoginGameGate.AccountName = request.Account;
            c2GLoginGameGate.RoleId = request.RoleId;
            
            //使用令牌请求登录进网关，在网关上创建player实体id
            G2C_LoginGameGate g2CLoginGameGate = (G2C_LoginGameGate)await gateSession.Call(c2GLoginGameGate);
            if (g2CLoginGameGate.Error != ErrorCode.ERR_Success)
            {

                response.Error = g2CLoginGameGate.Error;
                Log.Error($"登录gate失败，{g2CLoginGameGate.Error}");
                return;
            }
            Log.Debug("登录gate成功");
            G2C_EnterGame g2CEnterGame = (G2C_EnterGame)await gateSession.Call(C2G_EnterGame.Create());
            if (g2CEnterGame.Error != g2CEnterGame.Error)
            {
                response.Error = g2CEnterGame.Error; 
                Log.Error($"登录Map失败,,,{g2CEnterGame.Error}");
                return;
            }
            Log.Debug("登录Map成功");
            response.PlayerId = g2CEnterGame.MyUnitId;
        }
    }
}


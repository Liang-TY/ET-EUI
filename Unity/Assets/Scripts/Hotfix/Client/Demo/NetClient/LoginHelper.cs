namespace ET.Client
{
    public static class LoginHelper
    {
        public static async ETTask Login(Scene root, string account, string password)
        {
            root.RemoveComponent<ClientSenderComponent>();
            //移除后又添加是为了确保每一次点击登录都是全新的连接
            ClientSenderComponent clientSenderComponent = root.AddComponent<ClientSenderComponent>();
            //客户端获得账号在网关(gate1/gate2等)上的实体id
            NetClient2Main_Login response = await clientSenderComponent.LoginAsync(account, password);

            if (response.Error != ErrorCode.ERR_Success)
            {
                Log.Error($"请求登录失败，错误 : {response.Error}");
                return;
            }
            string Token = response.Token;
            // root.GetComponent<PlayerComponent>().MyId = response.PlayerId;
            
            //获取区服信息
            C2R_GetServerInfos c2RGetServerInfos = C2R_GetServerInfos.Create();
            c2RGetServerInfos.Account = account;
            c2RGetServerInfos.Token = response.Token;
            //没有创建出session之前用clientsendercomponent.loginasync发网络消息
            //客户端创建出session后就可以直接用clientsendercomponent.call
            R2C_GetServerInfos r2CGetServerInfos = await clientSenderComponent.Call(c2RGetServerInfos) as R2C_GetServerInfos;
            if (r2CGetServerInfos.Error != ErrorCode.ERR_Success)
            {
                Log.Error("请求服务器列表失败");
                return;
            }

            ServerInfoProto serverInfoProto = r2CGetServerInfos.ServerInfosList[0];
            
            
            //获取区服角色列表
            //暂时没到这里
            
            
            await EventSystem.Instance.PublishAsync(root, new LoginFinish());
        }
    }
}
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
            C2R_GetRoles c2RGetRoles = C2R_GetRoles.Create();
            c2RGetRoles.Token = response.Token;
            c2RGetRoles.Account = account;
            c2RGetRoles.ServerId = serverInfoProto.Id;
            R2C_GetRoles r2CGetRoles = await clientSenderComponent.Call(c2RGetRoles) as R2C_GetRoles;
            if (r2CGetRoles.Error != ErrorCode.ERR_Success)
            {
                Log.Error("请求区服角色列表失败");
                return;
            }

            RoleInfoProto roleInfoProto = default;
            if (r2CGetRoles.RoleInfo.Count <= 0)
            {
                //创建角色
                //暂时还没讲到
            }
            
            await EventSystem.Instance.PublishAsync(root, new LoginFinish());
        }
    }
}
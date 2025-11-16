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
                C2R_CreateRole c2RCreateRole = C2R_CreateRole.Create();
                c2RCreateRole.Token = Token;
                c2RCreateRole.Account = account;
                c2RCreateRole.ServerId = serverInfoProto.Id;
                c2RCreateRole.Name = account;
                R2C_CreateRole r2CCreateRole = await clientSenderComponent.Call(c2RCreateRole) as R2C_CreateRole;
                if (r2CCreateRole .Error  != ErrorCode.ERR_Success)
                {
                    Log.Error("创建区服角色失败");
                    return;
                }

                roleInfoProto = r2CCreateRole.RoleInfo;
            }
            else
            {
                roleInfoProto = r2CGetRoles.RoleInfo[0]; 
            }
            
            //请求获取RealmKey
            C2R_GetRealmKey c2RGetRealmKey = C2R_GetRealmKey.Create();
            c2RGetRealmKey.Token = Token;
            c2RGetRealmKey.Account = account;
            c2RGetRealmKey.ServerId = serverInfoProto.Id;
            R2C_GetRealmKey r2CGetRealmKey = await clientSenderComponent.Call(c2RGetRealmKey) as R2C_GetRealmKey;

            if (r2CGetRealmKey.Error != ErrorCode.ERR_Success)
            {
                Log.Error("获取RealmKey失败");
                return;
            }
            
            //请求角色进入map地图
            NetClient2Main_LoginGame netClient2MainLoginGame =
                    await clientSenderComponent.LoginGameAsync(account, r2CGetRealmKey.Key, roleInfoProto.Id, r2CGetRealmKey.Address);
            if (netClient2MainLoginGame.Error != ErrorCode.ERR_Success)
            {
                Log.Error($"进入游戏失败，{netClient2MainLoginGame.Error}");
                return;
            }
            await EventSystem.Instance.PublishAsync(root, new LoginFinish());
        }
    }
}
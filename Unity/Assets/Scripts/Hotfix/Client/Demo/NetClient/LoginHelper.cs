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
                Log.Error($"response error : {response.Error}");
                return;
            }
            
            // root.GetComponent<PlayerComponent>().MyId = response.PlayerId;
            
            
            await EventSystem.Instance.PublishAsync(root, new LoginFinish());
        }
    }
}
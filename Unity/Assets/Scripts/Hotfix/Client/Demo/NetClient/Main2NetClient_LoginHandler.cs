using System;
using System.Net;
using System.Net.Sockets;

namespace ET.Client
{
    //使用NetClient这个scene来处理网络消息
    [MessageHandler(SceneType.NetClient)]
    public class Main2NetClient_LoginHandler: MessageHandler<Scene, Main2NetClient_Login, NetClient2Main_Login>
    {
        protected override async ETTask Run(Scene root, Main2NetClient_Login request, NetClient2Main_Login response)
        {
            string account = request.Account;
            string password = request.Password;
            //RouterAddressComponent用于请求路由节点服务器，获取路由节点地址列表，也就是说节点记录着路由节点列表
            // 创建一个ETModel层的Session
            //移除该组件是为了获取全新的路由节点
            root.RemoveComponent<RouterAddressComponent>();
            //如果部署到公网，要改RouterHttpHost中路由服务器的ip和端口
            // 获取路由跟realmDispatcher地址
            RouterAddressComponent routerAddressComponent =
                    root.AddComponent<RouterAddressComponent, string, int>(ConstValue.RouterHttpHost, ConstValue.RouterHttpPort);
            //获取路由节点地址列表，存到RouterAddressComponent
            await routerAddressComponent.Init();
            //NetComponent才是真正和游戏服务器进行网络收发的组件
            root.AddComponent<NetComponent, AddressFamily, NetworkProtocol>(routerAddressComponent.RouterManagerIPAddress.AddressFamily, NetworkProtocol.UDP);
            root.GetComponent<FiberParentComponent>().ParentFiberId = request.OwnerFiberId;

            NetComponent netComponent = root.GetComponent<NetComponent>();
            //获取用于负载均衡的Realm地址
            //部署到公网时，会在起服配置中配置很多个Realm地址
            IPEndPoint realmAddress = routerAddressComponent.GetRealmAddress(account);

            R2C_Login r2CLogin;
            // 这里的session通话链路是: session -> Router -> Realm
            using (Session session = await netComponent.CreateRouterSession(realmAddress, account, password))
            {
                C2R_Login c2RLogin = C2R_Login.Create();
                c2RLogin.Account = account;
                c2RLogin.Password = password;
                r2CLogin = (R2C_Login)await session.Call(c2RLogin);
            }

            // 这里的gateSession通话链路是: gateSession -> Gate -> others -> gate -> Client -> gateSession
            // 创建一个gate Session,并且保存到SessionComponent中
            Session gateSession = await netComponent.CreateRouterSession(NetworkHelper.ToIPEndPoint(r2CLogin.Address), account, password);
            gateSession.AddComponent<ClientSessionErrorComponent>();
            root.AddComponent<SessionComponent>().Session = gateSession;
            C2G_LoginGate c2GLoginGate = C2G_LoginGate.Create();
            c2GLoginGate.Key = r2CLogin.Key;
            c2GLoginGate.GateId = r2CLogin.GateId;
            //使用令牌请求登录进网关，在网关上创建player实体id
            G2C_LoginGate g2CLoginGate = (G2C_LoginGate)await gateSession.Call(c2GLoginGate);

            Log.Debug("登陆gate成功!");

            response.PlayerId = g2CLoginGate.PlayerId;
        }
    }
}
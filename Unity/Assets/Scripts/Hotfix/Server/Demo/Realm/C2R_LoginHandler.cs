using System;
using System.Net;


namespace ET.Server
{
	//在Realm服务器上进行处理
	[MessageSessionHandler(SceneType.Realm)]
	public class C2R_LoginHandler : MessageSessionHandler<C2R_Login, R2C_Login>
	{//只要消息实现了ISessionRequest，那么就用MessageSessionHandler处理
		//这里的session是客户端在服务器上创建的session，不是客户端的session
		protected override async ETTask Run(Session session, C2R_Login request, R2C_Login response)
		{
			// 获取起服配置中的gate列表，随机分配一个Gate
			StartSceneConfig config = RealmGateAddressHelper.GetGate(session.Zone(), request.Account);
			Log.Debug($"gate address: {config}");
			
			// 向gate请求一个key,客户端可以拿着这个key连接gate
			R2G_GetLoginKey r2GGetLoginKey = R2G_GetLoginKey.Create();
			r2GGetLoginKey.Account = request.Account;
			G2R_GetLoginKey g2RGetLoginKey = (G2R_GetLoginKey) await session.Fiber().Root.GetComponent<MessageSender>().Call(
				config.ActorId, r2GGetLoginKey);

			response.Address = config.InnerIPPort.ToString();
			response.Key = g2RGetLoginKey.Key;
			response.GateId = g2RGetLoginKey.GateId;
			//先回复消息，1秒后断开这个session的连接，此后客户端的对应的session也用不了
			CloseSession(session).Coroutine();
		}

		private async ETTask CloseSession(Session session)
		{
			await session.Root().GetComponent<TimerComponent>().WaitAsync(1000);
			session.Dispose();
		}
	}
}

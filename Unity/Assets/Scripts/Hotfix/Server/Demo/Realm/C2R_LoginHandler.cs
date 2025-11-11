using System;
using System.Collections.Generic;
using System.Net;

namespace ET.Server
{
	[FriendOf(typeof(AccountInfo))]
	//在Realm服务器上进行处理
	[MessageSessionHandler(SceneType.Realm)]
	public class C2R_LoginHandler : MessageSessionHandler<C2R_Login, R2C_Login>
	{//只要消息实现了ISessionRequest，那么就用MessageSessionHandler处理
		//这里的session是客户端在服务器上创建的session，不是客户端的session
		//一般是客户端直连某个类型的服务器的时候会用MessageSessionHandler，会产生一个session
		protected override async ETTask Run(Session session, C2R_Login request, R2C_Login response)
		{
			
			//账户验证

			if (string.IsNullOrEmpty((request.Account)) || string.IsNullOrEmpty(request.Password))
			{
				response.Error = ErrorCode.ERR_LoginInfoEmpty;
				CloseSession(session).Coroutine();
				//以前是return ettask.completed;但是现在可以直接return
				//也就是直接return会先回复消息给客户端，然后过1秒断开连接
				return;
			}

			//进行账号操作时候加锁，防止多个玩家同时操作同一个账号
			using (await session.Root().GetComponent<CoroutineLockComponent>().Wait(CoroutineLockType.LoginAccount,request.Account.GetHashCode()))
			{
				//session是在relm下创建的，所以session.Zone()获取的也是relm所在的zone地址
				//这个地址是在startsceneconfig中配置的
				//拿到zone后，从startzoneconfig中拿到zone id对应的db的数据库地址、数据库名
				DBComponent dbComponent = session.Root().GetComponent<DBManagerComponent>().GetZoneDB(session.Zone());
				List<AccountInfo> accountInfos = await dbComponent.Query<AccountInfo>(info => info.Account == request.Account);
				if (accountInfos.Count <= 0)
				{
					//查不到账号就注册，这里后面再改
					AccountInfosComponent accountInfosComponent = 
							session.GetComponent<AccountInfosComponent>() ??
							session.AddComponent<AccountInfosComponent>();
					AccountInfo accountInfo = accountInfosComponent.AddChild<AccountInfo>();
					accountInfo.Account = request.Account;
					accountInfo.Password = request.Password;
					await dbComponent.Save(accountInfo);
				}
				else
				{
					AccountInfo accountInfo = accountInfos[0];
					if (accountInfo.Password != request.Password)
					{
						response.Error = ErrorCode.ERR_LoginPasswordError;
						CloseSession(session).Coroutine();
						return;
					}
				}
			}
			
			
			// 获取起服配置中的gate列表，随机分配一个Gate
			StartSceneConfig config = RealmGateAddressHelper.GetGate(session.Zone(), request.Account);
			Log.Debug($"gate address: {config}");
			
			// 向gate请求一个key,客户端可以拿着这个key连接gate
			R2G_GetLoginKey r2GGetLoginKey = R2G_GetLoginKey.Create();
			r2GGetLoginKey.Account = request.Account;
			//没明白这个config.ActorId哪来的，
			//使用MessageSender进行服务器之间的通讯:会先判断是不是在同一个进程下，不是就先构建a2NetInner进行转发
			//总之，服务器之间通信，先拿到StartSceneConfig，然后拿到StartSceneConfig.ActorId进行发送
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

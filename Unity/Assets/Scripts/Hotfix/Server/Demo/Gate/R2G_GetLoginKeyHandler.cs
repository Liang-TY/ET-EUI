using System;


namespace ET.Server
{
	[MessageHandler(SceneType.Gate)]
	public class R2G_GetLoginKeyHandler : MessageHandler<Scene, R2G_GetLoginKey, G2R_GetLoginKey>
	{
		protected override async ETTask Run(Scene scene, R2G_GetLoginKey request, G2R_GetLoginKey response)
		{
			string key = RandomGenerator.RandInt64().ToString() + TimeInfo.Instance.ServerNow().ToString();
			//这里add的时候设置了20秒后移除，也就是这个登录令牌有效期20秒
			scene.GetComponent<GateSessionKeyComponent>().Add(key.GetLongHashCode(), request.Account);
			response.Key = key.GetLongHashCode();
			response.GateId = scene.Id;
			//这里await是为了async不报错
			await ETTask.CompletedTask;
		}
	}
}
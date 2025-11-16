namespace ET.Server
{
    [MessageHandler(SceneType.LoginCenter)]
    public class R2L_LoginAccountRequestHandler:MessageHandler<Scene,R2L_LoginAccountRequest,L2R_LoginAccountRequest>
    {
        protected override async ETTask Run(Scene scene, R2L_LoginAccountRequest request, L2R_LoginAccountRequest response)
        {
            long accountId = request.AccountName.GetLongHashCode();
            CoroutineLockComponent coroutineLockComponent = scene.GetComponent<CoroutineLockComponent>();
            //为什么这里还要accountId.GetHashCode()？
            using (await coroutineLockComponent.Wait(CoroutineLockType.LoginCenterLock,accountId.GetHashCode()) )
            {
                if (!scene.GetComponent<LoginInfoRecordComponent>().isExist(accountId))
                {
                    return;
                }

                int zone = scene.GetComponent<LoginInfoRecordComponent>().Get(accountId);
                StartSceneConfig gateConfig = RealmGateAddressHelper.GetGate(zone, request.AccountName);
                L2G_DisconnectGateUnit l2GDisconnectGateUnit = L2G_DisconnectGateUnit.Create();
                l2GDisconnectGateUnit.AccountName = request.AccountName;
                G2L_DisconnectGateUnit g2LDisconnectGateUnit = 
                        await scene.GetComponent<MessageSender>().Call(gateConfig.ActorId, l2GDisconnectGateUnit) 
                                as G2L_DisconnectGateUnit;
                response.Error = g2LDisconnectGateUnit.Error;
            }
        }
    }
}


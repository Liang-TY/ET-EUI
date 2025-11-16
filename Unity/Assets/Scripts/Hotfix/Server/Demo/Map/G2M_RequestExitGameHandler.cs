namespace ET.Server
{
    [MessageLocationHandler(SceneType.Map)]
    public class G2M_RequestExitGameHandler:MessageLocationHandler<Unit,G2M_RequestExitGame,M2G_RequestExitGame>
    {
        protected override async ETTask Run(Unit unit, G2M_RequestExitGame request, M2G_RequestExitGame response)
        {
            await ETTask.CompletedTask;
            //unit角色下线业务逻辑，然后保存unit及其组件数据到数据库
            
            //正式释放unit
            RemoveUnit(unit).Coroutine();
        }

        private async ETTask RemoveUnit(Unit unit)
        {
            await unit.Fiber().WaitFrameFinish();
            await unit.RemoveLocation(LocationType.Unit);
            UnitComponent unitComponent = unit.Root().GetComponent<UnitComponent>();
            unitComponent.Remove(unit.Id);
        }
    }
}


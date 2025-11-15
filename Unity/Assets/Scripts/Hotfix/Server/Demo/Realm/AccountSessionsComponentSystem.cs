namespace ET.Server
{
    [EntitySystemOf(typeof(AccountSessionsComponent))]
    [FriendOfAttribute(typeof(ET.Server.AccountSessionsComponent))]
    public static partial class AccountSessionsComponentSystem
    {
        [EntitySystem]
        private static void Awake(this AccountSessionsComponent self)
        {

        }
        [EntitySystem]
        private static void Destroy(this AccountSessionsComponent self)
        {
            self.AccountSessionDict.Clear();
        }

        public static Session Get(this AccountSessionsComponent self, string accountName)
        {
            if (!self.AccountSessionDict.TryGetValue(accountName, out EntityRef<Session> session))
            {
                return null;
            }

            return session;
        }
        public static void Add(this AccountSessionsComponent self, string accountName,EntityRef<Session> session)
        {
            if (!self.AccountSessionDict.ContainsKey(accountName))
            {
                self.AccountSessionDict[accountName] = session;
                return ;
            }

            self.AccountSessionDict.Add(accountName, session);
        }
        public static void Remove(this AccountSessionsComponent self, string accountName)
        {
            if (!self.AccountSessionDict.ContainsKey(accountName))
            {
                self.AccountSessionDict.Remove(accountName);
            }
        }
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        


    }
}


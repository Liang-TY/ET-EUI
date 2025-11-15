namespace ET.Server
{
    [EntitySystemOf(typeof(TokenComponent))]
    [FriendOfAttribute(typeof(ET.Server.TokenComponent))]
    public static partial class TokenComponentSystem
    {
        [EntitySystem]
        private static void Awake(this TokenComponent self)
        {

        }
        public static void Add(this TokenComponent self, string key, string token)
        {
            self.TokenDict.Add(key, token);
            self.TimeOutRemoveKey(key,token).Coroutine();
        }
        public static string Get(this TokenComponent self, string key)
        {
            string value = string.Empty;

            self.TokenDict.TryGetValue(key, out value);
            return value;

        }
        public static void Remove(this TokenComponent self, string key)
        {
            if (self.TokenDict.ContainsKey(key))
            {
                self.TokenDict.Remove(key);
            }

        }
        public static async ETTask TimeOutRemoveKey(this TokenComponent self, string key,string tokenKey)
        {
            await self.Root().GetComponent<TimerComponent>().WaitAsync(600000);
            string onlineToken = self.Get(key);
            if (!string.IsNullOrEmpty(onlineToken) && onlineToken == tokenKey)
            {
                self.Remove(key);
            }

        }
    }
}


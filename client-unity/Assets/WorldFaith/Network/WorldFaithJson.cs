using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace WorldFaith.Client.Network
{
    /// <summary>
    /// JSON helper dùng Newtonsoft (camelCase) — khớp ASP.NET Core server.
    /// JsonUtility của Unity không serialize property và không hỗ trợ camelCase.
    /// </summary>
    public static class WorldFaithJson
    {
        public static readonly JsonSerializerSettings Settings = new()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingValueHandling.Ignore
        };

        public static string Serialize<T>(T value) =>
            JsonConvert.SerializeObject(value, Settings);

        public static T? Deserialize<T>(string json) =>
            JsonConvert.DeserializeObject<T>(json, Settings);
    }
}

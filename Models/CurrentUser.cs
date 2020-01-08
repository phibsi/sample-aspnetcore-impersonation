using Newtonsoft.Json;

namespace Sample.AspNetCore.Impersonation.Models
{
    public class CurrentUserResult
    {
        [JsonProperty("d")]
        public CurrentUser D { get; set; }
    }

    public class CurrentUser
    {
        [JsonProperty("__metadata")]
        public Metadata Metadata { get; set; }
        
        [JsonProperty("Groups")]
        public Groups Groups { get; set; }
        
        [JsonProperty("Id")]
        public int Id { get; set; }
        
        [JsonProperty("IsHiddenInUI")]
        public bool IsHiddenInUi { get; set; }
        
        [JsonProperty("LoginName")]
        public string LoginName { get; set; }
        
        [JsonProperty("Title")]
        public string Title { get; set; }
        
        [JsonProperty("PrincipalType")]
        public int PrincipalType { get; set; }
        
        [JsonProperty("Email")]
        public string Email { get; set; }
        
        [JsonProperty("IsShareByEmailGuestUser")]
        public bool IsShareByEmailGuestUser { get; set; }
        
        [JsonProperty("IsSiteAdmin")]
        public bool IsSiteAdmin { get; set; }
        
        [JsonProperty("UserId")]
        public Userid UserId { get; set; }
    }

    public class Metadata
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        
        [JsonProperty("uri")]
        public string Uri { get; set; }
        
        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public class Groups
    {
        [JsonProperty("__deferred")]
        public Deferred Deferred { get; set; }
    }

    public class Deferred
    {
        [JsonProperty("uri")]
        public string Uri { get; set; }
    }

    public class Userid
    {
        [JsonProperty("__metadata")]
        public Metadata Metadata { get; set; }
        
        [JsonProperty("NameId")]
        public string NameId { get; set; }
        
        [JsonProperty("NameIdIssuer")]
        public string NameIdIssuer { get; set; }
    }
}

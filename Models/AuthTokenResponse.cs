using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace StellarBlueAssignment.Models{
    public class AuthTokenResponse{
        [JsonPropertyName("access_token")] 
        public string? Token { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        public DateTime ExpirationTime { get; set; }
    }
}
﻿namespace Braggerbk.Auth
{
    public class JwtSettings
    {
        public string Secret { get; set; }
        public int ExpiryMinutes { get; set; }
    }
}

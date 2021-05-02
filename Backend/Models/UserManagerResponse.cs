﻿using System;
using System.Collections.Generic;

namespace Server.Models
{
    public class UserManagerResponse
    {
        public string Message { get; set; }
        public bool IsSuccess { get; set; }
        public IEnumerable<string> Errors { get; set; }
        public DateTime? ExpireDate { get; set; }
        public object Model { get; set; }
    }
}

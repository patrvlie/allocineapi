﻿using System.Runtime.Serialization;

namespace AlloCine
{
    [DataContract(Name = "format")]
    public class Format
    {
        [DataMember(Name = "code")]
        public int Code { get; set; }

        [DataMember(Name = "$")]
        public string Value { get; set; }
    }
}


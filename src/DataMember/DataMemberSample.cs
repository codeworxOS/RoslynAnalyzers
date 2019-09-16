using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DataMember
{
    [DataContract]
    public class DataMemberSample
    {
        [DataMember(Order = 1)]
        public bool Property { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(Order = 2)]
        public bool Property2 { get; set; }

        public bool Property3 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool Property4 { get { return true; } set {} }

    }
}

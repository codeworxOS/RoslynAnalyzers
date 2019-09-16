using System.Runtime.Serialization;

namespace Codeworx.DataContract.Analyzers.Test
{
    [DataContract]
    public class SampleDto
    {
        [DataMember(Order = 4)]
        public int Id { get; set; }

        [DataMember(Order = 1)]
        public string FirstName { get; set; }

        [DataMember(Order = 2)]
        public string LastName { get; set; }
    }
}
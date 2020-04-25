using System.Collections.Generic;

namespace CharonX.Authorization.Gateway.Dto
{
    public class SetAnonymousDto
    {
        public string ServiceName { get; set; }
        public List<string> Urls { get; set; }
    }
}

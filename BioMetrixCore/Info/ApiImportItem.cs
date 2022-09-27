using System.Collections.Generic;

namespace BioMetrixCore.Info
{
    public class ApiImportItem
    {
        public string code { get; set; }
        public string time { get; set; }
    }

    public class LoginRequestCDS
    {
        public string tenantCode { set; get; }
        public string userName { set; get; }
        public string passWord { set; get; }
    }

    public class LoginResonseDataCDS
    {
        public string id { set; get; }
        public string userName { get; set; }
        public string fullName { set; get; }
        public string avatar { set; get;}
        public string token { set; get; }
        public bool isAdmin { set; get; }   
        public bool isCreateVote { set; get; }
        public string orgIds { set; get; }
        public string permissionParams { set; get; }
    }

    public class LoginResonseCDS
    {
        public LoginResonseDataCDS data { set; get; }
        public object message { set; get; }
        public string statusCode { set; get; }
    }

    public class GeneralResponse
    {
        public string statusCode { set; get; }
    }


}

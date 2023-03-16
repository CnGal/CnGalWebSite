namespace Gt3_server_csharp_aspnetcoremvc_bypass.Controllers.Sdk
{
    /*
     * sdk lib包的返回结果信息。
     * 
     * @author liuquan@geetest.com
     */
    public class GeetestLibResult
    {
        // 成功失败的标识码，1表示成功，0表示失败
        private int status = 0;

        // 返回数据，json格式
        private string data = "";

        // 备注信息，如异常信息等
        private string msg = "";

        public void SetStatus(int status)
        {
            this.status = status;
        }

        public int GetStatus()
        {
            return status;
        }

        public void SetData(string data)
        {
            this.data = data;
        }

        public string GetData()
        {
            return data;
        }

        public void SetMsg(string msg)
        {
            this.msg = msg;
        }

        public string GetMsg()
        {
            return msg;
        }

        public void SetAll(int status, string data, string msg)
        {
            SetStatus(status);
            SetData(data);
            SetMsg(msg);
        }

        public override string ToString()
        {
            return $"GeetestLibResult{{status={status}, data={data}, msg={msg}}}";
        }

    }
}

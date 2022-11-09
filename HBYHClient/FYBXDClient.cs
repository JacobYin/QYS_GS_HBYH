using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Genersoft.Platform.AppFramework.ClientService;
using Genersoft.Platform.AppFramework.Service;

namespace Genersoft.GS.HBYHCWClient
{
    public class FYBXDClient
    {
        private const string Assembly = "Genersoft.GS.HBYHCore";
        private const string ClassName = "Genersoft.GS.HBYHCore.HBYHInterfaceMgr";
        private GSPState state;
        private static FYBXDClient _Current = null;
        public FYBXDClient(GSPState state)
        {
            this.state = state;
        }
        public static FYBXDClient Current
        {
            get
            {
                if (_Current == null)
                {

                    _Current = new FYBXDClient(ClientContext.Current.FramworkState);
                }
                return _Current;
            }
        }
        public string GetDataSetByPro(string procedure, string paramStr1, string paramStr2, int num)
        {
            string str = RESTFulService.Invoke<string>(state, Assembly, ClassName,
                                                   "GenerateBill", false, new string[] { "IMGM", "IMGM" });
            return str;
        }
        public string PushData(string billID)
        {
            string str = RESTFulService.Invoke<string>(state, Assembly, ClassName,
                                                   "PushData", false, new string[] { billID});
            return str;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NineSocket.ServerHelper
{
    public class UtilityHelper
    {
        public static void RaiseEvent(EventHandler _event, object sender, EventArgs args = null)
        {
            if (_event != null)
            {
                _event(sender, args);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models.Params
{
    public class RefreshModel
    {
        public string RefreshToken { get; set; }
        public string Token { get; set; }
    }
}

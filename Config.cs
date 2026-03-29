using Exiled.API.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomSRole
{
    public class Config : IConfig
    {
        [Description("是否启用")]
        public bool IsEnabled { get; set; } = true;
        [Description("是否输出调试文本")]
        public bool Debug { get; set; } = false;
    }
}

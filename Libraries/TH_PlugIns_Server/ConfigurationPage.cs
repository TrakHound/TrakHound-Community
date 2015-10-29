using System;

using System.ComponentModel.Composition;
using System.Windows.Media;

namespace TH_PlugIns_Server
{
    [InheritedExport(typeof(ConfigurationPage))]
    public interface ConfigurationPage
    {

        string PageName { get; }

        ImageSource Image { get; }

    }
}

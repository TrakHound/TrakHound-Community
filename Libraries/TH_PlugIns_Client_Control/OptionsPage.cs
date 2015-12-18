using System;

using System.ComponentModel.Composition;
using System.Windows.Media;

namespace TH_PlugIns_Client
{
    [InheritedExport(typeof(OptionsPage))]
    public interface OptionsPage
    {

        string PageName { get; }

        ImageSource Image { get; }

    }
}

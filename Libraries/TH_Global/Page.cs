using System;

using System.Windows.Media;

namespace TH_Global
{
    /// <summary>
    /// Interface used for any general page use that needs a name and image
    /// </summary>
    public interface Page
    {

        string PageName { get; }

        ImageSource Image { get; }

    }
}

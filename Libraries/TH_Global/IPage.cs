using System;
using System.ComponentModel;
using System.Windows.Media;

namespace TH_Global
{
    /// <summary>
    /// Interface used for any general page use that needs a name and image
    /// </summary>
    public interface IPage
    {
        string Title { get; }

        ImageSource Image { get; }

        void Opened();
        bool Opening();

        void Closed();
        bool Closing();
    }

}
